# Tasks: Security Audit Log (007)

**Input**: Design documents from `/specs/007-audit-log/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Included — constitution check mandates test-first development; unit tests for repository and state-change detection must be written before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on in-progress tasks)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (DB Schema + Core Types)

**Purpose**: Create the new table, POCO, and repository interface that every user story depends on. No user story work can begin until this phase is complete.

- [X] T001 [P] Create `AuditLogRecord` NPoco POCO in `src/Umbraco.SecurityDashboard/Models/Db/AuditLogRecord.cs` with columns: `Id`, `Timestamp`, `OverallStatus`, `ActionType`, `ActorName`, `Description`; annotate with `[TableName("SecurityDashboard_AuditLog")]` and `[PrimaryKey("Id")]`
- [X] T002 [P] Create `IAuditLogRepository` interface in `src/Umbraco.SecurityDashboard/Services/IAuditLogRepository.cs` with `Task AppendAsync(AuditLogRecord record)` and `Task<AuditLogPage> GetPagedAsync(int skip, int take)`; define `AuditLogPage(IReadOnlyList<AuditLogRecord> Entries, int TotalCount)` record in the same file
- [X] T003 Create `AddAuditLogTable` migration class in `src/Umbraco.SecurityDashboard/Migrations/AddAuditLogTable.cs` that creates `SecurityDashboard_AuditLog` with all columns from the data model and adds index `IX_SecurityDashboard_AuditLog_Timestamp` on `(Timestamp DESC)`
- [X] T004 Update `src/Umbraco.SecurityDashboard/Migrations/SecurityDashboardMigrationPlan.cs` to add `.To<AddAuditLogTable>("SecurityDashboard-1.2.0")` step in the migration chain

**Checkpoint**: DB schema, POCO, and interface defined. Repository tests and implementation can now begin.

---

## Phase 2: Foundational (Repository + DI)

**Purpose**: Repository implementation and DI registration that every user story needs to write or read audit entries.

**⚠️ CRITICAL**: Write tests before implementation (constitution requirement).

- [X] T005 Write unit tests (test-first) for `AuditLogRepository` covering: `AppendAsync` inserts a record, `GetPagedAsync` returns correct page and `TotalCount`, `GetPagedAsync` returns empty page when no records exist; create `tests/Umbraco.SecurityDashboard.Tests/Services/AuditLogRepositoryTests.cs`
- [X] T006 Implement `AuditLogRepository` in `src/Umbraco.SecurityDashboard/Services/AuditLogRepository.cs` using NPoco + `IScopeProvider`; `AppendAsync` inserts via `db.InsertAsync`; `GetPagedAsync` runs two queries — one for the page (`ORDER BY Timestamp DESC LIMIT/OFFSET`) and one `COUNT(*)`; make T005 tests pass
- [X] T007 Register `IAuditLogRepository` → `AuditLogRepository` as scoped in `src/Umbraco.SecurityDashboard/Composers/SecurityDashboardComposer.cs`

**Checkpoint**: `AuditLogRepository` fully functional, tests passing, registered in DI. All user stories can now be implemented independently.

---

## Phase 3: User Story 1 - Administrator Reviews Audit History (Priority: P1) 🎯 MVP

**Goal**: Expose a paginated `GET /audit-log` endpoint and render the history in a Lit component below the advisory list.

**Independent Test**: Seed the `SecurityDashboard_AuditLog` table with sample rows and verify: (1) the API returns them newest-first with all required fields, (2) the Lit component renders the table with correct columns, (3) an empty table shows the empty-state message.

### Tests for User Story 1 ⚠️ Write FIRST — ensure they FAIL before implementation

- [X] T008 [P] [US1] Write unit tests for `GET /audit-log` controller endpoint: returns 200 with `AuditLogPageResponse`, maps repository results to DTOs, clamps `take` to 100, returns empty `entries` and `totalCount: 0` when log is empty; create/extend `tests/Umbraco.SecurityDashboard.Tests/Controllers/SecurityDashboardControllerAuditTests.cs`

### Implementation for User Story 1

- [X] T009 [P] [US1] Create `AuditLogEntryDto` response model in `src/Umbraco.SecurityDashboard/Models/Api/AuditLogEntryDto.cs` with properties: `Id`, `Timestamp`, `OverallStatus`, `ActionType`, `ActorName`, `Description`
- [X] T010 [P] [US1] Create `AuditLogPageResponse` response model in `src/Umbraco.SecurityDashboard/Models/Api/AuditLogPageResponse.cs` with properties: `IReadOnlyList<AuditLogEntryDto> Entries`, `int TotalCount`
- [X] T011 [US1] Add `GET /audit-log` action to `src/Umbraco.SecurityDashboard/Controllers/SecurityDashboardController.cs`: inject `IAuditLogRepository`; accept `skip` (default 0) and `take` (default 25) query params; clamp `take` to 100; map `AuditLogRecord` → `AuditLogEntryDto`; return `AuditLogPageResponse`; make T008 tests pass
- [X] T012 [P] [US1] Add `AuditLogEntryDto` and `AuditLogPageResponse` TypeScript interface types to `client/src/types.ts` matching the API contract (`timestamp: string`, `overallStatus: 'Safe' | 'Mitigated' | 'Vulnerable' | 'NeverChecked'`, `actionType: 'Manual' | 'Automatic'`, `actorName: string | null`)
- [X] T013 [US1] Create `audit-log.element.ts` Lit element in `client/src/components/audit-log.element.ts`: fetches `GET /audit-log?skip=0&take=25` on connect; renders `uui-box` containing `uui-table` with columns Timestamp, Status, Type, Actor, Description; shows `uui-loader` while loading; shows empty-state paragraph when `totalCount === 0`; implements `uui-pagination` using `totalCount` and current `skip`
- [X] T014 [US1] Import and render `<security-audit-log>` element below the advisory list in `client/src/security-dashboard.element.ts`

**Checkpoint**: `GET /audit-log` returns data; Lit component shows paginated history; empty-state renders correctly.

---

## Phase 4: User Story 2 - System Logs Automatic State Changes (Priority: P1)

**Goal**: When `VulnerabilityService.RunCheckAsync()` causes the overall vulnerability state to change, write an `Automatic` audit entry.

**Independent Test**: Mock `IAuditLogRepository`, trigger `RunCheckAsync()` with a state change, verify `AppendAsync` is called with `ActionType = "Automatic"` and the correct `OverallStatus`; trigger `RunCheckAsync()` with no state change, verify `AppendAsync` is NOT called.

### Tests for User Story 2 ⚠️ Write FIRST — ensure they FAIL before implementation

- [X] T015 [P] [US2] Write unit tests for `VulnerabilityService` state-change detection and audit logging: state changes (NeverChecked→any, Safe→Vulnerable, Vulnerable→Safe, Vulnerable→Mitigated) produce one `AppendAsync` call with correct fields; no state change produces zero `AppendAsync` calls; create `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceAuditTests.cs`

### Implementation for User Story 2

- [X] T016 [US2] Add `Task<string> GetCurrentOverallStatusAsync()` to `IVulnerabilityService` in `src/Umbraco.SecurityDashboard/Services/IVulnerabilityService.cs` (or the interface file as it currently exists); extract the existing inline status-computation from `GetDashboardStatusAsync()` into this new method in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`
- [X] T017 [US2] Modify `VulnerabilityService.RunCheckAsync()` in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`: call `GetCurrentOverallStatusAsync()` before saving new results to capture `previousStatus`; after saving, compute `newStatus`; if `previousStatus != newStatus`, call `IAuditLogRepository.AppendAsync` with `ActionType = "Automatic"`, `ActorName = null`, `Description = "Scheduled vulnerability check completed"`, `OverallStatus = newStatus`, `Timestamp = DateTime.UtcNow`; make T015 tests pass

**Checkpoint**: `RunCheckAsync()` writes exactly one audit entry per state change and zero entries when state is unchanged.

---

## Phase 5: User Story 3 - System Logs Manual Mitigation Changes (Priority: P1)

**Goal**: `POST` and `DELETE /advisories/{ghsaId}/mitigations` always write a `Manual` audit entry with the actor's name and current overall status, regardless of whether the action changes the overall state.

**Independent Test**: As a known back-office user, POST a mitigation — verify an audit entry is created with `ActionType = "Manual"` and the user's name. Then DELETE it — verify another audit entry is created. Verify entries are created even when the overall state does not change.

### Tests for User Story 3 ⚠️ Write FIRST — ensure they FAIL before implementation

- [X] T018 [P] [US3] Write unit tests for `SecurityDashboardController` mitigation audit logging: POST mitigation always calls `AppendAsync` with `ActionType = "Manual"` and actor name from `IBackOfficeSecurityAccessor`; DELETE mitigation always calls `AppendAsync` with correct actor; actor name is pulled from `BackOfficeSecurity.CurrentUser.Name`; extend `tests/Umbraco.SecurityDashboard.Tests/Controllers/SecurityDashboardControllerAuditTests.cs`

### Implementation for User Story 3

- [X] T019 [US3] Inject `IAuditLogRepository` and `IBackOfficeSecurityAccessor` into `SecurityDashboardController` constructor in `src/Umbraco.SecurityDashboard/Controllers/SecurityDashboardController.cs`
- [X] T020 [US3] Modify `POST /advisories/{ghsaId}/mitigations` in `src/Umbraco.SecurityDashboard/Controllers/SecurityDashboardController.cs`: after saving the mitigation, call `GetCurrentOverallStatusAsync()` to get current status, then call `IAuditLogRepository.AppendAsync` with `ActionType = "Manual"`, `ActorName = BackOfficeSecurity.CurrentUser.Name`, `Description = $"Marked {ghsaId} as mitigated"`, `OverallStatus = currentStatus`, `Timestamp = DateTime.UtcNow`; make T018 tests pass
- [X] T021 [US3] Modify `DELETE /advisories/{ghsaId}/mitigations` in `src/Umbraco.SecurityDashboard/Controllers/SecurityDashboardController.cs`: after removing the mitigation, call `GetCurrentOverallStatusAsync()` to get current status, then call `IAuditLogRepository.AppendAsync` with `ActionType = "Manual"`, `ActorName = BackOfficeSecurity.CurrentUser.Name`, `Description = $"Removed mitigation for {ghsaId}"`, `OverallStatus = currentStatus`, `Timestamp = DateTime.UtcNow`; make T018 tests pass

**Checkpoint**: Every POST and DELETE to `/mitigations` writes an audit entry with full actor attribution; entries appear in the audit history view from US1.

---

## Phase 6: User Story 4 - Webhook Fires on Every Overall State Change (Priority: P2)

**Goal**: The configured update webhook fires exactly once per overall state change — whether triggered by an automatic check or a manual action — and never fires when the state is unchanged.

**Independent Test**: Configure a stub webhook endpoint; trigger: (1) an automatic state change → webhook fires once; (2) a manual action that changes state → webhook fires once; (3) a manual action that does not change state → webhook does NOT fire.

### Tests for User Story 4 ⚠️ Write FIRST — ensure they FAIL before implementation

- [ ] T022 [P] [US4] Write unit tests for conditional webhook firing: `RunCheckAsync()` with state change calls `IWebhookNotifier.NotifyAsync`; `RunCheckAsync()` with no state change does NOT call notifier; POST/DELETE mitigation that changes overall state calls notifier; POST/DELETE mitigation that does not change state does NOT call notifier; extend `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceAuditTests.cs` and `tests/Umbraco.SecurityDashboard.Tests/Controllers/SecurityDashboardControllerAuditTests.cs`

### Implementation for User Story 4

- [ ] T023 [US4] Modify `VulnerabilityService.RunCheckAsync()` in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`: move the existing unconditional `IWebhookNotifier.NotifyAsync()` call inside the `previousStatus != newStatus` branch so it fires only on actual state changes; pass empty advisories list; make T022 tests pass
- [ ] T024 [US4] Inject `IWebhookNotifier` into `SecurityDashboardController` in `src/Umbraco.SecurityDashboard/Controllers/SecurityDashboardController.cs`: after writing the audit entry in POST and DELETE mitigation actions, compare `overallStatus` before the action (captured before save) to `overallStatus` after (captured via `GetCurrentOverallStatusAsync()`); if they differ, call `IWebhookNotifier.NotifyAsync()` with an empty advisories list; make T022 tests pass

