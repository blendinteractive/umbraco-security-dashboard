# Implementation Plan: Exposure-Based Vulnerability Checks

**Branch**: `005-exposure-vuln-checks` | **Date**: 2026-05-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-exposure-vuln-checks/spec.md`

## Summary

Adds an extensible exposure-check system to the advisory evaluation cycle. When an advisory is version-matched, its description is parsed for a `### Exposure` section; extracted keywords trigger registered `IExposureCheck` implementations. Each check returns `Vulnerable`, `Mitigated`, or `NotAffected`; the worst-case result is persisted as the advisory's `AffectedStatus`. Two built-in checks cover `Non-Admin Backoffice Users` (queries Umbraco's user store) and `Content Delivery API` (reads `DeliveryApiSettings`). The existing `Affected` status string is replaced by `Vulnerable`; a new `Mitigated` value is added. No database schema migration is required — the `AffectedStatus` column (20 chars) already accommodates all new values.

## Technical Context

**Language/Version**: C# / .NET 10 (Umbraco 17 LTS)
**Primary Dependencies**: Umbraco.Cms 17.x — `IUserService`, `IUserGroupService`, `IOptions<DeliveryApiSettings>`, `IScopeProvider` (all existing); no new NuGet packages
**Storage**: Umbraco DB (NPoco) — no schema change; `AffectedStatus` VARCHAR(20) accommodates `Vulnerable` (10), `Mitigated` (9), `NotAffected` (11), `Unknown` (7)
**Testing**: xUnit (existing test project at `tests/Umbraco.SecurityDashboard.Tests/`)
**Target Platform**: Umbraco CMS back-office plugin (library project)
**Project Type**: Library
**Performance Goals**: No new external API calls; `description` field added to existing GitHub GHSA REST response
**Constraints**: `VulnerabilityService` is a singleton — exposure checks requiring scoped services (`IUserService`) must use `IServiceScopeFactory` internally
**Scale/Scope**: Exposure checks run once per advisory evaluation cycle (not per page load); advisory count is bounded by GitHub GHSA NuGet ecosystem (~dozens)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Assessment |
|-----------|-----------|
| I. Clean Code & Simplicity | PASS — parser, evaluator, and individual checks each have one clear responsibility; no premature abstraction |
| II. Minimal External Dependencies | PASS — uses only Umbraco-native APIs already available in the DI container; no new NuGet packages |
| III. Test-First Development | PASS — acceptance criteria in spec.md are complete; unit tests planned for parser, evaluator, and both built-in checks |
| IV. Umbraco UX Consistency | PASS — UI changes are colour/label updates using existing `uui-tag` components; no new custom components |
| V. Security by Design | PASS — check errors default to `Vulnerable` (fail-safe); no user-controlled input reaches check logic; no new external surfaces |

**Post-design re-check**: All gates still pass. Singleton-safety via `IServiceScopeFactory` is a standard .NET pattern, not a complexity violation.

## Project Structure

### Documentation (this feature)

```text
specs/005-exposure-vuln-checks/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── api.md
└── tasks.md             # Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code

```text
src/Umbraco.SecurityDashboard/
├── Composers/
│   └── SecurityDashboardComposer.cs         # MODIFIED: register exposure checks
├── Extensions/
│   └── UmbracoBuilderExposureExtensions.cs  # NEW: AddExposureCheck<T>() helper
├── Services/
│   ├── Exposure/
│   │   ├── IExposureCheck.cs                # NEW: interface + ExposureVerdict enum
│   │   ├── ExposureKeywordParser.cs         # NEW: parses ### Exposure sections
│   │   ├── IExposureCheckEvaluator.cs       # NEW: evaluator interface
│   │   ├── ExposureCheckEvaluator.cs        # NEW: runs checks, returns worst-case verdict
│   │   └── Checks/
│   │       ├── ContentDeliveryApiExposureCheck.cs   # NEW
│   │       └── NonAdminUsersExposureCheck.cs         # NEW
│   ├── VulnerabilityService.cs              # MODIFIED: inject evaluator; set Vulnerable/Mitigated
│   └── GitHubAdvisory.cs                   # MODIFIED: add Description property
├── Models/
│   ├── Api/
│   │   └── AdvisoryDto.cs                  # MODIFIED: update AffectedStatus doc comments
│   └── Db/
│       └── AdvisoryRecord.cs               # Unchanged
└── (no Migrations change needed)

client/src/
├── types.ts                                 # MODIFIED: update AffectedStatus union type
└── components/
    └── advisory-item.element.ts             # MODIFIED: getStatusColor for Vulnerable/Mitigated

tests/Umbraco.SecurityDashboard.Tests/
├── ExposureKeywordParserTests.cs            # NEW
├── ExposureCheckEvaluatorTests.cs           # NEW
├── ContentDeliveryApiExposureCheckTests.cs  # NEW
├── NonAdminUsersExposureCheckTests.cs       # NEW
└── VulnerabilityServiceTests.cs             # MODIFIED: updated status assertions
```

**Structure Decision**: Extends the existing single-project layout. Exposure-check types live under `Services/Exposure/` to keep them cohesive and clearly separated from the advisory-fetch pipeline.

## Complexity Tracking

*No Constitution violations requiring justification.*
