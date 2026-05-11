# Implementation Plan: Security Audit Log

**Branch**: `007-audit-log` | **Date**: 2026-05-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/007-audit-log/spec.md`

## Summary

Add an immutable audit log that records every overall vulnerability state change (automatic scans) and every manual mitigation action (regardless of state impact). Expose a paginated back-office API and Lit UI component. Condition webhook firing on actual state changes rather than every scan. No new NuGet or npm packages required.

## Technical Context

**Language/Version**: C# 13 / .NET 10 (Umbraco 17 LTS); TypeScript 5, Lit 3 (frontend)  
**Primary Dependencies**: Umbraco.Cms 17.x, NPoco, @umbraco-cms/backoffice, @umbraco-ui/uui — all existing  
**Storage**: Umbraco DB (SQL Server / SQLite) via NPoco + IScopeProvider; new `SecurityDashboard_AuditLog` table (migration `SecurityDashboard-1.2.0`)  
**Testing**: xUnit 2 + NSubstitute (existing `tests/Umbraco.SecurityDashboard.Tests/` project)  
**Target Platform**: Umbraco 17 back-office (server-side: Linux/Windows server)  
**Project Type**: Umbraco package (web application plugin)  
**Performance Goals**: Audit history page load ≤ 2 seconds (SC-003); achieved via DB index on `Timestamp DESC` and server-side pagination  
**Constraints**: No new NuGet packages; no schema changes to existing tables; audit entries append-only at the application level  
**Scale/Scope**: Expected hundreds to low-thousands of entries; single-site back-office deployment

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Assessment | Status |
|-----------|------------|--------|
| I. Clean Code & Simplicity | New types follow single-responsibility; audit writing is co-located with state-changing operations; `GetCurrentOverallStatusAsync()` extracts existing inline logic without duplication | ✅ PASS |
| II. Minimal External Dependencies | No new NuGet or npm packages; all runtime dependencies already in use | ✅ PASS |
| III. Test-First Development | Acceptance criteria in spec.md are the source of truth; unit tests for repository and state-change detection must be written before implementation; integration tests will use real DB contexts per constitution | ✅ PASS |
| IV. Umbraco UX Consistency | Audit view uses `uui-box`, `uui-table`, `uui-pagination`, `uui-loader` — all existing UUI components; typography and spacing follow established dashboard patterns | ✅ PASS |
| V. Security by Design | No UPDATE/DELETE endpoints for audit entries; access restricted to back-office users with Security Dashboard section access; actor name stored at write time to survive account deletion; threat model documented in research.md | ✅ PASS |

## Project Structure

### Documentation (this feature)

```text
specs/007-audit-log/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
│   └── audit-log-api.md
└── tasks.md             # Phase 2 output (/speckit-tasks command)
```

### Source Code (repository root)

```text
src/Umbraco.SecurityDashboard/
├── Controllers/
│   └── SecurityDashboardController.cs     # modified: GET audit-log endpoint; inject IAuditLogRepository + IWebhookNotifier; write audit + conditional webhook on mitigation changes
├── Composers/
│   └── SecurityDashboardComposer.cs       # modified: register AuditLogRepository → IAuditLogRepository
├── Migrations/
│   ├── SecurityDashboardMigrationPlan.cs  # modified: add .To<AddAuditLogTable>("SecurityDashboard-1.2.0")
│   └── AddAuditLogTable.cs                # new: creates SecurityDashboard_AuditLog with index on Timestamp DESC
├── Models/
│   ├── Api/
│   │   ├── AuditLogEntryDto.cs            # new: response shape for a single audit entry
│   │   └── AuditLogPageResponse.cs        # new: { Entries, TotalCount } pagination wrapper
│   └── Db/
│       └── AuditLogRecord.cs              # new: NPoco POCO mapped to SecurityDashboard_AuditLog
└── Services/
    ├── IAuditLogRepository.cs             # new: AppendAsync + GetPagedAsync
    ├── AuditLogRepository.cs              # new: NPoco implementation
    └── VulnerabilityService.cs            # modified: capture previous status, write audit entry on state change, fire webhook only on state change

client/src/
├── components/
│   └── audit-log.element.ts              # new: Lit element — paginated table with empty-state
├── security-dashboard.element.ts         # modified: render audit-log section below advisory list
└── types.ts                              # modified: add AuditLogEntryDto, AuditLogPageResponse

tests/Umbraco.SecurityDashboard.Tests/
├── Services/
│   ├── AuditLogRepositoryTests.cs        # new
│   └── VulnerabilityServiceAuditTests.cs # new
└── Controllers/
    └── SecurityDashboardControllerAuditTests.cs  # new
```

**Structure Decision**: Single project (existing). Follows the established Models → Services → Controllers → Migrations layered pattern.

## Complexity Tracking

> No Constitution Check violations. No exceptions required.
