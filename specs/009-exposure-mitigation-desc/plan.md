# Implementation Plan: Exposure Check Mitigation Descriptions

**Branch**: `009-exposure-mitigation-desc` | **Date**: 2026-06-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/009-exposure-mitigation-desc/spec.md`

## Summary

Exposure checks that return `Mitigated` will now also produce a human-readable description string. These descriptions are combined (semicolon-joined) by the evaluator, stored in the `SecurityDashboard_Advisory` table as a new nullable column, surfaced via the API's `AdvisoryDto`, and displayed in the back-office dashboard alongside the advisory's mitigated status. When a manual mitigation exists it takes precedence; the exposure-check description shows only for auto-mitigated advisories without a manual override.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (Umbraco 17 LTS); TypeScript 5, Lit 3 (frontend)  
**Primary Dependencies**: Umbraco.Cms 17.x, NPoco, @umbraco-ui/uui, @umbraco-cms/backoffice — all existing; **no new packages**  
**Storage**: Umbraco DB (SQL Server / SQLite) via NPoco + IScopeProvider; `SecurityDashboard_Advisory` gains `ExposureCheckDescription NVARCHAR(MAX) NULL` (migration `SecurityDashboard-1.3.0`)  
**Testing**: xUnit, NSubstitute — all existing  
**Target Platform**: Umbraco back-office (desktop)  
**Project Type**: Umbraco package (library + back-office extension)  
**Performance Goals**: No additional latency; descriptions are produced inline within existing check execution  
**Constraints**: Description strings nullable; no upper-length constraint enforced (multi-check joins can grow the value); mobile out of scope per spec  
**Scale/Scope**: Single-site deployment; admin back-office users only

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Code & Simplicity | ✅ PASS | Narrowly scoped: new return types (`ExposureCheckResult`, `ExposureEvaluationResult`), propagation to storage and display. No premature abstraction. |
| II. Minimal External Dependencies | ✅ PASS | No new packages. All changes use existing .NET BCL types, NPoco, and the existing Umbraco stack. |
| III. Test-First Development | ✅ PASS (gated) | Tests must be written before implementation for: each updated check, evaluator description-combining and fallback logic, DTO mapping. |
| IV. Umbraco UX Consistency | ✅ PASS | UI re-uses established `.mitigation-attribution` CSS pattern from manual mitigation display. |
| V. Security by Design | ✅ PASS | Descriptions are server-generated strings (not user input); no new authorization surface introduced. |

**Post-Phase-1 re-check**: Design does not introduce new complexity or violations. All gates still pass.

## Project Structure

### Documentation (this feature)

```text
specs/009-exposure-mitigation-desc/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── advisory-api.md  # Updated advisory API contract (Phase 1)
└── tasks.md             # Phase 2 output (/speckit-tasks — not yet created)
```

### Source Code (repository root)

```text
src/Umbraco.SecurityDashboard/
├── Services/Exposure/
│   ├── ExposureCheckResult.cs          ← NEW: return type for individual checks
│   ├── ExposureEvaluationResult.cs     ← NEW: return type for evaluator
│   ├── IExposureCheck.cs               ← MODIFIED: CheckAsync → Task<ExposureCheckResult>
│   ├── IExposureCheckEvaluator.cs      ← MODIFIED: EvaluateAsync → Task<ExposureEvaluationResult>
│   ├── ExposureCheckEvaluator.cs       ← MODIFIED: collects and joins descriptions
│   └── Checks/
│       ├── ContentDeliveryApiExposureCheck.cs  ← MODIFIED: returns description when Mitigated
│       └── NonAdminUsersExposureCheck.cs       ← MODIFIED: returns description when Mitigated
├── Models/
│   ├── Db/
│   │   └── AdvisoryRecord.cs           ← MODIFIED: add ExposureCheckDescription property
│   └── Api/
│       └── AdvisoryDto.cs              ← MODIFIED: add ExposureCheckMitigationDescription
├── Migrations/
│   ├── AddExposureCheckDescriptionColumn.cs  ← NEW: SecurityDashboard-1.3.0
│   └── SecurityDashboardMigrationPlan.cs     ← MODIFIED: register new step
└── Services/
    └── VulnerabilityService.cs         ← MODIFIED: store description on save; map on read

client/src/
├── types.ts                            ← MODIFIED: add exposureCheckMitigationDescription
└── components/
    └── advisory-item.element.ts        ← MODIFIED: render description for auto-mitigated advisories

tests/Umbraco.SecurityDashboard.Tests/
└── Services/Exposure/
    ├── ExposureCheckEvaluatorTests.cs  ← NEW/MODIFIED: description combining, fallback
    └── Checks/
        ├── ContentDeliveryApiExposureCheckTests.cs  ← MODIFIED: verify description returned
        └── NonAdminUsersExposureCheckTests.cs       ← MODIFIED: verify description returned
```

**Structure Decision**: Existing single-project layout. All changes are in-place modifications plus new files in their natural locations within the existing `Services/Exposure/`, `Models/`, `Migrations/`, and `client/src/` directories.

## Complexity Tracking

> No constitution violations to justify.
