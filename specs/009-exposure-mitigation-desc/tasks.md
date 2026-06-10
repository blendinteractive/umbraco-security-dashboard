# Tasks: Exposure Check Mitigation Descriptions

**Input**: Design documents from `/specs/009-exposure-mitigation-desc/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/advisory-api.md ✓, quickstart.md ✓

**Tests**: TDD is required per Constitution Principle III (gated). Tests must be written and confirmed failing before implementing description logic, evaluator combining, and DTO mapping.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no blocking dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2)

---

## Phase 1: Setup

**Purpose**: Confirm the existing test baseline before any changes.

- [X] T001 Run `dotnet test tests/Umbraco.SecurityDashboard.Tests` and confirm 0 failures as a baseline before making any changes

**Checkpoint**: Baseline confirmed — safe to begin foundational changes.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: New return-type records, interface signature changes, compile-stub implementations, AdvisoryRecord property, and DB migration. Must be fully complete before any user story work begins.

**⚠️ CRITICAL**: Interface changes in T004–T005 will break compilation of all existing implementations. T006–T009 are compile stubs that restore a buildable state. No feature logic yet — descriptions will be null stubs until Phase 3.

- [X] T002 [P] Create `ExposureCheckResult` record with `(ExposureVerdict Verdict, string? MitigationDescription = null)` in `src/Umbraco.SecurityDashboard/Services/Exposure/ExposureCheckResult.cs`
- [X] T003 [P] Create `ExposureEvaluationResult` record with `(string Verdict, string? MitigationDescription)` in `src/Umbraco.SecurityDashboard/Services/Exposure/ExposureEvaluationResult.cs`
- [X] T004 Update `IExposureCheck.CheckAsync` return type from `Task<ExposureVerdict>` to `Task<ExposureCheckResult>` in `src/Umbraco.SecurityDashboard/Services/Exposure/IExposureCheck.cs`
- [X] T005 Update `IExposureCheckEvaluator.EvaluateAsync` return type from `Task<string>` to `Task<ExposureEvaluationResult>` in `src/Umbraco.SecurityDashboard/Services/Exposure/IExposureCheckEvaluator.cs`
- [X] T006 [P] Update `ContentDeliveryApiExposureCheck.CheckAsync` compile stub: wrap existing verdict in `new ExposureCheckResult(verdict, null)` (description placeholder — full description added in T017) in `src/Umbraco.SecurityDashboard/Services/Exposure/Checks/ContentDeliveryApiExposureCheck.cs`
- [X] T007 [P] Update `NonAdminUsersExposureCheck.CheckAsync` compile stub: wrap existing verdict in `new ExposureCheckResult(verdict, null)` (description placeholder — full description added in T018) in `src/Umbraco.SecurityDashboard/Services/Exposure/Checks/NonAdminUsersExposureCheck.cs`
- [X] T008 Update `ExposureCheckEvaluator.EvaluateAsync` compile stub: adapt to collect `ExposureCheckResult` from checks, determine worst verdict as before, and return `new ExposureEvaluationResult(verdict, null)` (description joining added in T019) in `src/Umbraco.SecurityDashboard/Services/Exposure/ExposureCheckEvaluator.cs`
- [X] T009 Update `VulnerabilityService` call site(s) of `EvaluateAsync` to destructure `ExposureEvaluationResult` — use `.Verdict` for existing logic, ignore `.MitigationDescription` for now (storage wired in T026) in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`
- [X] T010 [P] Add `public string? ExposureCheckDescription { get; set; }` property to `AdvisoryRecord` in `src/Umbraco.SecurityDashboard/Models/Db/AdvisoryRecord.cs`
- [X] T011 Create `AddExposureCheckDescriptionColumn` migration class that runs `ALTER TABLE SecurityDashboard_Advisory ADD ExposureCheckDescription NVARCHAR(MAX) NULL` in `src/Umbraco.SecurityDashboard/Migrations/AddExposureCheckDescriptionColumn.cs`
- [X] T012 Register `AddExposureCheckDescriptionColumn` as step `SecurityDashboard-1.3.0` in `SecurityDashboardMigrationPlan.DefinePlan()` in `src/Umbraco.SecurityDashboard/Migrations/SecurityDashboardMigrationPlan.cs`
- [X] T013 Run `dotnet build` and confirm 0 errors — all compile stubs must be in place before proceeding

**Checkpoint**: Foundation complete — build passes, implementations are stubs, ready for TDD phases.

---

## Phase 3: User Story 1 — View Why Vulnerability Is Mitigated (Priority: P1) 🎯 MVP

**Goal**: A security admin sees a plain-language description explaining why an advisory is auto-mitigated (e.g., "Content Delivery API is disabled"), shown below the package list in the dashboard.

**Independent Test**: Run a scan where at least one exposure check returns `Mitigated` → view advisory in dashboard → confirm description is displayed and the manual mitigation display is unaffected.

