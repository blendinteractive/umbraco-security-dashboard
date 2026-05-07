# Tasks: Webhook Notifications for Vulnerability Scan Results

**Feature**: `004-webhook-notifications`  
**Input**: Design documents from `/specs/004-webhook-notifications/`  
**Tech Stack**: C# / .NET 10 (Umbraco 17 LTS) — no new NuGet dependencies

**Tests**: Not requested in spec — no test tasks generated.

**Organization**: Tasks grouped by user story to enable independent implementation and verification.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no blocking dependencies)
- **[Story]**: User story this task belongs to (US1, US2, US3)

---

## Phase 1: Setup

**Purpose**: Local development configuration for manual end-to-end testing

- [X] T001 Add `Webhook` section with sample values (`SiteUrl`, `EndpointUrl`, `Secret`, `TimeoutSeconds`) to `demo/appsettings.Development.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core types that every user story phase depends on — must be complete before any story work begins

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T002 [P] Create `WebhookSettings` POCO with properties `SiteUrl` (`string?`), `EndpointUrl` (`string?`), `Secret` (`string?`), and `TimeoutSeconds` (`int`, default `10`) in `src/Umbraco.SecurityDashboard/Configuration/WebhookSettings.cs`
- [X] T003 Add `public WebhookSettings Webhook { get; set; } = new();` property to `SecurityDashboardSettings` in `src/Umbraco.SecurityDashboard/Configuration/SecurityDashboardSettings.cs`
- [X] T004 [P] Create `IWebhookNotifier` interface with signature `Task NotifyAsync(string overallStatus, DateTime checkedAt, IReadOnlyList<AdvisoryRecord> affectedAdvisories, CancellationToken cancellationToken = default)` in `src/Umbraco.SecurityDashboard/Services/IWebhookNotifier.cs`

**Checkpoint**: Foundation ready — user story phases can now proceed

---

## Phase 3: User Story 1 — Configure Webhook Endpoint (Priority: P1) 🎯 MVP

**Goal**: System reads webhook config from appsettings, validates the endpoint URL and site URL at startup, and silently skips dispatch when either value is absent or malformed.

**Independent Test**: Start the app with (a) a valid webhook URL, (b) an invalid URL (e.g., `"not-a-url"`), and (c) no URL at all — verify correct log messages in each case and that the app does not crash in any scenario.

### Implementation for User Story 1

- [X] T005 [US1] Create `WebhookNotifier` class skeleton in `src/Umbraco.SecurityDashboard/Services/WebhookNotifier.cs` implementing `IWebhookNotifier` — constructor accepts `IOptions<SecurityDashboardSettings>`, `IHttpClientFactory`, and `ILogger<WebhookNotifier>`; on construction, validate that `EndpointUrl` is a well-formed absolute `http://`/`https://` URI and that `SiteUrl` is non-empty; set an internal `_dispatchEnabled` flag to `false` and log a warning when either value is absent or invalid; `NotifyAsync` must return `Task.CompletedTask` immediately when `_dispatchEnabled` is `false`
- [X] T006 [US1] Register `IWebhookNotifier` as a singleton (`WebhookNotifier`) and add a named HTTP client `"WebhookNotifier"` with `AllowAutoRedirect = false` and a `Timeout` set from `WebhookSettings.TimeoutSeconds` (defaulting to 10 s) in `src/Umbraco.SecurityDashboard/Composers/SecurityDashboardComposer.cs`

**Checkpoint**: User Story 1 fully functional — configuration is recognized, invalid/missing values produce the correct log output, no crashes

---

## Phase 4: User Story 2 — Receive Scan Results via Webhook (Priority: P1)

**Goal**: Each completed vulnerability scan dispatches a POST request to the configured endpoint carrying the site URL, overall status, UTC timestamp, and affected package list.

**Independent Test**: With `demo/appsettings.Development.json` pointing `EndpointUrl` to a local request-capture tool (e.g., `https://webhook.site` or a local listener), trigger a scan and verify the POST body matches the contract in `specs/004-webhook-notifications/contracts/webhook-payload.md`.

### Implementation for User Story 2

- [X] T007 [P] [US2] Create `WebhookPayload` record with properties `SiteUrl` (`string`), `Status` (`string`), `CheckedAt` (`DateTime`), and `AffectedPackages` (`WebhookAffectedPackage[]`) in `src/Umbraco.SecurityDashboard/Models/Webhook/WebhookPayload.cs`
- [X] T008 [P] [US2] Create `WebhookAffectedPackage` record with properties `PackageName` (`string`), `InstalledVersion` (`string?`), `AdvisoryUrl` (`string`), and `Severity` (`string`) in `src/Umbraco.SecurityDashboard/Models/Webhook/WebhookAffectedPackage.cs`
- [X] T009 [US2] Implement `WebhookNotifier.NotifyAsync` in `src/Umbraco.SecurityDashboard/Services/WebhookNotifier.cs` — map `affectedAdvisories` to `WebhookAffectedPackage[]`; build a `WebhookPayload` using `WebhookSettings.SiteUrl`, `overallStatus`, `checkedAt`, and the mapped packages; serialize with `System.Text.Json` using `JsonNamingPolicy.CamelCase`; POST to `EndpointUrl` with `Content-Type: application/json`; add `X-Webhook-Secret` header only when `WebhookSettings.Secret` is non-empty; log success at Information level on a 2xx response
- [X] T010 [US2] Inject `IWebhookNotifier` into `VulnerabilityService` via constructor and call `await _webhookNotifier.NotifyAsync(overallStatus, checkResult.CheckedAt, affectedAdvisories, cancellationToken)` inside the success `try` block immediately after `SaveAdvisoriesAsync` completes in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs` — `affectedAdvisories` is the subset of `advisoryRecords` where `AffectedStatus == "Affected" || AffectedStatus == "Unknown"`; `overallStatus` mirrors the existing `affectedCount > 0 ? "Vulnerable" : "Safe"` logic

**Checkpoint**: User Story 2 fully functional — scan produces POST with correct JSON payload delivered to the configured endpoint

---

## Phase 5: User Story 3 — Graceful Failure Handling (Priority: P2)

**Goal**: A failed webhook dispatch (unreachable endpoint, timeout, non-2xx response, or redirect) never blocks or corrupts the scan result; every failure is logged with enough detail for an administrator to investigate.

**Independent Test**: Set `EndpointUrl` to an unreachable address (e.g., `http://localhost:19999/dead`), trigger a scan — verify the scan result is persisted normally and an error entry appears in the application log containing the endpoint URL and failure reason.

