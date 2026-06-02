# Tasks: Configurable Vulnerability Scan Schedule

**Input**: Design documents from `/specs/008-scan-schedule-config/`
**Prerequisites**: plan.md, spec.md, data-model.md, research.md, contracts/api-diff.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to
- Include exact file paths in descriptions

---

## Phase 1: Setup

*No separate setup required — existing Umbraco project and test project are already in place.*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Configuration POCO, enum, static schedule helper, and validator — required by all user stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T001 [P] Create `ScanFrequency` enum (Daily/Weekly/Disabled) in `src/Umbraco.SecurityDashboard/Configuration/ScanFrequency.cs`
- [X] T002 [P] Create `ScanScheduleSettings` POCO with `Frequency`, `Hour`, `Minute`, `DayOfWeek` properties and defaults in `src/Umbraco.SecurityDashboard/Configuration/ScanScheduleSettings.cs`
- [X] T003 [P] Add `public ScanScheduleSettings ScanSchedule { get; set; } = new();` property to `SecurityDashboardSettings` in `src/Umbraco.SecurityDashboard/Configuration/SecurityDashboardSettings.cs`
- [X] T004 Create `ScanSchedule` static helper with `ComputeNextOccurrence`, `GetCheckInterval`, and `GetStaleThreshold` methods (Daily / Weekly / Disabled paths per data-model.md) in `src/Umbraco.SecurityDashboard/Scheduling/ScanSchedule.cs`
- [X] T005 Create `SecurityDashboardSettingsValidator` implementing `IValidateOptions<SecurityDashboardSettings>` that validates `Hour ∈ [0,23]`, `Minute ∈ [0,59]`, `DayOfWeek` validity, and `Frequency` validity in `src/Umbraco.SecurityDashboard/Configuration/SecurityDashboardSettingsValidator.cs`
- [X] T006 Register `SecurityDashboardSettingsValidator` as `IValidateOptions<SecurityDashboardSettings>` singleton and call `.ValidateOnStart()` in `src/Umbraco.SecurityDashboard/Composers/SecurityDashboardComposer.cs`

**Checkpoint**: Foundation ready — enum, settings POCO, static helper, and validator all exist. User story work can now begin.

---

## Phase 3: User Story 1 — Configure Daily Scan Time (Priority: P1) 🎯 MVP

**Goal**: A developer can configure the scan to run daily at a custom hour and minute; defaults (4:00 AM daily) are preserved when no config is set.

**Independent Test**: Set `Hour=2`, `Minute=30` in `appsettings.json`, restart, and confirm `nextScheduledCheckAt` in the dashboard shows the next 2:30 AM occurrence.

### Tests for User Story 1 ⚠️ Write first — must FAIL before implementation

- [X] T007 [P] [US1] Create `ScanScheduleTests.cs` covering daily scenarios: default 4 AM, custom 2:30 AM, past-time-rolls-to-next-day, and midnight (00:00) edge case in `tests/Umbraco.SecurityDashboard.Tests/Scheduling/ScanScheduleTests.cs`
- [X] T008 [P] [US1] Create `SecurityDashboardSettingsValidatorTests.cs` with test cases for `Hour > 23` and `Minute > 59` producing `OptionsValidationException` in `tests/Umbraco.SecurityDashboard.Tests/Configuration/SecurityDashboardSettingsValidatorTests.cs`

### Implementation for User Story 1

