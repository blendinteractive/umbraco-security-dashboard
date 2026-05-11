# Tasks: Manual Vulnerability Mitigation Marking

**Input**: Design documents from `/specs/006-manual-mitigation-marking/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/api.md ✅, quickstart.md ✅

**Tests**: Included per constitution (Test-First Development principle; integration tests use real DB).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (US1, US2)
- Exact file paths are included in all task descriptions

---

## Phase 1: Setup (Data Layer Foundation)

**Purpose**: Create the database model and migration that all other work depends on.

- [ ] T001 Create ManualMitigationRecord NPoco POCO in `src/Umbraco.SecurityDashboard/Models/Db/ManualMitigationRecord.cs`
- [ ] T002 Create AddManualMitigationTable migration (Id, GhsaId UNIQUE, Description, MitigatedAt, MitigatedBy) in `src/Umbraco.SecurityDashboard/Migrations/AddManualMitigationTable.cs`
- [ ] T003 Register migration step `From("SecurityDashboard-1.0.0").To<AddManualMitigationTable>("SecurityDashboard-1.1.0")` in `src/Umbraco.SecurityDashboard/Migrations/SecurityDashboardMigrationPlan.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Repository interface, implementation, and DI registration — shared by both user stories. All story work blocks on this phase.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T004 [P] Create IMitigationRepository interface (GetAllMitigationsAsync, CreateMitigationAsync, DeleteMitigationAsync) in `src/Umbraco.SecurityDashboard/Services/IMitigationRepository.cs`
- [ ] T005 [P] Add integration tests for all three IMitigationRepository methods (real DB, IScopeProvider) in `tests/Umbraco.SecurityDashboard.Tests/Services/MitigationRepositoryTests.cs`
- [ ] T006 Implement MitigationRepository using NPoco + IScopeProvider; CreateMitigationAsync throws on UNIQUE violation (409 path); DeleteMitigationAsync returns bool in `src/Umbraco.SecurityDashboard/Services/MitigationRepository.cs`
- [ ] T007 Register `IMitigationRepository → MitigationRepository` as `AddScoped` in `src/Umbraco.SecurityDashboard/Composers/SecurityDashboardComposer.cs`

**Checkpoint**: Foundation ready — both user stories can now be implemented independently.

---

## Phase 3: User Story 1 — Mark Vulnerability as Mitigated (Priority: P1) 🎯 MVP

**Goal**: An administrator can click "Mark As Mitigated" on a Vulnerable or Unknown advisory, enter a description, and confirm — the advisory immediately shows as Mitigated with full attribution (who, when, description).

**Independent Test**: Mark any advisory in Vulnerable or Unknown state → confirm status changes to Mitigated with stored attribution; attempt empty description → confirm submission is blocked.

### Tests for User Story 1 ⚠️ Write FIRST — ensure they FAIL before implementing T012–T018

- [ ] T008 [P] [US1] Add mitigation overlay unit test cases (Vulnerable→Mitigated, Unknown→Mitigated, NotAffected unaffected, overlay attribute population) in `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`
- [ ] T009 [P] [US1] Add CreateMitigation controller unit tests (201 with description, 400 empty description, 409 duplicate, 401 unauthorised) in `tests/Umbraco.SecurityDashboard.Tests/Controllers/MitigationControllerTests.cs`

### Implementation for User Story 1

