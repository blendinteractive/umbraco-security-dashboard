# security-dashboard Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-06-02

## Active Technologies
- C# / .NET 10 (Umbraco 17) + Umbraco.Cms 17.x, Swashbuckle.AspNetCore, NuGet.Versioning, Lit 3, Vite 5, @umbraco-ui/uui, @umbraco-cms/backoffice (001-vulnerability-dashboard)
- Umbraco database (SQL Server / SQLite) via NPoco + IScopeProvider (001-vulnerability-dashboard)
- C# / .NET 10 (Umbraco 17) + Umbraco.Cms 17.x (existing) — `UmbracoApplicationStartedNotification` from `Umbraco.Cms.Core.Notifications` (002-startup-security-check)
- Umbraco database (SQL Server / SQLite) via NPoco + IScopeProvider (existing) (002-startup-security-check)
- C# / .NET 10 (Umbraco 17) + NuGet.Versioning (existing — `VersionRange.MaxVersion` / `IsMaxInclusive` properties used) (003-filter-irrelevant-advisories)
- Umbraco database via NPoco (existing — no schema change) (003-filter-irrelevant-advisories)
- C# / .NET 10 (Umbraco 17 LTS) + Umbraco.Cms 17.x — `IUserService`, `IUserGroupService`, `IOptions<DeliveryApiSettings>`, `IScopeProvider` (all existing); no new NuGet packages (005-exposure-vuln-checks)
- Umbraco DB (NPoco) — no schema change; `AffectedStatus` VARCHAR(20) accommodates `Vulnerable` (10), `Mitigated` (9), `NotAffected` (11), `Unknown` (7) (005-exposure-vuln-checks)
- C# / .NET 10 (Umbraco 17 LTS); TypeScript, Lit 3 (frontend) + Umbraco.Cms 17.x, NPoco, @umbraco-ui/uui, @umbraco-cms/backoffice — all existing (006-manual-mitigation-marking)
- `SecurityDashboard_ManualMitigation` table via NPoco + IScopeProvider (new table, new migration) (006-manual-mitigation-marking)
- C# 13 / .NET 10 (Umbraco 17 LTS); TypeScript 5, Lit 3 (frontend) + Umbraco.Cms 17.x, NPoco, @umbraco-cms/backoffice, @umbraco-ui/uui — all existing (007-audit-log)
- Umbraco DB (SQL Server / SQLite) via NPoco + IScopeProvider; new `SecurityDashboard_AuditLog` table (migration `SecurityDashboard-1.2.0`) (007-audit-log)
- C# 13 / .NET 10 (Umbraco 17 LTS) + TypeScript 5, Lit 3 + Umbraco.Cms 17.x, NSubstitute, xUnit — all existing; **no new NuGet packages** (008-scan-schedule-config)
- No DB schema change — `NextScheduledCheckAt` is already persisted in `CheckResultRecord` (008-scan-schedule-config)


## Project Structure

```text
src/
tests/
```

## Commands

# Add commands for C# / .NET 10 (Umbraco 17 LTS)

## Code Style

C# / .NET 10 (Umbraco 17 LTS): Follow standard conventions

## Recent Changes
- 008-scan-schedule-config: Added C# 13 / .NET 10 (Umbraco 17 LTS) + TypeScript 5, Lit 3 + Umbraco.Cms 17.x, NSubstitute, xUnit — all existing; **no new NuGet packages**
- 007-audit-log: Added C# 13 / .NET 10 (Umbraco 17 LTS); TypeScript 5, Lit 3 (frontend) + Umbraco.Cms 17.x, NPoco, @umbraco-cms/backoffice, @umbraco-ui/uui — all existing
- 006-manual-mitigation-marking: Added C# / .NET 10 (Umbraco 17 LTS); TypeScript, Lit 3 (frontend) + Umbraco.Cms 17.x, NPoco, @umbraco-ui/uui, @umbraco-cms/backoffice — all existing

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
at specs/008-scan-schedule-config/plan.md
<!-- SPECKIT END -->
