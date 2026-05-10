# Tasks: Exposure-Based Vulnerability Checks

**Input**: Design documents from `/specs/005-exposure-vuln-checks/`
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/api.md ✓, quickstart.md ✓

**Tests**: Included — plan.md explicitly plans unit tests for parser, evaluator, and both built-in checks.

**Organization**: Tasks grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1–US4)
- All paths are relative to the repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add the `Description` property to the GitHub advisory model — the single prerequisite that all exposure check work depends on.

- [ ] T001 Add `[JsonPropertyName("description")] public string? Description { get; set; }` property to `GitHubAdvisory` in `src/Umbraco.SecurityDashboard/Services/GitHubAdvisory.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core types, interfaces, parser, evaluator, and the `Affected → Vulnerable` rename. ALL user story phases depend on this phase being complete.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T002 [P] Create `IExposureCheck.cs` containing the `ExposureVerdict` enum (`NotAffected = 0`, `Mitigated = 1`, `Vulnerable = 2`) and the `IExposureCheck` interface (`string Keyword`, `Task<ExposureVerdict> CheckAsync(CancellationToken)`) in `src/Umbraco.SecurityDashboard/Services/Exposure/IExposureCheck.cs`
- [ ] T003 [P] Create `IExposureCheckEvaluator.cs` containing the `IExposureCheckEvaluator` interface (`Task<string> EvaluateAsync(IEnumerable<string> keywords, CancellationToken)`) — contract per data-model.md — in `src/Umbraco.SecurityDashboard/Services/Exposure/IExposureCheckEvaluator.cs`
- [ ] T004 Create `ExposureKeywordParser.cs` as a static utility with a `ParseKeywords(string? description)` method that extracts `* *[Keyword]*` bullets from the `### Exposure` section of an advisory markdown string, returning an empty collection for null/missing/malformed input (fail-safe per FR-001, FR-002) in `src/Umbraco.SecurityDashboard/Services/Exposure/ExposureKeywordParser.cs`
- [ ] T005 Create `ExposureCheckEvaluator.cs` implementing `IExposureCheckEvaluator`: inject `IEnumerable<IExposureCheck>` (registered checks); on `EvaluateAsync`, find checks whose `Keyword` matches any item in `keywords`, run all matches concurrently, catch exceptions per check (log + treat as `Vulnerable`), return worst-case `ExposureVerdict.Max()` cast to string; return `"Vulnerable"` when no keywords or no matches (FR-004–FR-008, FR-014) in `src/Umbraco.SecurityDashboard/Services/Exposure/ExposureCheckEvaluator.cs`
- [ ] T006 Replace all `"Affected"` string literals with `"Vulnerable"` in `VulnerabilityService.cs`: update `DetermineAffectedStatus()` return value, update `ConsolidateStatus()` precedence order (`Vulnerable > Unknown > Mitigated > NotAffected`), update `GetDashboardStatusAsync()` overallStatus logic to support `Mitigated` state (all matched advisories are Mitigated, none Vulnerable/Unknown) and to exclude `Mitigated` from `affectedAdvisoryCount` (FR-009, FR-016), update `RunCheckAsync()` webhook filter to trigger on `"Vulnerable"` instead of `"Affected"` in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin.

---

## Phase 3: User Story 1 — Accurate Risk Assessment for Conditional Vulnerabilities (Priority: P1) 🎯 MVP

**Goal**: The Non-Admin Backoffice Users check enables advisories to display as **Mitigated** (yellow) when no non-admin users exist, and **Vulnerable** (red) when they do.

**Independent Test**: Seed an advisory with `### Exposure\n* *Non-Admin Backoffice Users*` in its description. Configure the site without non-admin users and verify the dashboard shows "Mitigated". Add a non-admin user and verify "Vulnerable".

### Tests for User Story 1

> **Write these tests BEFORE or alongside implementation; they should fail before the implementation is complete.**

- [ ] T007 [P] [US1] Write `ExposureKeywordParserTests.cs` covering: keywords extracted from valid `### Exposure` bullet list; empty collection returned for null description; empty collection for description with no `### Exposure` heading; empty collection for `### Exposure` section with no bullets; multiple keywords extracted from multiple bullets in `tests/Umbraco.SecurityDashboard.Tests/ExposureKeywordParserTests.cs`
- [ ] T008 [P] [US1] Write `ExposureCheckEvaluatorTests.cs` covering: single matching check returning `Mitigated` → result `"Mitigated"`; single matching check returning `Vulnerable` → result `"Vulnerable"`; multiple checks with mixed verdicts → worst-case wins (`Vulnerable`); no keywords → returns `"Vulnerable"`; no matching checks registered → returns `"Vulnerable"`; check throws exception → treated as `Vulnerable` and does not propagate in `tests/Umbraco.SecurityDashboard.Tests/ExposureCheckEvaluatorTests.cs`

### Implementation for User Story 1

