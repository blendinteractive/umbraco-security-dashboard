# Implementation Plan: Webhook Notifications for Vulnerability Scan Results

**Branch**: `004-webhook-notifications` | **Date**: 2026-05-05 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/004-webhook-notifications/spec.md`

## Summary

Add outbound webhook dispatch to the existing vulnerability scan pipeline. When a scan completes successfully, the system posts a JSON payload — containing the site URL, exposure status, and a list of affected packages (with severity and advisory reference) — to an administrator-configured endpoint. Configuration is entirely via appsettings; no new NuGet packages are needed. A shared secret header and per-request timeout are optional configuration values.

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: Umbraco.Cms 17.x (existing), `IHttpClientFactory` / `HttpClient` (BCL), `System.Text.Json` (BCL), `Microsoft.Extensions.Options` (transitively available via Umbraco)  
**Storage**: No schema changes — no new tables or migrations required  
**Testing**: xUnit 2.x + NSubstitute 5.x (existing test project)  
**Target Platform**: Umbraco 17 back-office package (server-side, .NET 10)  
**Project Type**: Umbraco CMS package / class library  
**Performance Goals**: Webhook POST completes within 5 seconds under normal network conditions; scan adds at most `TimeoutSeconds` (default 10s) of latency when the endpoint is unresponsive  
**Constraints**: Synchronous dispatch (blocks scan completion); no retry; timeout defaults to 10 seconds  
**Scale/Scope**: Single endpoint per installation; one dispatch per scan cycle (approximately daily)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| I. Clean Code & Simplicity | PASS | `WebhookNotifier` has a single responsibility. `IWebhookNotifier` interface keeps the scan service testable. No deep inheritance. |
| II. Minimal External Dependencies | PASS | No new NuGet packages. `HttpClient`, `System.Text.Json`, `IHttpClientFactory`, `IOptions<T>` are all BCL / already transitively referenced. |
| III. Test-First Development | PASS | Unit tests for `WebhookNotifier` (all dispatch paths) and updated `VulnerabilityService` tests (notifier called after successful scan) must be written before implementation. |
| IV. Umbraco UX Consistency | N/A | No UI changes in this feature. |
| V. Security by Design | PASS | Secret value is never logged (validated in test). No redirect following (`AllowAutoRedirect = false`). Config is validated at startup. |

All gates pass. No Complexity Tracking entries required.

## Project Structure

### Documentation (this feature)

```text
specs/004-webhook-notifications/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── contracts/
│   └── webhook-payload.md  # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit-tasks — not created here)
```

### Source Code Changes

```text
src/Umbraco.SecurityDashboard/
├── Configuration/
│   └── SecurityDashboardSettings.cs        MODIFY — add Webhook property of type WebhookSettings
│   └── WebhookSettings.cs                  NEW
├── Models/
│   └── Webhook/
│       └── WebhookPayload.cs               NEW — WebhookPayload + WebhookAffectedPackage records
├── Services/
│   ├── IWebhookNotifier.cs                 NEW
│   ├── WebhookNotifier.cs                  NEW
│   └── VulnerabilityService.cs             MODIFY — inject IWebhookNotifier; call after SaveAdvisoriesAsync
└── Composers/
    └── SecurityDashboardComposer.cs        MODIFY — register WebhookNotifier + named "WebhookNotifier" HttpClient

tests/Umbraco.SecurityDashboard.Tests/
└── Services/
    ├── WebhookNotifierTests.cs             NEW
    └── VulnerabilityServiceTests.cs        MODIFY — add tests for webhook dispatch call
```

**Structure Decision**: Single-project layout (existing). No new projects. Server-side only — no frontend changes.

---

## Implementation Details

### 1. `WebhookSettings.cs` (new)

```
namespace Umbraco.SecurityDashboard.Configuration;

public class WebhookSettings
{
    public string? SiteUrl { get; set; }
    public string? EndpointUrl { get; set; }
    public string? Secret { get; set; }
    public int TimeoutSeconds { get; set; } = 10;
}
```

### 2. `SecurityDashboardSettings.cs` (modify)

Add property:

```
public WebhookSettings Webhook { get; set; } = new();
```

### 3. `WebhookPayload.cs` (new, in `Models/Webhook/`)

```
namespace Umbraco.SecurityDashboard.Models.Webhook;

public record WebhookPayload(
    string SiteUrl,
    string Status,
    DateTime CheckedAt,
    IReadOnlyList<WebhookAffectedPackage> AffectedPackages);