### Tests for User Story 1 ⚠️ Write FIRST — confirm FAIL before implementing

- [X] T014 [P] [US1] Add failing tests for `ExposureCheckEvaluator` description combining: (a) single Mitigated check with description → description returned; (b) two Mitigated checks → descriptions joined with `"; "`; (c) Mitigated check with null description → fallback `"Mitigated by exposure check"`; (d) Vulnerable verdict → `MitigationDescription` is null in `tests/Umbraco.SecurityDashboard.Tests/Services/Exposure/ExposureCheckEvaluatorTests.cs`
- [X] T015 [P] [US1] Add failing tests verifying `ContentDeliveryApiExposureCheck.CheckAsync` returns `MitigationDescription == "Content Delivery API is disabled"` when Mitigated, and `MitigationDescription == null` for NotAffected/Vulnerable verdicts in `tests/Umbraco.SecurityDashboard.Tests/Services/Exposure/Checks/ContentDeliveryApiExposureCheckTests.cs`
- [X] T016 [P] [US1] Add failing tests verifying `NonAdminUsersExposureCheck.CheckAsync` returns `MitigationDescription == "All backoffice users are administrators"` when Mitigated, and `MitigationDescription == null` for other verdicts in `tests/Umbraco.SecurityDashboard.Tests/Services/Exposure/Checks/NonAdminUsersExposureCheckTests.cs`
- [X] T021 [US1] Add failing test verifying `VulnerabilityService.GetDashboardStatusAsync` maps `AdvisoryRecord.ExposureCheckDescription` → `AdvisoryDto.ExposureCheckMitigationDescription`; confirm null `ExposureCheckDescription` maps to null DTO field in `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs` (create file if it does not exist)

### Implementation for User Story 1

- [X] T017 [P] [US1] Update `ContentDeliveryApiExposureCheck.CheckAsync` Mitigated path to return `new ExposureCheckResult(ExposureVerdict.Mitigated, "Content Delivery API is disabled")`; other paths remain `new ExposureCheckResult(verdict, null)` in `src/Umbraco.SecurityDashboard/Services/Exposure/Checks/ContentDeliveryApiExposureCheck.cs`
- [X] T018 [P] [US1] Update `NonAdminUsersExposureCheck.CheckAsync` Mitigated path to return `new ExposureCheckResult(ExposureVerdict.Mitigated, "All backoffice users are administrators")`; other paths remain `new ExposureCheckResult(verdict, null)` in `src/Umbraco.SecurityDashboard/Services/Exposure/Checks/NonAdminUsersExposureCheck.cs`
- [X] T019 [US1] Implement `ExposureCheckEvaluator.EvaluateAsync` description combining: (1) collect non-null `.MitigationDescription` from all results where `Verdict == Mitigated`; (2) join with `"; "`; (3) if joined string is empty apply fallback `"Mitigated by exposure check"`; (4) set description to null when worst verdict is not Mitigated; return `new ExposureEvaluationResult(verdict, description)` in `src/Umbraco.SecurityDashboard/Services/Exposure/ExposureCheckEvaluator.cs`
- [X] T020 [P] [US1] Add `public string? ExposureCheckMitigationDescription { get; set; }` to `AdvisoryDto` in `src/Umbraco.SecurityDashboard/Models/Api/AdvisoryDto.cs`
- [X] T022 [US1] Update `VulnerabilityService.GetDashboardStatusAsync` to set `dto.ExposureCheckMitigationDescription = record.ExposureCheckDescription` when building each `AdvisoryDto` in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`
- [X] T023 [P] [US1] Add `exposureCheckMitigationDescription?: string` to the `AdvisoryDto` TypeScript interface in `client/src/types.ts`
- [X] T024 [US1] Update `advisory-item.element.ts` to render a `.mitigation-attribution` block labeled `"Auto-mitigated"` with `.attribution-description` containing `exposureCheckMitigationDescription` when `affectedStatus === "Mitigated"` and `manualMitigation` is absent; no change to existing manual mitigation display path in `client/src/components/advisory-item.element.ts`

**Checkpoint**: Run `dotnet test` — T014–T016 and T021 tests now pass. User Story 1 is fully functional and independently testable.

---

## Phase 4: User Story 2 — Mitigation Description Persisted Across Page Loads (Priority: P2)

**Goal**: Descriptions are stored in `SecurityDashboard_Advisory.ExposureCheckDescription` during scan save so they are available on subsequent dashboard loads without re-running exposure checks.

**Independent Test**: Complete a scan, navigate away, return to dashboard — descriptions are still shown. Force a new scan where exposure check now returns Vulnerable — description is no longer shown for that advisory.

### Tests for User Story 2 ⚠️ Write FIRST — confirm FAIL before implementing

- [ ] T025 [US2] Add failing test verifying that when `VulnerabilityService` saves scan results, `ExposureEvaluationResult.MitigationDescription` is assigned to `AdvisoryRecord.ExposureCheckDescription` before the record is written to the DB in `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`

### Implementation for User Story 2

- [ ] T026 [US2] Update `VulnerabilityService` scan-save path: after receiving `ExposureEvaluationResult` from `EvaluateAsync`, set `advisoryRecord.ExposureCheckDescription = evaluationResult.MitigationDescription` before inserting/updating the record in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`

