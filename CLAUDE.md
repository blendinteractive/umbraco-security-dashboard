# security-dashboard Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-04-09

## Active Technologies
- C# / .NET 10 (Umbraco 17) + Umbraco.Cms 17.x, Swashbuckle.AspNetCore, NuGet.Versioning, Lit 3, Vite 5, @umbraco-ui/uui, @umbraco-cms/backoffice (001-vulnerability-dashboard)
- Umbraco database (SQL Server / SQLite) via NPoco + IScopeProvider (001-vulnerability-dashboard)
- C# / .NET 10 (Umbraco 17) + Umbraco.Cms 17.x (existing) — `UmbracoApplicationStartedNotification` from `Umbraco.Cms.Core.Notifications` (002-startup-security-check)
- Umbraco database (SQL Server / SQLite) via NPoco + IScopeProvider (existing) (002-startup-security-check)
- C# / .NET 10 (Umbraco 17) + NuGet.Versioning (existing — `VersionRange.MaxVersion` / `IsMaxInclusive` properties used) (003-filter-irrelevant-advisories)
- Umbraco database via NPoco (existing — no schema change) (003-filter-irrelevant-advisories)


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
- 003-filter-irrelevant-advisories: Added C# / .NET 10 (Umbraco 17) + NuGet.Versioning (existing — `VersionRange.MaxVersion` / `IsMaxInclusive` properties used)
- 002-startup-security-check: Added C# / .NET 10 (Umbraco 17) + Umbraco.Cms 17.x (existing) — `UmbracoApplicationStartedNotification` from `Umbraco.Cms.Core.Notifications`
- 001-vulnerability-dashboard: Added C# / .NET 10 (Umbraco 17) + Umbraco.Cms 17.x, Swashbuckle.AspNetCore, NuGet.Versioning, Lit 3, Vite 5, @umbraco-ui/uui, @umbraco-cms/backoffice

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->

<!-- SPECKIT START -->
For additional context about technologies to be used, project structure,
shell commands, and other important information, read the current plan
<!-- SPECKIT END -->