- [X] T009 [US1] Update `VulnerabilityCheckTask` to inject `IOptions<SecurityDashboardSettings>` and use `ScanSchedule.GetCheckInterval` for `Period` and `ScanSchedule.ComputeNextOccurrence` for initial `Delay` in `src/Umbraco.SecurityDashboard/Scheduling/VulnerabilityCheckTask.cs`
- [X] T010 [US1] Update `StartupVulnerabilityCheckHandler` to inject `IOptions<SecurityDashboardSettings>` and replace `VulnerabilityService.CheckInterval` reference with `ScanSchedule.GetCheckInterval(settings.Value.ScanSchedule)` in `src/Umbraco.SecurityDashboard/Scheduling/StartupVulnerabilityCheckHandler.cs`
- [X] T011 [US1] Update `StartupVulnerabilityCheckHandlerTests.cs` to remove all `VulnerabilityService.CheckInterval` references and pass the check interval directly from a `ScanScheduleSettings` instance in `tests/Umbraco.SecurityDashboard.Tests/Scheduling/StartupVulnerabilityCheckHandlerTests.cs`

**Checkpoint**: Daily scheduling with configurable hour/minute is fully functional and tested independently. Default 4 AM behavior preserved.

---

## Phase 4: User Story 2 — Configure Weekly Scan on a Specific Day (Priority: P2)

**Goal**: A developer can configure the scan to run once per week on a specific day at a specific time.

**Independent Test**: Set `Frequency=Weekly`, `DayOfWeek=Monday`, `Hour=3`, restart, and confirm `nextScheduledCheckAt` in the dashboard shows the next Monday at 3:00 AM.

### Tests for User Story 2 ⚠️ Write first — must FAIL before implementation

- [ ] T012 [P] [US2] Add weekly schedule test cases to `ScanScheduleTests.cs`: next Monday at 3 AM, same-day-time-already-passed rolls to next week, same-day-time-not-yet-passed returns today, and day-of-week walk (today is Tuesday, configured for Friday) in `tests/Umbraco.SecurityDashboard.Tests/Scheduling/ScanScheduleTests.cs`
- [ ] T013 [P] [US2] Add weekly validation test cases to `SecurityDashboardSettingsValidatorTests.cs`: invalid `DayOfWeek` string produces `OptionsValidationException` in `tests/Umbraco.SecurityDashboard.Tests/Configuration/SecurityDashboardSettingsValidatorTests.cs`

### Implementation for User Story 2

- [ ] T014 [US2] Verify `ScanSchedule.ComputeNextOccurrence` weekly path satisfies all US2 acceptance scenarios; confirm `GetCheckInterval` returns `TimeSpan.FromDays(7)` for `Weekly` in `src/Umbraco.SecurityDashboard/Scheduling/ScanSchedule.cs`

**Checkpoint**: Weekly scheduling with configurable day-of-week, hour, and minute is fully functional and tested independently.

---

## Phase 5: User Story 3 — View Scheduled Check Time in Dashboard (Priority: P3)

**Goal**: The dashboard's `nextScheduledCheckAt` field reflects the configured schedule (not the old hardcoded 4 AM constant).

**Independent Test**: Change schedule to weekly Monday 3 AM, restart, load dashboard, confirm `nextScheduledCheckAt` shows the next Monday at 3:00 AM.

### Tests for User Story 3 ⚠️ Write first — must FAIL before implementation

- [ ] T015 [P] [US3] Update `VulnerabilityServiceTests.cs` with tests verifying stale threshold is 48 h for Daily and 9 days for Weekly (replaces hardcoded constant assertions) in `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`

### Implementation for User Story 3