**Checkpoint**: Run `dotnet test` — T025 passes. Navigate away from dashboard and return; descriptions are still shown (read from DB, not recomputed).

---

## Phase 5: Polish & Cross-Cutting Concerns

- [ ] T027 Run `dotnet test tests/Umbraco.SecurityDashboard.Tests` and confirm 0 failures across all test files
- [ ] T028 [P] Build frontend to confirm TypeScript compiles with no errors: run `npm run build` (or `vite build`) in `client/`
- [ ] T029 [P] Validate end-to-end via demo app per quickstart.md: `dotnet run --project src/Umbraco.SecurityDashboard.Demo`, trigger a manual scan, and confirm that (a) a Mitigated advisory shows its description, (b) a manually mitigated advisory still shows the manual description unchanged

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — run immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — **blocks all user story phases**
- **Phase 3 (US1)**: Depends on Phase 2 completion — no dependency on Phase 4
- **Phase 4 (US2)**: Depends on Phase 2 and Phase 3 (VulnerabilityService changes built on T009 and T022)
- **Phase 5 (Polish)**: Depends on Phase 3 and Phase 4 both complete

### Within Phase 2

```
T002, T003 → parallel (new files, no deps)
T004 → after T002 (IExposureCheck uses ExposureCheckResult)
T005 → after T003 (IExposureCheckEvaluator uses ExposureEvaluationResult)
T006, T007 → parallel, after T004 (check stubs implement IExposureCheck)
T008 → after T004 + T005 (evaluator stub uses ExposureCheckResult + ExposureEvaluationResult)
T009 → after T005 + T008 (VulnerabilityService calls EvaluateAsync)
T010 → parallel to T004–T009 (AdvisoryRecord property is independent)
T011 → after T010 (migration adds the column AdvisoryRecord maps to)
T012 → after T011 (registers the migration class)
T013 → after T006–T012 (build confirmation)
```

### Within Phase 3

```
T014, T015, T016, T021 → parallel (different test files, different concerns)
T017, T018 → parallel, after T014–T016 tests are written (different check files)
T019 → after T017 + T018 (evaluator uses results from both checks)
T020 → parallel to T017–T019 (different file: AdvisoryDto)
T022 → after T019 + T020 (VulnerabilityService maps from AdvisoryRecord + into AdvisoryDto)
T021 test → must FAIL before T022; T022 makes it pass
T023 → parallel to T019–T022 (different file: TypeScript types)
T024 → after T023 (frontend uses updated TypeScript types)
```

### Within Phase 4

```
T025 (test) → write first, confirm fails
T026 → after T025, after T022 (VulnerabilityService already updated for read; now add write path)
```

---

## Parallel Example: Phase 3 Tests (Launch All Together)

```bash
# Write all failing tests in parallel (different files):
Task: T014 — ExposureCheckEvaluatorTests.cs (description combining + fallback)
Task: T015 — ContentDeliveryApiExposureCheckTests.cs (description when Mitigated)
Task: T016 — NonAdminUsersExposureCheckTests.cs (description when Mitigated)
Task: T021 — VulnerabilityServiceTests.cs (DTO mapping test)

# Then implement the two check descriptions in parallel:
Task: T017 — ContentDeliveryApiExposureCheck.cs
Task: T018 — NonAdminUsersExposureCheck.cs
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Baseline confirmation
2. Complete Phase 2: Foundational (CRITICAL — blocks everything)
3. Complete Phase 3: User Story 1 (full display pipeline)
4. **STOP and VALIDATE**: Description is shown in dashboard for Mitigated advisories
5. Polish with Phase 5

### Incremental Delivery

1. Setup + Foundational → build compiles, stubs in place
2. US1 complete → admin can see descriptions (MVP)
3. US2 complete → descriptions persist across page loads (full feature)
4. Polish → all tests green, frontend builds, demo validated

---

## Summary

| Phase | Tasks | Parallel Opportunities |
|-------|-------|------------------------|
| Setup | 1 (T001) | — |
| Foundational | 12 (T002–T013) | T002/T003, T006/T007, T010 |
| US1 (P1) | 11 (T014–T024) | T014/T015/T016/T021, T017/T018, T020/T023 |
| US2 (P2) | 2 (T025–T026) | — |
| Polish | 3 (T027–T029) | T028/T029 |
| **Total** | **29** | — |
