# Research: Webhook Notifications

**Feature**: `004-webhook-notifications` | **Date**: 2026-05-05

## Decision 1: Site URL Source

**Decision**: Require the administrator to configure the site's primary URL explicitly as part of the webhook settings in appsettings (e.g., `Umbraco:SecurityDashboard:Webhook:SiteUrl`).

**Rationale**: The vulnerability check runs as a background service, outside any HTTP request context. Umbraco's hosting context, `IHttpContextAccessor`, and public URL detection all depend on a live HTTP request or DNS resolution, making them unreliable in a background job. An explicit configuration value is the only approach that is guaranteed to work in all hosting scenarios (reverse-proxied, multi-tenant, containerised). It also gives administrators full control over what URL is reported to external systems.

**Alternatives considered**:
- `IHttpContextAccessor.HttpContext?.Request.Host` — unreliable; null in background services.
- `IWebHostEnvironment.ApplicationName` — server name, not the public-facing URL.
- `Umbraco.Cms.Core.Hosting.IHostingEnvironment.ApplicationVirtualPath` — not a full URL.

---

## Decision 2: HttpClient Pattern

**Decision**: Register a dedicated named HTTP client `"WebhookNotifier"` via `IHttpClientFactory`, injected into `WebhookNotifier` as `IHttpClientFactory`. Configure the 10-second default timeout on the `HttpClient` instance.

**Rationale**: The project already uses the named `HttpClient` pattern for `"GitHubAdvisories"`. Consistency with this pattern is required by the Constitution (Principle I: Clean Code). `IHttpClientFactory` manages connection pooling and lifetime correctly; creating `HttpClient` directly in a singleton is an anti-pattern.

**Alternatives considered**:
- Reusing the `"GitHubAdvisories"` client — rejected: different headers, different timeout, different failure semantics.
- Injecting `HttpClient` directly — rejected: singleton `HttpClient` has DNS staleness issues.

---

## Decision 3: Secret Header Name

**Decision**: Use `X-Webhook-Secret` as the header name when a shared secret is configured.

**Rationale**: Simple, self-documenting, and widely understood. The spec chose the shared-secret-header approach (Option D from clarification). HMAC signing (used by `X-Hub-Signature-256`) was not selected, so there is no reason to use GitHub's header name.

**Alternatives considered**:
- `Authorization: Bearer <secret>` — conventionally for bearer tokens; a shared webhook secret is not a bearer token.
- `X-Hub-Signature-256` — implies HMAC payload signing, which is not what is implemented.

---

## Decision 4: Status Vocabulary in Webhook Payload

**Decision**: The webhook payload uses `"Vulnerable"` and `"Safe"` directly — the same values used internally by `VulnerabilityService`. No translation layer is needed.

**Rationale**: Spec updated via clarification (2026-05-05) to align with internal code. Using the same strings eliminates a mapping step and prevents bugs where a mapping diverges from the internal logic.

---

## Decision 5: Payload Serialisation

**Decision**: Use `System.Text.Json` with camelCase property naming (`JsonSerializerOptions` with `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`) to serialise the POST body.

**Rationale**: `System.Text.Json` is part of the .NET BCL — no new dependency. CamelCase JSON is the standard for webhook payloads consumed by JavaScript/TypeScript tooling and most modern HTTP clients.

**Alternatives considered**:
- `Newtonsoft.Json` — not needed; BCL is sufficient.
- PascalCase — non-standard for JSON APIs; excluded.

---

## Decision 6: Redirect Handling

**Decision**: Disable automatic redirect following on the `"WebhookNotifier"` HTTP client (`AllowAutoRedirect = false`). Any redirect response (3xx) is treated as a failed delivery and logged.

**Rationale**: Webhook endpoints should not redirect. Following a redirect could silently deliver the payload to an unintended URL. The Constitution's Security by Design principle (V) requires treating external interactions conservatively.

---

## Decision 7: Dispatch Integration Point

**Decision**: `IWebhookNotifier.NotifyAsync(...)` is called from `VulnerabilityService.RunCheckAsync()` immediately after `SaveAdvisoriesAsync` completes successfully, inside the existing `try` block. Any exception from the notifier is caught separately (inner try/catch) so that a webhook failure does not prevent the check result from being committed.

**Rationale**: The spec (FR-007) requires the scan to complete and persist regardless of webhook outcome. Dispatching inside the success path (after persistence) ensures the database is always consistent. The separate try/catch for the notifier call isolates webhook failures from scan failures.

---

## Decision 8: No New NuGet Dependencies

**Decision**: No new NuGet packages are required for this feature.

**Rationale**: `HttpClient`, `System.Text.Json`, and `IHttpClientFactory` are all in the .NET 10 BCL or `Microsoft.Extensions.*` packages already referenced transitively via Umbraco. The Constitution (Principle II) requires justifying every new dependency; none are needed here.