- [ ] T016 [US3] Update `VulnerabilityService` to remove static `CheckInterval` and `StaleThreshold` fields and use `ScanSchedule.GetStaleThreshold(settings.ScanSchedule)` for stale calculation and `ScanSchedule.GetCheckInterval` where needed in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`

**Checkpoint**: Dashboard `nextScheduledCheckAt` reflects the active schedule configuration; stale threshold adapts to configured frequency.

---

## Phase 6: User Story 4 — Disable Automatic Scanning in Dev Environments (Priority: P3)

**Goal**: Setting `Frequency=Disabled` in `appsettings.Development.json` suppresses all automatic checks and displays a prominent dashboard warning.

**Independent Test**: Set `Frequency=Disabled`, restart, confirm no scan runs on startup, load dashboard, confirm scanning-disabled warning is visible.

### Tests for User Story 4 ⚠️ Write first — must FAIL before implementation

- [ ] T017 [P] [US4] Add Disabled-frequency test cases to `ScanScheduleTests.cs`: `ComputeNextOccurrence` returns `DateTime.MaxValue`, `GetCheckInterval` returns `TimeSpan.MaxValue`, `GetStaleThreshold` returns 9-day value in `tests/Umbraco.SecurityDashboard.Tests/Scheduling/ScanScheduleTests.cs`
- [ ] T018 [P] [US4] Add `ScanningDisabled` tests to `VulnerabilityServiceTests.cs`: response sets `ScanningDisabled = true` and `IsStale = false` when frequency is Disabled in `tests/Umbraco.SecurityDashboard.Tests/Services/VulnerabilityServiceTests.cs`

### Implementation for User Story 4

- [ ] T019 [P] [US4] Update `SecurityDashboardComposer` to read `Umbraco:SecurityDashboard:ScanSchedule:Frequency` from `builder.Config` at compose-time and skip `AddRecurringBackgroundJob<VulnerabilityCheckTask>()` when value is `"Disabled"` in `src/Umbraco.SecurityDashboard/Composers/SecurityDashboardComposer.cs`
- [ ] T020 [P] [US4] Update `StartupVulnerabilityCheckHandler` to check `settings.Value.ScanSchedule.Frequency == ScanFrequency.Disabled` and skip (with a log warning) when Disabled in `src/Umbraco.SecurityDashboard/Scheduling/StartupVulnerabilityCheckHandler.cs`
- [ ] T021 [P] [US4] Add `public bool ScanningDisabled { get; set; }` to `DashboardStatusResponse` in `src/Umbraco.SecurityDashboard/Models/Api/DashboardStatusResponse.cs`
- [ ] T022 [US4] Update `VulnerabilityService` to populate `ScanningDisabled = true` and force `IsStale = false` in the status response when frequency is Disabled in `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs`
- [ ] T023 [P] [US4] Add `scanningDisabled: boolean` field to `DashboardStatusResponse` interface in `client/src/types.ts`
- [ ] T024 [US4] Add `@property({ type: Boolean }) scanningDisabled = false;` and a disabled-warning render block (styled with `--uui-color-danger-surface` / `--uui-color-danger` tokens; suppress `isStale` warning when `scanningDisabled` is true) in `client/src/components/staleness-warning.element.ts`

**Checkpoint**: All four user stories are complete. Disabled mode suppresses scheduling, startup handler, and shows the dashboard warning.

---

## Phase 7: Polish & Cross-Cutting Concerns

- [ ] T025 [P] Document `ScanSchedule` config section in `README.md` — frequency values, `Hour`/`Minute`/`DayOfWeek` fields, defaults, three example JSON snippets (Daily 2:30 AM, Weekly Monday 3 AM, Disabled), and restart-required note
- [ ] T026 [P] Add `"ScanSchedule": { "Frequency": "Disabled" }` to `Umbraco:SecurityDashboard` section in `demo/appsettings.Development.json` so the demo environment suppresses background scans by default

---

## Dependencies & Execution Order

### Phase Dependencies

- **Foundational (Phase 2)**: No dependencies — start immediately. BLOCKS all user stories.
- **US1 (Phase 3)**: Requires Phase 2 complete.
- **US2 (Phase 4)**: Requires Phase 2 complete. Independent of US1.
- **US3 (Phase 5)**: Requires Phase 2 complete. Best after US1 (shares VulnerabilityCheckTask understanding).
- **US4 (Phase 6)**: Requires Phase 2 + US3 complete (needs `ScanningDisabled` on response model from US3 context).
- **Polish (Phase 7)**: Requires all user stories complete.

### User Story Dependencies

- **US1 (P1)**: Starts after Phase 2. No dependency on US2–US4.
- **US2 (P2)**: Starts after Phase 2. `ScanSchedule.cs` weekly path must exist (T004 from Phase 2).
- **US3 (P3)**: Starts after Phase 2. Edits `VulnerabilityService.cs` — serialize after US1 edits `VulnerabilityCheckTask.cs`.
- **US4 (P3)**: Depends on T021 (`DashboardStatusResponse.cs`) before T022 (`VulnerabilityService.cs` sets the flag).

### Within Each User Story

- Tests MUST be written and FAIL before their implementation tasks
- Phase 2 foundational files (T001–T003) can all be created in parallel
- T004 (ScanSchedule.cs) requires T001 + T002 to exist
- T005 (Validator) requires T002 to exist; can be written in parallel with T004

### Parallel Opportunities

- T001, T002, T003 — three different new files, no inter-dependencies
- T007, T008 — two different new test files for US1
- T012, T013 — two different test files for US2 (T012 extends T007's file; T013 extends T008's file)
- T017, T018 — two different test files for US4
- T019, T020, T021, T023 — four different files in US4 implementation

---

## Parallel Example: Foundational Phase

```
Parallel group A (no dependencies):
  Task T001: Create ScanFrequency.cs
  Task T002: Create ScanScheduleSettings.cs
  Task T003: Add ScanSchedule property to SecurityDashboardSettings.cs

