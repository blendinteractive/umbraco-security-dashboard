# Tasks: Filter Irrelevant Version Advisories

**Input**: Design documents from `/specs/003-filter-irrelevant-advisories/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ quickstart.md ✅

**Note**: Test tasks are included per the project constitution (Principle III — Test-First
Development). Tests MUST be written before implementation and MUST FAIL first.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no incomplete-task dependencies)
- **[Story]**: Maps to user story in spec.md (US1 = P1, US2 = P2)

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: No infrastructure changes needed — the project structure, dependencies,
and schema are all unchanged. The only prerequisite is confirming the existing
`VulnerabilityServiceTests.cs` helper (`CreateSut`) is in place for new tests.

*No foundational tasks required. Proceed directly to user story phases.*

---

## Phase 2: User Story 1 — See Only Relevant Advisories (Priority: P1) 🎯 MVP

**Goal**: Advisory entries whose entire affected version range falls strictly below the
installed package version are excluded from storage during a check run. The advisor list
on the dashboard shows only advisories relevant to the currently installed version.

**Independent Test**: Run `RunCheckAsync` with a mock advisory whose range is
`>= 16.0, < 17.0` and installed version `17.1.0`. Assert that `SaveAdvisoriesAsync` is
called with zero advisory records.

### Tests for User Story 1 ⚠️ WRITE FIRST — must FAIL before implementation

- [X] T001 [P] [US1] Add `RunCheckAsync_OldVersionAdvisory_IsExcluded` to `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`: mock one advisory with range `">= 16.0, < 17.0"` for `Umbraco.Cms`, installed version `"17.1.0"`, assert `SaveAdvisoriesAsync` is called with an empty collection (0 records stored)
- [X] T002 [P] [US1] Add `RunCheckAsync_ExactVersionBelowInstalled_IsExcluded` to `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`: mock one advisory with range `"= 16.5.0"` for `Umbraco.Cms`, installed `"17.1.0"`, assert 0 records stored
- [X] T003 [P] [US1] Add `RunCheckAsync_SpanningAdvisory_IsIncluded` to `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`: mock one advisory with range `">= 16.0, < 17.5"` for `Umbraco.Cms`, installed `"17.1.0"`, assert 1 record stored with `AffectedStatus = "Affected"`
- [X] T004 [P] [US1] Add `RunCheckAsync_FutureAdvisory_IsIncluded` to `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`: mock one advisory with range `">= 17.5"` (no upper bound), installed `"17.1.0"`, assert 1 record stored with `AffectedStatus = "NotAffected"`
- [X] T005 [P] [US1] Add `RunCheckAsync_UnparsableRange_IsIncluded` to `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`: mock one advisory with range `"all"` (unparseable) for `Umbraco.Cms`, installed `"17.1.0"`, assert 1 record stored with `AffectedStatus = "Unknown"`
- [X] T006 [P] [US1] Add `RunCheckAsync_ExactVersionMatchInstalled_IsIncluded` to `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`: mock one advisory with range `"= 17.1.0"`, installed `"17.1.0"`, assert 1 record stored with `AffectedStatus = "Affected"`

### Implementation for User Story 1

- [X] T007 [US1] Add private static method `IsObsoleteForInstalledVersion(string? rangeString, string installedVersionString)` to `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`: parse the range via `VersionRangeParser.Parse`; if range is null or `range.MaxVersion` is null, return false; parse installedVersionString via `NuGetVersion.TryParse`, return false on failure; return `installedVersion > range.MaxVersion` when `range.IsMaxInclusive`, else `installedVersion >= range.MaxVersion` (depends on T001–T006 tests failing first)
- [X] T008 [US1] In `RunCheckAsync` in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`, add a guard immediately after the `affectedStatus`/`installedVersion` assignments: if `affectedStatus == "NotAffected"` AND `installedVersion is not null` AND `IsObsoleteForInstalledVersion(vuln.VulnerableVersionRange, installedVersion)`, execute `continue` to skip adding the record to `advisoryRecords` (depends on T007)