**Checkpoint**: Webhook fires for 100% of state changes and 0% of non-state-changing actions.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T025 [P] Verify `take` clamping: add or confirm a test in `tests/Umbraco.SecurityDashboard.Tests/Controllers/SecurityDashboardControllerAuditTests.cs` that `take=200` is silently reduced to 100 before calling the repository
- [ ] T026 [P] Add Swashbuckle XML doc comment to the `GET /audit-log` action in `src/Umbraco.SecurityDashboard/Controllers/SecurityDashboardController.cs` so it appears correctly in the OpenAPI output (matching the existing doc comment style on other endpoints)
- [ ] T027 Run `dotnet test tests/Umbraco.SecurityDashboard.Tests/` and fix any regressions introduced by the VulnerabilityService and controller modifications

---

## Dependencies

```
Phase 1 (T001–T004)
  └── Phase 2 (T005–T007)
        ├── Phase 3 / US1 (T008–T014)   [can start independently]
        ├── Phase 4 / US2 (T015–T017)   [can start independently]
        ├── Phase 5 / US3 (T018–T021)   [depends on US2 for GetCurrentOverallStatusAsync — T016]
        └── Phase 6 / US4 (T022–T024)   [depends on US2 (T017) and US3 (T020–T021)]
              └── Phase 7 / Polish (T025–T027)
```

