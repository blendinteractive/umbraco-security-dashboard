# Implementation Plan: Manual Vulnerability Mitigation Marking

**Branch**: `006-manual-mitigation-marking` | **Date**: 2026-05-11 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/006-manual-mitigation-marking/spec.md`

## Summary

Administrators need to manually mark advisories as mitigated (via compensating controls, configuration changes, etc.) and later remove that marking if circumstances change. This feature adds a `SecurityDashboard_ManualMitigation` table keyed by GhsaId, two new API endpoints (create / delete mitigation), service-layer overlay that applies manual mitigations on top of calculated advisory statuses, and frontend UI (inline dialog + attribution display) within the existing `advisory-item` Lit component.

## Technical Context

**Language/Version**: C# / .NET 10 (Umbraco 17 LTS); TypeScript, Lit 3 (frontend)  
**Primary Dependencies**: Umbraco.Cms 17.x, NPoco, @umbraco-ui/uui, @umbraco-cms/backoffice — all existing  
**Storage**: `SecurityDashboard_ManualMitigation` table via NPoco + IScopeProvider (new table, new migration)  
**Testing**: xUnit (existing test project); Moq for unit tests  
**Target Platform**: Umbraco backoffice (web)  
**Project Type**: Umbraco plugin (library + backoffice extension)  
**Performance Goals**: No special goals — write path is infrequent (per-advisory, per-administrator action)  
**Constraints**: No new NuGet packages; no new npm packages; stay within existing auth (BackOfficeAccess policy)  
**Scale/Scope**: Per-instance; mitigations are per-advisory (GhsaId); low volume

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Code & Simplicity | ✅ PASS | New `IMitigationRepository` has a single responsibility; no over-engineering |
| II. Minimal External Dependencies | ✅ PASS | Zero new dependencies; all capabilities exist in the current stack |
| III. Test-First Development | ✅ PASS | Acceptance tests derived from spec before implementation; integration tests require real DB per constitution |
| IV. Umbraco UX Consistency | ✅ PASS | Uses `<uui-dialog>`, `<uui-textarea>`, `<uui-button>` from existing UUI; no custom CSS frameworks |
| V. Security by Design | ✅ PASS | Auth enforced server-side via existing `BackOfficeAccess` policy; description sanitised at boundary; parameterised SQL throughout |

**Post-design re-check**: To be completed after Phase 1.

## Project Structure

### Documentation (this feature)

```text
specs/006-manual-mitigation-marking/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code

```text
src/Umbraco.SecurityDashboard/
├── Migrations/
│   ├── CreateSecurityDashboardTables.cs       (existing)
│   ├── AddManualMitigationTable.cs            (NEW)
│   └── SecurityDashboardMigrationPlan.cs      (MODIFIED — add new step)
├── Models/
│   ├── Db/
│   │   ├── AdvisoryRecord.cs                  (existing)
│   │   ├── CheckResultRecord.cs               (existing)
│   │   └── ManualMitigationRecord.cs          (NEW)
│   └── Api/
│       ├── AdvisoryDto.cs                     (MODIFIED — add ManualMitigationDto field)
│       ├── DashboardStatusResponse.cs         (existing)
│       ├── ManualMitigationDto.cs             (NEW)
│       └── CreateMitigationRequest.cs         (NEW)
├── Services/
│   ├── IMitigationRepository.cs               (NEW)
│   ├── MitigationRepository.cs               (NEW)
│   ├── IVulnerabilityService.cs               (existing)
│   └── VulnerabilityService.cs               (MODIFIED — overlay mitigations in GetDashboardStatusAsync)
├── Controllers/
│   └── SecurityDashboardController.cs        (MODIFIED — add 2 new endpoints)
└── Composers/
    └── SecurityDashboardComposer.cs           (MODIFIED — register IMitigationRepository)

client/src/
├── types.ts                                   (MODIFIED — add ManualMitigationDto, extend AdvisoryDto)
└── components/
    ├── advisory-item.element.ts               (MODIFIED — add "Mark As Mitigated" + details + remove)
    └── mitigation-dialog.element.ts           (NEW — reusable inline dialog for mark/remove)

tests/Umbraco.SecurityDashboard.Tests/
├── Services/
│   ├── VulnerabilityServiceTests.cs           (existing — add mitigation overlay cases)
│   └── MitigationRepositoryTests.cs          (NEW — integration tests)
└── Controllers/
    └── MitigationControllerTests.cs          (NEW — unit tests for new endpoints)
```

**Structure Decision**: Single project layout (existing pattern). The mitigation repository follows the same repository pattern as `VulnerabilityCheckRepository`. No new projects are introduced.

## Complexity Tracking

> No constitution violations.
