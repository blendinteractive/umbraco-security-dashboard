# Data Model: Webhook Notifications

**Feature**: `004-webhook-notifications` | **Date**: 2026-05-05

No database schema changes are required. All entities in this feature are in-memory configuration and runtime models.

---

## Configuration Entities

### `WebhookSettings`

Nested POCO bound from `Umbraco:SecurityDashboard:Webhook` in appsettings.

| Property | Type | Default | Description |
|---|---|---|---|
| `SiteUrl` | `string?` | `null` | The public-facing URL of the site, included verbatim in every payload. Required for dispatch; absent value disables webhook. |
| `EndpointUrl` | `string?` | `null` | The URL to POST to. Required for dispatch; absent value disables webhook. |
| `Secret` | `string?` | `null` | Optional. When set, sent as the `X-Webhook-Secret` request header. Must not be logged. |
| `TimeoutSeconds` | `int` | `10` | How long (in seconds) to wait for the endpoint to respond before treating the request as failed. |

**Validation rules**:
- If `EndpointUrl` is configured it MUST be a well-formed absolute `http://` or `https://` URI. Invalid values cause a startup warning and disable dispatch.
- If `SiteUrl` is configured it MUST be a non-empty string. If absent, dispatch is disabled and a startup warning is logged.
- `TimeoutSeconds` MUST be > 0. Values ≤ 0 are treated as invalid; a warning is logged and the default (10) is used.
- `Secret` is optional; no format validation is applied.

**Hosting location**: Embedded inside the existing `SecurityDashboardSettings` class as a property `Webhook` of type `WebhookSettings`.

---

## Payload Models

### `WebhookPayload`

Runtime record constructed by `WebhookNotifier` and serialised to JSON for the POST body.

| Property | JSON key | Type | Description |
|---|---|---|---|
| `SiteUrl` | `siteUrl` | `string` | Value from `WebhookSettings.SiteUrl`. |
| `Status` | `status` | `string` | `"Vulnerable"` when affected advisories exist; `"Safe"` otherwise. Matches `VulnerabilityService`'s internal status values directly — no mapping needed. |
| `CheckedAt` | `checkedAt` | `DateTime` (UTC) | The UTC timestamp of the completed scan, taken from `CheckResultRecord.CheckedAt`. |
| `AffectedPackages` | `affectedPackages` | `WebhookAffectedPackage[]` | Empty array when status is `"Safe"`. |

### `WebhookAffectedPackage`

An entry in `WebhookPayload.AffectedPackages`.

| Property | JSON key | Type | Description |
|---|---|---|---|
| `PackageName` | `packageName` | `string` | NuGet package identifier (e.g., `Umbraco.Cms`). |
| `InstalledVersion` | `installedVersion` | `string?` | Version string detected on the server; `null` when unknown. |
| `AdvisoryUrl` | `advisoryUrl` | `string` | URL to the GitHub Security Advisory (GHSA) page. |
| `Severity` | `severity` | `string` | `Critical` \| `High` \| `Moderate` \| `Low`. |

**Source mapping** (from existing `AdvisoryRecord`):
- `PackageName` ← `AdvisoryRecord.PackageName`
- `InstalledVersion` ← `AdvisoryRecord.InstalledVersion`
- `AdvisoryUrl` ← `AdvisoryRecord.AdvisoryUrl`
- `Severity` ← `AdvisoryRecord.Severity`
- Only records with `AffectedStatus == "Affected"` or `AffectedStatus == "Unknown"` are included (mirrors the dashboard's "affected" grouping).

---

## Service Entities

### `IWebhookNotifier`

Interface for the outbound webhook dispatcher.

```
Task NotifyAsync(
    string overallStatus,
    DateTime checkedAt,
    IReadOnlyList<AdvisoryRecord> affectedAdvisories,
    CancellationToken cancellationToken = default
)
```

- `overallStatus`: the internal status string from `VulnerabilityService` (`"Vulnerable"` | `"Safe"`). Used directly in the payload without remapping.
- `affectedAdvisories`: only the affected/unknown records, already filtered by the caller.
- Returns `Task`; any delivery failure is swallowed after logging (does not propagate to the caller).

### `WebhookNotifier`

Concrete implementation of `IWebhookNotifier`. Singleton. Dependencies:

| Dependency | How resolved |
|---|---|
| `IOptions<SecurityDashboardSettings>` | Options pattern; holds `WebhookSettings` |
| `IHttpClientFactory` | Creates the named `"WebhookNotifier"` client |
| `ILogger<WebhookNotifier>` | Structured logging |