public record WebhookAffectedPackage(
    string PackageName,
    string? InstalledVersion,
    string AdvisoryUrl,
    string Severity);
```

### 4. `IWebhookNotifier.cs` (new)

```
namespace Umbraco.SecurityDashboard.Services;

public interface IWebhookNotifier
{
    Task NotifyAsync(
        string overallStatus,
        DateTime checkedAt,
        IReadOnlyList<AdvisoryRecord> advisoryRecords,
        CancellationToken cancellationToken = default);
}
```

### 5. `WebhookNotifier.cs` (new)

Key responsibilities:
- Validate `WebhookSettings` at construction/first call; skip if `EndpointUrl` or `SiteUrl` is absent/invalid.
- Use `overallStatus` directly (`"Vulnerable"` or `"Safe"`) — no mapping layer needed.
- Filter `advisoryRecords` to those with `AffectedStatus == "Affected"` or `"Unknown"`.
- Build `WebhookPayload` and serialise with `System.Text.Json` camelCase options.
- Create the named HTTP client; set `Timeout` from `WebhookSettings.TimeoutSeconds`; disable `AllowAutoRedirect`.
- Add `X-Webhook-Secret` header only when `Secret` is non-null/non-empty.
- POST and inspect response; log success at Information, non-2xx/network failure at Error.
- Never log the `Secret` value.
- Swallow all exceptions after logging (does not propagate).

### 6. `VulnerabilityService.cs` (modify)

Constructor change: add `IWebhookNotifier webhookNotifier` parameter.

In `RunCheckAsync`, after `await _repository.SaveAdvisoriesAsync(advisoryRecords)`:

```csharp
var affectedForWebhook = advisoryRecords
    .Where(r => r.AffectedStatus == "Affected" || r.AffectedStatus == "Unknown")
    .ToList();

try
{
    await _webhookNotifier.NotifyAsync(overallStatus, checkResult.CheckedAt, affectedForWebhook, cancellationToken);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Webhook notification failed.");
}
```

Where `overallStatus` is determined from `advisoryRecords` using the same logic already used for the log message (count of affected > 0 → `"Vulnerable"`, else `"Safe"`).

### 7. `SecurityDashboardComposer.cs` (modify)

Register:

```csharp
builder.Services.AddSingleton<IWebhookNotifier, WebhookNotifier>();

builder.Services.AddHttpClient("WebhookNotifier", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Umbraco-SecurityDashboard/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false
});
```

---

## Test Plan

### `WebhookNotifierTests.cs` (new)

| # | Scenario | Assert |
|---|---|---|
| W01 | Endpoint URL not configured | `NotifyAsync` returns without making any HTTP call |
| W02 | Site URL not configured | `NotifyAsync` returns without making any HTTP call |
| W03 | Valid config, no affected packages | POST sent; body `status = "Safe"`, `affectedPackages = []` |
| W04 | Valid config, one affected package | POST sent; body `status = "Vulnerable"`, package fields populated correctly |
| W05 | Secret configured | `X-Webhook-Secret` header present with correct value |
| W06 | No secret configured | `X-Webhook-Secret` header absent |
| W07 | Endpoint returns 4xx | Exception swallowed; error logged; no throw |
| W08 | Endpoint times out | Exception swallowed; error logged; no throw |
| W09 | Endpoint returns 3xx (redirect) | Treated as failure; error logged |
| W10 | `overallStatus = "Vulnerable"` | Payload `status = "Vulnerable"` |
| W11 | `overallStatus = "Safe"` | Payload `status = "Safe"` |
| W13 | Valid config, scan just completed | Payload `checkedAt` matches the `CheckResultRecord.CheckedAt` timestamp |
| W12 | Secret never appears in log output | Verified via log capture |

### `VulnerabilityServiceTests.cs` (add tests)

| # | Scenario | Assert |
|---|---|---|
| VS-W01 | Successful scan with affected advisories | `IWebhookNotifier.NotifyAsync` called once with correct status and records |
| VS-W02 | Successful scan with no advisories | `IWebhookNotifier.NotifyAsync` called with `"Safe"` and empty list |
| VS-W03 | Webhook notifier throws | Exception is caught; scan result still saved; no rethrow |
| VS-W04 | Scan fails (GitHub client throws) | `IWebhookNotifier.NotifyAsync` NOT called |

Use `NSubstitute` mocks for `IWebhookNotifier`. All existing tests must remain green.