- [ ] T010 [P] [US1] Create ManualMitigationDto (Description, MitigatedAt, MitigatedBy) in `src/Umbraco.SecurityDashboard/Models/Api/ManualMitigationDto.cs`
- [ ] T011 [P] [US1] Create CreateMitigationRequest with `[Required][StringLength(2000, MinimumLength = 1)]` on Description in `src/Umbraco.SecurityDashboard/Models/Api/CreateMitigationRequest.cs`
- [ ] T012 [US1] Add nullable `ManualMitigationDto? ManualMitigation` field to AdvisoryDto in `src/Umbraco.SecurityDashboard/Models/Api/AdvisoryDto.cs`
- [ ] T013 [US1] Implement mitigation overlay in `VulnerabilityService.GetDashboardStatusAsync()`: fetch all mitigations, join by GhsaId, set AffectedStatus="Mitigated" and populate ManualMitigation, recalculate affectedCount/mitigatedCount/overallStatus in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`
- [ ] T014 [US1] Add `[HttpPost("advisories/{ghsaId}/mitigations")]` endpoint: validate description, get current user via IBackOfficeSecurityAccessor, create record, return 201/400/409 in `src/Umbraco.SecurityDashboard/Controllers/SecurityDashboardController.cs`
- [ ] T015 [P] [US1] Add `ManualMitigationDto` interface and `manualMitigation: ManualMitigationDto | null` to `AdvisoryDto` in `client/src/types.ts`
- [ ] T016 [US1] Create `security-dashboard-mitigation-dialog` Lit element with `@property() mode: 'mark' | 'remove'` and `@property() ghsaId: string`; mark mode renders uui-textarea for description; remove mode renders confirmation prompt; both modes emit `mitigation-changed` on success or `mitigation-cancelled` on cancel in `client/src/components/mitigation-dialog.element.ts`
- [ ] T017 [US1] Add "Mark As Mitigated" uui-button (visible for Vulnerable/Unknown without existing mitigation), inline mitigation attribution display (who/when/description for Mitigated advisories), and `<security-dashboard-mitigation-dialog mode="mark">` integration to `client/src/components/advisory-item.element.ts`
- [ ] T018 [US1] Listen for bubbled `mitigation-changed` event on host element and call `_fetchStatus()` to refresh data in `client/src/components/security-dashboard.element.ts`

**Checkpoint**: User Story 1 is fully functional — mark an advisory, see it show Mitigated with attribution.

---

## Phase 4: User Story 2 — Remove Manual Mitigation (Priority: P2)

**Goal**: An administrator can remove a previously applied manual mitigation — the advisory reverts to its automatically calculated status.

**Independent Test**: Mark any advisory as mitigated (via US1), then click "Remove Mitigation", confirm → advisory reverts to Vulnerable/Unknown; cancel → advisory remains Mitigated.

### Tests for User Story 2 ⚠️ Write FIRST — ensure they FAIL before implementing T020–T021

- [ ] T019 [P] [US2] Add DeleteMitigation controller unit tests (204 on success, 404 not found, 401 unauthorised) in `tests/Umbraco.SecurityDashboard.Tests/Controllers/MitigationControllerTests.cs`

### Implementation for User Story 2

- [ ] T020 [US2] Add `[HttpDelete("advisories/{ghsaId}/mitigations")]` endpoint: call DeleteMitigationAsync, return 204/404 in `src/Umbraco.SecurityDashboard/Controllers/SecurityDashboardController.cs`
- [ ] T021 [US2] Add "Remove Mitigation" uui-button (visible only for manually mitigated advisories) and `<security-dashboard-mitigation-dialog mode="remove">` integration to `client/src/components/advisory-item.element.ts`

**Checkpoint**: Both user stories are functional — administrators can mark and remove mitigations independently.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Validation of the complete flow against quickstart.md acceptance tests and cross-cutting hardening.

- [ ] T022 Validate all five quickstart.md acceptance tests manually: (1) mark Vulnerable advisory → Mitigated with attribution; (2) empty description blocked; (3) mark then remove → reverts to Vulnerable; (4) two admins can both remove any mitigation; (5) mark an advisory, restart + rescan, confirm mitigation persists
- [ ] T023 [P] Verify 409 Conflict response when attempting to mark an already-mitigated advisory via the POST endpoint (covers spec clarification: button not shown, but API also rejects)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 completion — BLOCKS both user stories
- **Phase 3 (US1)**: Depends on Phase 2 — can proceed once foundational phase is complete
- **Phase 4 (US2)**: Depends on Phase 2 and Phase 3 (reuses mitigation-dialog.element.ts from T016)
- **Phase 5 (Polish)**: Depends on all implementation phases complete

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Phase 2 — no dependencies on US2
- **User Story 2 (P2)**: Starts after Phase 2; reuses T016 (`mitigation-dialog.element.ts`)

### Within Each User Story

- Tests (T008, T009, T019) MUST be written and FAIL before corresponding implementation tasks
- Models before services (T010/T011 before T012/T013)
- Services before endpoints (T013 before T014 on backend)
- TypeScript types before components (T015 before T016/T017)
- Dialog component before advisory-item wiring (T016 before T017, T016 before T021)

### Parallel Opportunities

- T004 and T005 (interface definition and test skeleton) can run in parallel
- T008 and T009 (US1 test files) can run in parallel
- T010 and T011 (ManualMitigationDto and CreateMitigationRequest) can run in parallel
- T015 (TypeScript types) can run in parallel with T010/T011

---

## Parallel Example: Phase 2

```
Run in parallel:
  T004 → Create IMitigationRepository in src/Umbraco.SecurityDashboard/Services/IMitigationRepository.cs
  T005 → Add MitigationRepositoryTests in tests/Umbraco.SecurityDashboard.Tests/Services/MitigationRepositoryTests.cs

Then sequentially:
  T006 → Implement MitigationRepository (depends on T004, T005)
  T007 → Register in composer (depends on T006)
```

## Parallel Example: User Story 1

```
Run in parallel:
  T008 → VulnerabilityServiceTests overlay cases
  T009 → MitigationControllerTests POST cases
  T010 → ManualMitigationDto model
  T011 → CreateMitigationRequest model

Then:
  T012 → AdvisoryDto extension (depends on T010)
  T015 → TypeScript types (depends on T010, can parallel with T012)

Then:
  T013 → VulnerabilityService overlay (depends on T010, T012)
  T014 → POST endpoint (depends on T011)
  T016 → mitigation-dialog.element.ts (depends on T015)

Then:
  T017 → advisory-item.element.ts (depends on T016)
  T018 → security-dashboard.element.ts (depends on T016)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Data layer (T001–T003)
2. Complete Phase 2: Repository + DI (T004–T007)
3. Complete Phase 3: US1 — Mark As Mitigated (T008–T018)
4. **STOP and VALIDATE**: Run quickstart items 1 and 2 manually
5. Deploy/demo if ready

### Incremental Delivery

1. Phase 1 + Phase 2 → Database and repository ready
2. Phase 3 → Administrators can mark mitigations → **MVP deliverable**
3. Phase 4 → Administrators can also remove mitigations → **Complete feature**
4. Phase 5 → Full acceptance validation

---

## Notes

- [P] tasks = different files, no shared state dependencies — safe to run concurrently
- [Story] label maps each task to a specific user story for traceability
- Integration tests (T005) use real DB via IScopeProvider per constitution (no mocks)
- Unit tests (T008, T009, T019) use Moq per plan.md
- `mitigation-dialog.element.ts` (T016) covers both mark and remove modes — created in US1, reused in US2
- `IBackOfficeSecurityAccessor` (not HttpContext.User) is the identity source per research Decision 3
- UNIQUE constraint on GhsaId enforces one active mitigation per advisory at DB level (research Decision 4)
- Refresh strategy: `mitigation-changed` custom DOM event bubbles to dashboard → re-fetches full status (research Decision 6)
