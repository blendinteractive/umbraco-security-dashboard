# Implementation Plan: Configurable Vulnerability Scan Schedule

**Branch**: `008-scan-schedule-config` | **Date**: 2026-06-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/008-scan-schedule-config/spec.md`

## Summary

Add configurable `Daily | Weekly | Disabled` scan scheduling via `appsettings.json` under `Umbraco:SecurityDashboard:ScanSchedule`. The current hardcoded 4 AM daily schedule becomes the default. A new `ScanSchedule` static helper replaces `ComputeNext4Am()` and drives `VulnerabilityCheckTask`, `StartupVulnerabilityCheckHandler`, stale-threshold derivation, and a new `scanningDisabled` flag in the dashboard API response.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (Umbraco 17 LTS) + TypeScript 5, Lit 3  
**Primary Dependencies**: Umbraco.Cms 17.x, NSubstitute, xUnit — all existing; **no new NuGet packages**  
**Storage**: No DB schema change — `NextScheduledCheckAt` is already persisted in `CheckResultRecord`  
**Testing**: xUnit + NSubstitute (existing patterns)  
**Target Platform**: Umbraco backoffice plugin  
**Performance Goals**: N/A — configuration-time computation only  
**Constraints**: Config read once at startup; hot-reload not required  
**Scale/Scope**: Single package; schedule affects one recurring background job

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Assessment | Notes |
|-----------|-----------|-------|
| I. Clean Code & Simplicity | **PASS** | `ScanSchedule` static helper has single responsibility; existing methods deleted when superseded |
| II. Minimal External Dependencies | **PASS** | Only .NET BCL (`DateTime`, `DayOfWeek`, `TimeSpan`); no new packages |
| III. Test-First Development | **PASS** | Tests specified for schedule computation, startup handler, validation, and dashboard response |
| IV. Umbraco UX Consistency | **PASS** | Disabled warning reuses UUI color tokens matching existing stale-warning pattern |
| V. Security by Design | **PASS** | `IValidateOptions` rejects invalid config at startup; no sensitive data in schedule settings |

## Project Structure

### Documentation (this feature)

```text
specs/008-scan-schedule-config/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/Umbraco.SecurityDashboard/
├── Configuration/
│   ├── SecurityDashboardSettings.cs        # MODIFY — add ScanSchedule property
│   ├── ScanScheduleSettings.cs             # NEW — schedule config POCO
│   └── ScanFrequency.cs                    # NEW — Daily / Weekly / Disabled enum
├── Scheduling/
│   ├── ScanSchedule.cs                     # NEW — static helper replacing ComputeNext4Am
│   ├── VulnerabilityCheckTask.cs           # MODIFY — use ScanSchedule for Period/Delay
│   ├── StartupVulnerabilityCheckHandler.cs # MODIFY — respect Disabled; use configured period
│   └── SecurityDashboardSettingsValidator.cs  # NEW — IValidateOptions implementation
├── Services/
│   └── VulnerabilityService.cs             # MODIFY — remove static CheckInterval/StaleThreshold;
│                                           #           use ScanSchedule; add ScanningDisabled flag
├── Models/Api/
│   └── DashboardStatusResponse.cs          # MODIFY — add ScanningDisabled bool
└── Composers/
    └── SecurityDashboardComposer.cs        # MODIFY — register validator; conditionally skip
                                            #           AddRecurringBackgroundJob when Disabled

client/src/
├── types.ts                                # MODIFY — add scanningDisabled: boolean
└── components/
    └── staleness-warning.element.ts        # MODIFY — add @property scanningDisabled + render block

README.md (repo root)                       # MODIFY — document ScanSchedule config section:
                                            #   frequency values, hour/minute/dayOfWeek fields,
                                            #   defaults, example JSON snippets, Disabled behavior
```

**Structure Decision**: Single .NET project with a companion Vite/Lit client — the existing layout. No new project or layer needed.

## Complexity Tracking

> No constitution violations requiring justification.

---

## Phase 0: Research

*Completed — all NEEDS CLARIFICATION resolved from codebase inspection. No unknowns remain.*

See [research.md](research.md) for findings.

---

## Phase 1: Design

*See [data-model.md](data-model.md) for entity definitions and [contracts/](contracts/) for API diff.*
