# Research: Security Audit Log (007)

## 1. State-Change Detection in `VulnerabilityService.RunCheckAsync()`

**Decision**: Compute the previous overall status immediately before the new check result is saved, using a new `GetCurrentOverallStatusAsync()` helper extracted from the existing `GetDashboardStatusAsync()` logic.

**Rationale**: `GetDashboardStatusAsync()` already contains the full status-computation algorithm. Extracting the "overall status string" sub-task into a reusable method avoids duplication, satisfies Constitution Principle I (single responsibility), and gives the controller the same capability for wrap-around state comparison on mitigation actions.

**Alternatives considered**:
- Storing the last-known status in a cached field on `VulnerabilityService` — rejected because it would not survive application restarts and introduces mutable shared state.
- Adding a `PreviousStatus` column to `CheckResultRecord` — rejected because it creates a derived value in the DB that the code must maintain in sync.

**Implementation**: Add `Task<string> GetCurrentOverallStatusAsync()` to `IVulnerabilityService` and `VulnerabilityService`. In `RunCheckAsync()`, call it before saving the new `CheckResultRecord`, then compare to the newly-computed `overallStatus` after saving advisories.

---

## 2. Webhook Firing on Manual Mitigation Changes

**Decision**: Inject `IWebhookNotifier` directly into `SecurityDashboardController`. After a successful `CreateMitigation` or `DeleteMitigation`, compare the overall status before and after the change; fire the webhook only when it differs.

**Rationale**: The controller already controls the timing of the mitigation write. Adding webhook dispatch there keeps the logic local to the state-changing operation. Passing an empty advisories list to `NotifyAsync()` is acceptable — the webhook is a state-change signal; the recipient can call the status API for full advisory details if needed. This avoids adding repository dependencies to the notifier.

**Alternatives considered**:
- Creating an `AuditService` wrapper that owns both audit writes and webhook dispatch — rejected as premature abstraction given the small number of call sites.
- Modifying `WebhookNotifier.NotifyAsync()` to accept no advisories parameter — rejected to avoid breaking the existing call site in `VulnerabilityService`; passing an empty list is sufficient.

---

## 3. Pagination Strategy (SQLite + SQL Server Compatibility)

**Decision**: Use `LIMIT`/`OFFSET` SQL via NPoco's parameterised `Fetch<T>(sql, skip, take)` for the paged query; return `COUNT(*)` in a second query for `TotalCount`.

**Rationale**: Both SQLite and SQL Server support `LIMIT`/`OFFSET` (SQL Server via `OFFSET … FETCH NEXT` which NPoco's `Fetch` abstracts). Two queries (one for the page, one for the count) are simpler than a single combined query and remain correct under Umbraco's multi-DB abstraction.

**Alternatives considered**:
- Keyset pagination (cursor-based) — rejected as over-engineering for expected volumes (hundreds to low-thousands of entries).
- Returning all entries and slicing in memory — rejected because it violates the ≤ 2 s performance goal for large logs.

**Page size default**: 25 entries per page, exposed as `take` query parameter (max 100 enforced server-side).

---

## 4. Actor Identity Storage

**Decision**: Store `ActorName` as a `VARCHAR(500)` string captured at write time from `IBackOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Name`. Do not store user ID.

**Rationale**: The spec states "The audit entry retains the actor name as it was at the time of the action" (edge case: deleted accounts). Storing the display name directly satisfies this without a JOIN on a potentially deleted user record. 500 characters matches the existing `MitigatedBy` column in `SecurityDashboard_ManualMitigation`.

---

## 5. Audit Entry Description Templates

**Decision**: Descriptions are constructed at the call site — not stored as enum codes — using simple interpolated strings.

| Trigger | Description |
|---------|-------------|
| Scheduled vulnerability check (state change) | `"Scheduled vulnerability check completed"` |
| User marks mitigation for `{ghsaId}` | `"Marked {ghsaId} as mitigated"` |
| User removes mitigation for `{ghsaId}` | `"Removed mitigation for {ghsaId}"` |

---

## 6. Threat Model (Constitution Principle V)

| Threat | Mitigation |
|--------|------------|
| Back-office user tampers with or deletes audit entries | No UPDATE/DELETE endpoints exposed; repository has only `AppendAsync` + `GetPagedAsync` |
| Unauthenticated access to audit history | `[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]` enforced on controller |
| Audit entry flood from rapid repeated actions | No rate-limiting required at this scale; entries are bounded by user actions + daily scheduled check |
| Actor name containing HTML/script | Output-escaped by Lit's `html` template literal; stored as plain text |
