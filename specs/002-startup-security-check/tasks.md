# Tasks: Startup Vulnerability Check

**Input**: Design documents from `/specs/002-startup-security-check/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ quickstart.md ✅

**Note**: Test tasks are included per the project constitution (Principle III — Test-First
Development). Tests MUST be written before implementation; each test MUST fail before
the corresponding implementation is written.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no incomplete-task dependencies)
- **[Story]**: Maps to user story in spec.md (US1 = P1, US2 = P2)

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: Shared infrastructure changes that both user stories depend on. Must be
complete before any story work begins.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T001 Add `public static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24)` constant and `_checkInProgress` concurrency guard (`private int _checkInProgress = 0;`) to `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`, and wrap the body of `RunCheckAsync` with `Interlocked.CompareExchange` skip-if-busy guard (return early if `_checkInProgress` is already 1; reset to 0 in `finally`)
- [x] T002 Update `Period` property in `src/Umbraco.SecurityDashboard/Scheduling/VulnerabilityCheckTask.cs` to return `VulnerabilityService.CheckInterval` instead of the hardcoded `TimeSpan.FromHours(24)`

**Checkpoint**: Foundational changes complete — `RunCheckAsync` is now concurrency-safe and the interval is defined in one place.

---

## Phase 2: User Story 1 — Catch Missed Checks After Extended Downtime (Priority: P1) 🎯 MVP

**Goal**: When the site starts up and the last successful check is more than 24 hours old
(or has never run), the vulnerability check fires automatically in the background.

**Independent Test**: Set `CheckedAt` on the latest `CheckResult` record to > 24 hours
ago (or delete all records), restart the site, and confirm a new `CheckResult` row is
written with a recent `CheckedAt` timestamp — without waiting for the 4 AM scheduled window.

### Tests for User Story 1 ⚠️ WRITE FIRST — must FAIL before implementation

- [x] T003 [P] [US1] Create `tests/Umbraco.SecurityDashboard.Tests/Scheduling/StartupVulnerabilityCheckHandlerTests.cs` and write these four unit tests using NSubstitute: (1) `HandleAsync_WhenNeverChecked_RunsCheck` — `GetLatestSuccessfulCheckAsync` returns null → `RunCheckAsync` is called; (2) `HandleAsync_WhenLastCheckOver24hAgo_RunsCheck` — `CheckedAt = UtcNow - 25h` → `RunCheckAsync` is called; (3) `HandleAsync_WhenLastCheckUnder24hAgo_SkipsCheck` — `CheckedAt = UtcNow - 23h` → `RunCheckAsync` is NOT called; (4) `HandleAsync_WhenLastCheckExactly24hAgo_RunsCheck` — `CheckedAt = UtcNow - 24h` → `RunCheckAsync` IS called (boundary: 24h old is not "within last 24 hours")
- [x] T004 [P] [US1] Add concurrency unit test `RunCheckAsync_WhenAlreadyRunning_SkipsSecondCall` to `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`: start one call (mock `GetUmbracoAdvisoriesAsync` to block briefly), fire a second concurrent call, confirm `GetUmbracoAdvisoriesAsync` is called exactly once

### Implementation for User Story 1

- [x] T005 [US1] Create `src/Umbraco.SecurityDashboard/Scheduling/StartupVulnerabilityCheckHandler.cs` implementing `INotificationAsyncHandler<UmbracoApplicationStartedNotification>`: inject `IVulnerabilityCheckRepository`, `IVulnerabilityService`, and `ILogger<StartupVulnerabilityCheckHandler>`; in `HandleAsync` — query `GetLatestSuccessfulCheckAsync()`, skip if result is not null and `CheckedAt >= UtcNow - VulnerabilityService.CheckInterval`, otherwise log and fire `_ = Task.Run(() => _vulnerabilityService.RunCheckAsync(CancellationToken.None))` (depends on T003, T004 tests failing first; depends on T001 for `CheckInterval`)
- [x] T006 [US1] Register the handler in `src/Umbraco.SecurityDashboard/Composers/SecurityDashboardComposer.cs` by adding `builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, StartupVulnerabilityCheckHandler>()` (depends on T005)

**Checkpoint**: User Story 1 is fully functional — startup triggers a check when data is stale or absent.

---

## Phase 3: User Story 2 — Graceful Handling During Startup Check (Priority: P2)

**Goal**: Confirm the startup check is non-blocking and that failure is recorded
consistently with scheduled check failures. No new implementation is required beyond
Phase 2 — this phase adds verification tests for behaviour already delivered.

**Independent Test**: The handler's `HandleAsync` method returns in under 50ms even when
the underlying check takes seconds (fire-and-forget confirmed). A simulated check failure
writes a `CheckResult` row with `Succeeded = false`.

### Tests for User Story 2 ⚠️ WRITE FIRST — must FAIL before implementation

- [x] T007 [P] [US2] Add `HandleAsync_IsNonBlocking_ReturnsBeforeCheckCompletes` to `tests/Umbraco.SecurityDashboard.Tests/Scheduling/StartupVulnerabilityCheckHandlerTests.cs`: mock `RunCheckAsync` with a `Task.Delay(500ms)` delay, time the `HandleAsync` call, assert it returns in under 100ms
- [x] T008 [P] [US2] Add `HandleAsync_WhenCheckFails_FailureIsRecordedViaService` to `tests/Umbraco.SecurityDashboard.Tests/Scheduling/StartupVulnerabilityCheckHandlerTests.cs`: mock `RunCheckAsync` to throw, confirm the exception is swallowed by the fire-and-forget wrapper and `HandleAsync` still completes (the underlying `RunCheckAsync` already persists failures internally)

### Implementation for User Story 2

No new production code is required. The `Task.Run` fire-and-forget in T005 satisfies
non-blocking startup (US2 scenario 1). The dashboard reading pre-existing DB rows is
unchanged behaviour (US2 scenario 2). Failure persistence is handled by the existing
`RunCheckAsync` catch block (US2 scenario 3).

**Checkpoint**: Both user stories are fully functional and tested independently.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [x] T009 Run `dotnet test tests/Umbraco.SecurityDashboard.Tests/` and confirm all new and existing tests pass
- [x] T010 [P] Verify `quickstart.md` scenarios manually against the running site: (1) stale-data restart triggers check, (2) fresh-data restart skips check, (3) concurrency guard prevents duplicate check

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 1)**: No dependencies — start immediately
- **User Story 1 (Phase 2)**: Depends on Phase 1 completion (needs `CheckInterval` constant and concurrency guard)
- **User Story 2 (Phase 3)**: Depends on Phase 2 completion (tests build on handler created in Phase 2)
- **Polish (Phase 4)**: Depends on Phases 1–3 complete

### Within Each Phase

- Tests (T003, T004, T007, T008) MUST be written first and MUST FAIL before implementation
- T005 depends on T003 and T004 (tests must exist and fail first), and on T001 (needs `CheckInterval`)
- T006 depends on T005

### Parallel Opportunities

- T003 and T004 can run in parallel (different test files)
- T007 and T008 can run in parallel (both add to the same test class, but are independent test methods — coordinate to avoid file conflicts)
- T009 and T010 can run in parallel

---

## Parallel Example: User Story 1

```
# Tests — write in parallel (different files):
T003: StartupVulnerabilityCheckHandlerTests.cs (new file)
T004: VulnerabilityServiceTests.cs (existing file — add new test method)

# After both test files are written and tests confirmed failing:
T005: StartupVulnerabilityCheckHandler.cs (new file)
T006: SecurityDashboardComposer.cs (small addition)
```

---

## Implementation Strategy

### MVP (User Story 1 only — 6 tasks)

1. Complete Phase 1: T001, T002
2. Write tests: T003, T004 (confirm they fail)
3. Implement: T005, T006
4. **STOP and VALIDATE**: `dotnet test` — all tests pass; manually verify startup check fires

### Full Delivery (both stories — 10 tasks total)

1. Complete MVP above
2. Add US2 tests: T007, T008 (confirm they fail — they should already pass given T005 implementation)
3. Polish: T009, T010