- [ ] T009 [P] [US1] Create `NonAdminUsersExposureCheck.cs` implementing `IExposureCheck` with `Keyword = "Non-Admin Backoffice Users"`; inject `IServiceScopeFactory`; in `CheckAsync` create a scope, resolve `IUserService`, call `GetAll(0, int.MaxValue, out _)`, return `Vulnerable` if any user's groups exclude `Constants.Security.AdminGroupAlias`, `Mitigated` otherwise (empty user list → `Mitigated` per research.md Decision 6) in `src/Umbraco.SecurityDashboard/Services/Exposure/Checks/NonAdminUsersExposureCheck.cs`
- [ ] T010 [US1] Write `NonAdminUsersExposureCheckTests.cs` covering: all-admin site → `Mitigated`; site with one non-admin user → `Vulnerable`; empty user list → `Mitigated`; `IUserService.GetAll` throws → result is `Vulnerable` (exception handled by evaluator, tested indirectly here by asserting check behavior) in `tests/Umbraco.SecurityDashboard.Tests/NonAdminUsersExposureCheckTests.cs`
- [ ] T011 [US1] Modify `VulnerabilityService.cs` to inject `IExposureCheckEvaluator` via constructor; in `RunCheckAsync`, for each advisory call `ExposureKeywordParser.ParseKeywords(advisory.Description)` before the inner package loop, then call `_evaluator.EvaluateAsync(keywords)` and use the returned string as `AffectedStatus` for all version-matched packages in that advisory (per research.md Decision 8) in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`
- [ ] T012 [US1] Update `VulnerabilityServiceTests.cs` assertions to use `"Vulnerable"` (replacing `"Affected"`) and add assertions that mock the evaluator returning `"Mitigated"` produces a `Mitigated` advisory record in `tests/Umbraco.SecurityDashboard.Tests/VulnerabilityServiceTests.cs`
- [ ] T013 [P] [US1] Update `AdvisoryDto.cs` XML doc comment on `AffectedStatus` property to read `Vulnerable | Mitigated | NotAffected | Unknown` in `src/Umbraco.SecurityDashboard/Models/Api/AdvisoryDto.cs`
- [ ] T014 [P] [US1] Update `affectedStatus` union type in `AdvisoryPackageDto` and `AdvisoryDto` interfaces from `'Affected' | ...` to `'Vulnerable' | 'Mitigated' | 'NotAffected' | 'Unknown'` in `client/src/types.ts`
- [ ] T015 [US1] Update `getStatusColor` in `advisory-item.element.ts` to map: `'Vulnerable'` → `'danger'` (replacing `'Affected'`), `'Mitigated'` → `'caution'`, `'Unknown'` → `'warning'`, `'NotAffected'` → `'positive'`; ensure label renders `'NotAffected'` as `"Not Affected"` (no change needed for others) in `client/src/components/advisory-item.element.ts`

**Checkpoint**: User Story 1 is fully functional. The dashboard correctly shows Mitigated/Vulnerable for advisories with `Non-Admin Backoffice Users` exposure. All parser, evaluator, and non-admin check unit tests pass.

---

## Phase 4: User Story 2 — Mitigated Status for Partially-Addressed Vulnerabilities (Priority: P2)

**Goal**: The Content Delivery API check allows advisories to display as **Mitigated** when the CDA is disabled, and **Vulnerable** when enabled.

**Independent Test**: Enable `Umbraco:CMS:DeliveryApi:Enabled = true` in configuration and verify a matching advisory shows "Vulnerable". Set to `false` and verify "Mitigated".

- [ ] T016 [P] [US2] Create `ContentDeliveryApiExposureCheck.cs` implementing `IExposureCheck` with `Keyword = "Content Delivery API"`; inject `IOptions<DeliveryApiSettings>` from `Umbraco.Cms.Core.Configuration.Models`; in `CheckAsync` return `Vulnerable` if `settings.Value.Enabled == true`, `Mitigated` otherwise (no network probe — config-only per research.md Decision 5) in `src/Umbraco.SecurityDashboard/Services/Exposure/Checks/ContentDeliveryApiExposureCheck.cs`
- [ ] T017 [P] [US2] Write `ContentDeliveryApiExposureCheckTests.cs` covering: `Enabled = true` → `Vulnerable`; `Enabled = false` → `Mitigated` in `tests/Umbraco.SecurityDashboard.Tests/ContentDeliveryApiExposureCheckTests.cs`

**Checkpoint**: User Story 2 is fully functional. Content Delivery API check correctly returns Vulnerable/Mitigated based on configuration.

---

## Phase 5: User Story 3 — Advisories Without Exposure Checks Default to Vulnerable (Priority: P2)

**Goal**: Confirm the fail-safe posture — advisories with no `### Exposure` section, unrecognised keywords, or null descriptions always display as **Vulnerable**.

**Independent Test**: Seed an advisory with no `### Exposure` section and verify "Vulnerable". Seed one with only an unregistered keyword and verify "Vulnerable".

