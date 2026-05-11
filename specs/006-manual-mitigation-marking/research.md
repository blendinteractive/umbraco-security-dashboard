# Research: Manual Vulnerability Mitigation Marking

## Decision 1: Mitigation storage key — GhsaId, not AdvisoryRecord.Id

**Decision**: Key `SecurityDashboard_ManualMitigation` rows on `GhsaId` (VARCHAR 50), not on the `SecurityDashboard_Advisory.Id` foreign key.

**Rationale**: Advisory records are deleted and recreated on each vulnerability rescan (`DeleteAdvisoriesForCheckAsync` + `SaveAdvisoriesAsync`). A FK to `SecurityDashboard_Advisory.Id` would be invalidated every 24 hours. `GhsaId` is stable — it is assigned by GitHub and never changes for a given advisory.

**Alternatives considered**: Storing mitigation on the advisory row directly (adds a Mitigated-nullable column cluster to the advisory table). Rejected because advisory rows are ephemeral; mitigation records must survive rescans.

---

## Decision 2: Separate `IMitigationRepository`, not extending `IVulnerabilityCheckRepository`

**Decision**: Create a dedicated `IMitigationRepository` / `MitigationRepository`.

**Rationale**: `IVulnerabilityCheckRepository` already has a clear single responsibility (vulnerability check results and advisory scan data). Mitigations are a distinct domain concept with a different lifecycle. Extending the existing interface would violate the constitution's single-responsibility principle.

**Alternatives considered**: Adding mitigation methods to `IVulnerabilityCheckRepository`. Rejected to preserve separation of concerns.

---

## Decision 3: Current-user identity via `IBackOfficeSecurityAccessor`

**Decision**: Inject `IBackOfficeSecurityAccessor` into the controller. Use `.BackOfficeSecurity?.CurrentUser?.Name` to get the display name at request time and store it as a string in the `MitigatedBy` column.

**Rationale**: `IBackOfficeSecurityAccessor` is the canonical Umbraco way to access the current backoffice user in a non-async context. It is already available in Umbraco 17 without additional setup. Storing the display name as a string (not a user ID FK) is explicitly required by the spec: if the admin is later deleted, the record must still show "who" performed the action.

**Alternatives considered**:
- `HttpContext.User` claims directly — works but `IBackOfficeSecurityAccessor` is more semantically correct in the Umbraco ecosystem.
- Storing user ID with a live lookup — rejected; spec explicitly states the stored name remains visible even after the user is deleted.

---

## Decision 4: One active mitigation per GhsaId (UNIQUE constraint)

**Decision**: Add a `UNIQUE` index on `GhsaId` in `SecurityDashboard_ManualMitigation`. The API returns 409 Conflict if a mitigation already exists for that GhsaId.

**Rationale**: The spec requires "only one active manual mitigation record per advisory at a time" and "marking an already-mitigated advisory again is not permitted." A DB-level constraint enforces this even under concurrent requests (last-write-wins is specified only for the concurrent case, but the UNIQUE constraint prevents duplicate rows cleanly and surfaces a clear conflict).

**Alternatives considered**: Application-level check only (upsert). Rejected because a DB constraint is more robust under concurrent writes.

---

## Decision 5: Frontend dialog — inline `<uui-dialog>` element, not UmbModalManagerContext

**Decision**: Use an inline `<uui-dialog>` rendered conditionally within `advisory-item.element.ts` (or a child `mitigation-dialog.element.ts`) controlled by a `@state` boolean.

**Rationale**: The UMB modal manager requires async context acquisition and is better suited for application-level flows. For a simple per-row dialog that is tightly coupled to a single advisory item, an inline conditional `<uui-dialog>` is simpler, easier to test, and keeps the component self-contained. `<uui-dialog>` is available in `@umbraco-ui/uui` (already in the dependency tree) with no additional imports.

**Alternatives considered**: `UmbModalManagerContext` — more powerful but involves context wiring and separate modal definitions, which is overkill for a two-step confirmation dialog.

---

## Decision 6: Refresh strategy after mitigation mutation

**Decision**: After a successful mark/remove mutation, emit a custom DOM event (`mitigation-changed`) that bubbles up to `security-dashboard.element.ts`, which re-calls `_fetchStatus()`.

**Rationale**: The dashboard already re-fetches all status data on load. Re-using this path (rather than optimistically patching local state) ensures the UI always reflects server truth. SC-003 requires status updates without a full page reload — re-fetching the API response satisfies this without a browser refresh.

**Alternatives considered**: Optimistic local state update — rejected because it risks showing stale data if the server request fails.

---

## Decision 7: Description field length — 2000 characters

**Decision**: Store `Description` as `NVARCHAR(2000)` in the database and validate max 2000 chars on the server.

**Rationale**: Matches the existing `ErrorMessage` column length convention in this project. Long enough for any meaningful mitigation description; short enough to avoid unbounded text storage.

**Alternatives considered**: `NVARCHAR(MAX)` — unnecessary; `VARCHAR(500)` — too short for a detailed description.