Then sequentially (T004 and T005 require T001/T002):
  Task T004: Create ScanSchedule.cs static helper
  Task T005: Create SecurityDashboardSettingsValidator.cs
  Task T006: Update SecurityDashboardComposer.cs (register validator)
```

## Parallel Example: User Story 4

```
Parallel group A (tests — write first):
  Task T017: Disabled tests in ScanScheduleTests.cs
  Task T018: ScanningDisabled tests in VulnerabilityServiceTests.cs

Then parallel group B (independent implementation files):
  Task T019: SecurityDashboardComposer.cs (skip background job)
  Task T020: StartupVulnerabilityCheckHandler.cs (skip startup check)
  Task T021: DashboardStatusResponse.cs (add ScanningDisabled property)
  Task T023: client/src/types.ts (add scanningDisabled field)

Then sequentially (T022 requires T021):
  Task T022: VulnerabilityService.cs (populate ScanningDisabled)
  Task T024: staleness-warning.element.ts (consume scanningDisabled)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational
2. Complete Phase 3: User Story 1 (daily schedule with configurable time)
3. **STOP and VALIDATE**: Confirm default 4 AM behavior preserved; confirm custom 2:30 AM works; confirm invalid `Hour` fails at startup
4. Deploy or demo if ready

### Incremental Delivery

1. Phase 2 (Foundational) → All stories unlocked
2. US1 → Daily schedule configurable → Test + demo (MVP)
3. US2 → Weekly schedule configurable → Test + demo
4. US3 → Dashboard reflects configured schedule → Test + demo
5. US4 → Disabled mode + frontend warning → Test + demo
6. Polish → README + dev config

### Parallel Team Strategy

With multiple developers, once Phase 2 is complete:
- Developer A: US1 (VulnerabilityCheckTask, StartupHandler)
- Developer B: US2 tests (extend ScanScheduleTests.cs for weekly)
- Developer C: US3 (VulnerabilityService stale threshold)

US4 and Polish follow after US1–US3.

---

## Notes

- `[P]` tasks = different files, no blocking dependencies within the phase
- `[Story]` label maps task to specific user story for traceability
- Tests must be written before implementation and must FAIL first (TDD)
- `VulnerabilityService.CheckInterval` public static field is removed in T016 — update any test references in T015 before T016 runs
- `ScanSchedule.cs` weekly path is implemented in T004 (Phase 2); US2 (Phase 4) validates it with targeted tests
- No DB migration required — `NextScheduledCheckAt` column already exists on `CheckResultRecord`