- [ ] T018 [US3] Extend `ExposureCheckEvaluatorTests.cs` with explicit fail-safe scenarios: empty keyword list → `"Vulnerable"`; keywords list with no registered match → `"Vulnerable"`; keywords that partially match (some match, some don't) → worst-case applies in `tests/Umbraco.SecurityDashboard.Tests/ExposureCheckEvaluatorTests.cs`
- [ ] T019 [P] [US3] Extend `VulnerabilityServiceTests.cs` with null/empty advisory description scenarios: version-matched advisory with `null` description must store `AffectedStatus = "Vulnerable"`; advisory with description but no `### Exposure` section must store `AffectedStatus = "Vulnerable"` in `tests/Umbraco.SecurityDashboard.Tests/VulnerabilityServiceTests.cs`

**Checkpoint**: All fail-safe scenarios verified. No version-matched advisory can be silently dismissed.

---

## Phase 6: User Story 4 — Extensible Check Registration (Priority: P3)

**Goal**: Third-party developers can register a new `IExposureCheck` implementation via `builder.AddExposureCheck<T>()` without modifying core advisory-processing logic.

**Independent Test**: Register a custom check implementation against keyword `Public Registration`, seed an advisory with that keyword, and verify the check's verdict determines the advisory status.

- [ ] T020 [US4] Create `UmbracoBuilderExposureExtensions.cs` with `AddExposureCheck<T>()` extension method on `IUmbracoBuilder` that registers `T` as `IExposureCheck` with singleton lifetime in the DI container in `src/Umbraco.SecurityDashboard/Extensions/UmbracoBuilderExposureExtensions.cs`
- [ ] T021 [US4] Modify `SecurityDashboardComposer.cs` to register `NonAdminUsersExposureCheck` and `ContentDeliveryApiExposureCheck` using `builder.AddExposureCheck<T>()`, and register `ExposureCheckEvaluator` as `IExposureCheckEvaluator` singleton in `src/Umbraco.SecurityDashboard/Composers/SecurityDashboardComposer.cs`

**Checkpoint**: Both built-in checks are wired into DI. A third-party developer can call `builder.AddExposureCheck<MyCheck>()` in their own `IComposer` without touching the core codebase.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T022 [P] Search codebase for any remaining `"Affected"` string literals (excluding comments/tests referencing the old value by name) and replace with `"Vulnerable"` across `src/`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1)**: Depends on Phase 2 — first user story, MVP scope
- **Phase 4 (US2)**: Depends on Phase 2 — can begin once foundation is complete; independent of Phase 3
- **Phase 5 (US3)**: Depends on Phase 2 infrastructure; tests extend files from Phases 3/4 but fail-safe logic is already in the evaluator
- **Phase 6 (US4)**: Depends on Phases 3 and 4 (both built-in checks must exist before registering them)
- **Phase 7 (Polish)**: Depends on all phases complete

### User Story Dependencies

- **US1 (P1)**: Requires Foundation (Phase 2) only — independently testable via unit tests
- **US2 (P2)**: Requires Foundation (Phase 2) only — independently testable via unit tests
- **US3 (P2)**: Validated by evaluator tests; no new implementation required beyond Phase 2
- **US4 (P3)**: Requires US1 + US2 checks to exist (T009, T016) so they can be registered

### Parallel Opportunities

- **T002 + T003**: Both interface files are independent — write simultaneously
- **T007 + T008 + T009**: Parser tests, evaluator tests, and NonAdminCheck implementation are all different files
- **T013 + T014**: AdvisoryDto doc update and TypeScript types update are independent
- **T016 + T017**: ContentDeliveryApiExposureCheck implementation and its tests are independent
- **T018 + T019**: Fail-safe tests in different test files

---

## Parallel Example: Phase 3 (User Story 1)

```
# Launch these in parallel:
T007 — ExposureKeywordParserTests.cs
T008 — ExposureCheckEvaluatorTests.cs
T009 — NonAdminUsersExposureCheck.cs

# Then sequentially:
T010 — NonAdminUsersExposureCheckTests.cs (depends on T009)
T011 — VulnerabilityService.cs modification (depends on T005 evaluator)
T012 — VulnerabilityServiceTests.cs update (depends on T011)

# In parallel with T010–T012:
T013 — AdvisoryDto.cs doc update
T014 — types.ts union type update

# After T014:
T015 — advisory-item.element.ts color mapping
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 (T001) + Phase 2 (T002–T006)
2. Complete Phase 3 (T007–T015)
3. **STOP and VALIDATE**: Confirm Non-Admin Users advisory correctly shows Mitigated/Vulnerable
4. Dashboard UI shows correct yellow/red colors per FR-010

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready
2. Phase 3 (US1) → Non-admin check live, UI updated → **MVP demo**
3. Phase 4 (US2) → CDA check live
4. Phase 5 (US3) → Fail-safe edge cases verified
5. Phase 6 (US4) → Extensibility mechanism available for third parties
6. Phase 7 → Cleanup

---

## Notes

- `[P]` tasks touch different files — safe to run concurrently
- Unit tests do **not** require DI registration (Phases 3–5 test classes directly); registration happens in Phase 6
- `ExposureCheckEvaluator` is a singleton — it injects `IEnumerable<IExposureCheck>` once at construction; no scoped lifetime issues at the evaluator level
- `NonAdminUsersExposureCheck` must use `IServiceScopeFactory` to resolve `IUserService` (scoped) — this pattern is tested in T010
- No database migrations required; `AffectedStatus VARCHAR(20)` already accommodates all four new values