### Implementation for User Story 3

- [X] T011 [US3] Extend `WebhookNotifier.NotifyAsync` failure handling in `src/Umbraco.SecurityDashboard/Services/WebhookNotifier.cs` — after the POST, inspect the response: treat any non-2xx status code (including 3xx, since `AllowAutoRedirect` is `false`) as a failure and log an error with the endpoint URL and status code; wrap the entire HTTP call in a `try/catch` catching `TaskCanceledException` (timeout) and `HttpRequestException` (network error), logging an error with the endpoint URL and exception message in each case; ensure no exception propagates out of `NotifyAsync`
- [X] T012 [US3] Wrap the `_webhookNotifier.NotifyAsync(...)` call in its own inner `try/catch (Exception ex)` block in `VulnerabilityService.RunCheckAsync` in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs` — log the exception at Error level and continue; this ensures a webhook failure cannot abort the scan or prevent the result record from being returned to callers

**Checkpoint**: User Story 3 fully functional — scan pipeline is resilient to all webhook failure modes; results always persisted

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Security hardening and edge-case guards that apply across all stories

- [X] T013 Audit all log statements in `src/Umbraco.SecurityDashboard/Services/WebhookNotifier.cs` to confirm `WebhookSettings.Secret` is never included in any log message — use structured logging placeholders that reference endpoint URL and status only
- [X] T014 Add `TimeoutSeconds` guard in `WebhookNotifier` constructor in `src/Umbraco.SecurityDashboard/Services/WebhookNotifier.cs` — if `TimeoutSeconds ≤ 0`, log a warning and use 10 as the effective timeout when configuring the `HttpClient`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — **BLOCKS all user story phases**
- **US1 (Phase 3)**: Depends on Foundational (T002, T003, T004)
- **US2 (Phase 4)**: Depends on Foundational (T002–T004) and US1 (T005, T006)
- **US3 (Phase 5)**: Depends on US2 (T009, T010) — adds failure handling to code created in Phase 4
- **Polish (Phase 6)**: Depends on all story phases complete

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational — no other story dependencies
- **US2 (P1)**: Starts after US1 — extends `WebhookNotifier` stub created in US1
- **US3 (P2)**: Starts after US2 — adds error handling to the dispatch logic created in US2

### Within Each Phase

- Tasks marked `[P]` touch different files and can be worked in parallel
- T003 depends on T002 (needs the `WebhookSettings` type to exist)
- T009 depends on T007 and T008 (needs the payload models)
- T010 depends on T009 (needs `NotifyAsync` to be implemented)
- T011 and T012 can be worked in parallel (different files)

---

## Parallel Example: Foundational Phase

```
Parallel: T002 (WebhookSettings.cs) + T004 (IWebhookNotifier.cs)
Then sequential: T003 (add Webhook property to SecurityDashboardSettings — needs T002)
```

## Parallel Example: User Story 2

```
Parallel: T007 (WebhookPayload.cs) + T008 (WebhookAffectedPackage.cs)
Then sequential: T009 (NotifyAsync — needs T007, T008)
Then sequential: T010 (VulnerabilityService wiring — needs T009)
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

1. Complete Phase 1: Setup (T001)
2. Complete Phase 2: Foundational (T002–T004)
3. Complete Phase 3: US1 (T005–T006)
4. Complete Phase 4: US2 (T007–T010)
5. **STOP and VALIDATE**: Trigger a scan against a live capture endpoint, verify payload matches contract
6. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → base types and interface ready
2. US1 → webhook config is read, validated, and gracefully disabled when absent
3. US2 → scan produces correct POST payload (**MVP deliverable**)
4. US3 → resilience added; scan never blocked by webhook failure
5. Polish → secret never logged; timeout guard in place

---

## Notes

- No new NuGet packages required — `HttpClient`, `System.Text.Json`, and `IHttpClientFactory` are available via existing Umbraco/BCL references
- No database schema changes — all entities are in-memory configuration and runtime models
- `Secret` must never appear in logs — treat it as a credential throughout
- `AllowAutoRedirect = false` is a security requirement (Decision 6 in research.md)
- `[P]` tasks touch different files and have no shared dependencies within the phase