**Checkpoint**: User Story 1 complete — run T001–T006 tests, all should now pass.

---

## Phase 3: User Story 2 — Overall Status Reflects Only Relevant Advisories (Priority: P2)

**Goal**: The red/green status indicator and advisory count on the dashboard are computed
from the filtered advisory set only. No additional implementation is required — filtering
at check time (Phase 2) means only relevant advisories reach the display path. This phase
adds a targeted test verifying the status count is consistent with the filtered list.

**Independent Test**: Given a dataset containing only one advisory that is filtered out
(old-version range), confirm the dashboard `GetDashboardStatusAsync` returns
`OverallStatus = "Safe"` and `AffectedAdvisoryCount = 0`.

### Tests for User Story 2 ⚠️ WRITE FIRST — should already pass after Phase 2 implementation

- [X] T009 [US2] Add `RunCheckAsync_OnlyIrrelevantAdvisories_StatusIsSafe` to `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`: mock one advisory with range `">= 16.0, < 17.0"`, installed `"17.1.0"`; after `RunCheckAsync`, call `GetDashboardStatusAsync` (using a repo mock that returns the saved records); assert `OverallStatus = "Safe"` and `AffectedAdvisoryCount = 0`

### Implementation for User Story 2

No new production code required. The status and count in `GetDashboardStatusAsync`
are derived directly from stored advisory records — since Phase 2 filtering prevents
irrelevant records from being stored, the status is correct automatically.

**Checkpoint**: Both user stories complete and independently testable.

---

## Phase 4: Polish & Cross-Cutting Concerns

- [X] T010 Run `dotnet test tests/Umbraco.SecurityDashboard.Tests/` and confirm all new and existing tests pass
- [ ] T011 [P] Verify `quickstart.md` scenarios A–D manually: confirm old-version advisory excluded (A), spanning advisory kept (B), future advisory kept (C), unknown range kept (D)

---

## Dependencies & Execution Order

### Phase Dependencies

- **User Story 1 (Phase 2)**: No foundational prerequisites — start immediately
- **User Story 2 (Phase 3)**: Depends on Phase 2 implementation complete (T007, T008)
- **Polish (Phase 4)**: Depends on Phases 2 and 3 complete

### Within Phase 2

- T001–T006 (tests) can all run in parallel — all add to the same file but are independent test methods; coordinate to avoid file conflicts
- T007 depends on T001–T006 existing and failing
- T008 depends on T007

### Parallel Opportunities

- T001, T002, T003, T004, T005, T006 can be written in parallel (or sequentially in the same file)
- T010 and T011 can run in parallel

---

## Parallel Example: User Story 1

```
# Write tests in one session (all in VulnerabilityServiceTests.cs):
T001: RunCheckAsync_OldVersionAdvisory_IsExcluded
T002: RunCheckAsync_ExactVersionBelowInstalled_IsExcluded
T003: RunCheckAsync_SpanningAdvisory_IsIncluded
T004: RunCheckAsync_FutureAdvisory_IsIncluded
T005: RunCheckAsync_UnparsableRange_IsIncluded
T006: RunCheckAsync_ExactVersionMatchInstalled_IsIncluded

# Confirm build fails (IsObsoleteForInstalledVersion does not exist yet)
# Then implement:
T007: Add IsObsoleteForInstalledVersion to VulnerabilityService.cs
T008: Add continue guard to RunCheckAsync loop
```

---

## Implementation Strategy

### MVP (User Story 1 only — 9 tasks)

1. Write tests T001–T006 — confirm they fail
2. Implement T007 (new method) → T008 (loop guard)
3. **STOP and VALIDATE**: `dotnet test` — T001–T006 all pass

### Full Delivery (both stories — 11 tasks total)

1. Complete MVP above
2. Add T009 (US2 status test — likely already passes)
3. Polish: T010, T011