**Story independence notes**:
- US1 (audit history view) is fully independent once the repository is in place — it only reads.
- US2 (automatic logging) requires `GetCurrentOverallStatusAsync` (T016) before US3 can inject the controller.
- US3 (manual logging) calls `GetCurrentOverallStatusAsync` added in T016, so T016 must be complete before T020–T021.
- US4 (webhook) extends the state-change logic built in US2 (T017) and US3 (T020–T021).

---

## Parallel Execution Examples

### After Phase 2 completes, these stories can proceed in parallel:

**Terminal 1 — US1 (Audit History View)**:
```
T008 → T009 (parallel with T010) → T011 → T012 (parallel) → T013 → T014
```

**Terminal 2 — US2 (Automatic Logging)**:
```
T015 → T016 → T017
```

Then once T016 is done, US3 can begin:

**Terminal 2 continued — US3 (Manual Logging)**:
```
T018 → T019 → T020 → T021
```

Then US4:
```
T022 → T023 → T024
```

---

## Implementation Strategy

**MVP Scope** (deliver US1 + US2 first for fastest value):

1. Complete Phase 1 + Phase 2 (foundation: ~4 tasks)
2. Complete Phase 4 / US2 (automatic logging: ~3 tasks) — this produces real data in the log
3. Complete Phase 3 / US1 (audit history view: ~7 tasks) — this makes the data visible
4. Complete Phase 5 / US3 (manual logging: ~4 tasks) — adds user attribution
5. Complete Phase 6 / US4 (conditional webhook: ~3 tasks) — extends webhook behaviour

This order ensures every phase delivers something independently verifiable before moving to the next.
