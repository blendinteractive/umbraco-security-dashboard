# Implementation Plan: Filter Irrelevant Version Advisories

**Branch**: `003-filter-irrelevant-advisories` | **Date**: 2026-04-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-filter-irrelevant-advisories/spec.md`

## Summary

During each vulnerability check run, advisory entries classified as "NotAffected" are
now evaluated against an additional relevance test: if the advisory's entire version
range falls strictly below the currently installed package version, the entry is skipped
and not stored. A single new private method (`IsObsoleteForInstalledVersion`) is added
to `VulnerabilityService` and called inside the existing advisory-building loop.
No schema changes, no display-path changes, no new dependencies.

## Technical Context

**Language/Version**: C# / .NET 10 (Umbraco 17)
**Primary Dependencies**: NuGet.Versioning (existing — `VersionRange.MaxVersion` / `IsMaxInclusive` properties used)
**Storage**: Umbraco database via NPoco (existing — no schema change)
**Testing**: xUnit, NSubstitute (existing)
**Target Platform**: .NET 10 web server (existing)
**Project Type**: Umbraco NuGet package (existing)
**Performance Goals**: No measurable impact — relevance check is a simple in-memory comparison per advisory entry
**Constraints**: No new external dependencies; `DetermineAffectedStatus` method signature unchanged
**Scale/Scope**: One new private method; one modified foreach loop; new unit tests

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Code & Simplicity | ✅ PASS | One new private method with a single responsibility; the advisory-building loop gains one conditional `continue`; no new abstraction layers |
| II. Minimal External Dependencies | ✅ PASS | Uses `NuGet.Versioning.VersionRange` properties already in the dependency graph; no new packages |
| III. Test-First Development | ✅ PASS | Unit tests for `IsObsoleteForInstalledVersion` and integration-level tests for `RunCheckAsync` filtering written before implementation |
| IV. Umbraco UX Consistency | ✅ PASS | No UI changes |
| V. Security by Design | ✅ PASS | No new inputs, no new permissions, no new external calls |

**Complexity Tracking**: No violations. No new dependencies.

## Project Structure

### Documentation (this feature)

```text
specs/003-filter-irrelevant-advisories/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

No `contracts/` directory — no API contract changes (response shape is unchanged;
the dashboard receives fewer advisory entries but via the same endpoint and DTO).

### Source Code (changes only)

```text
src/
  Umbraco.SecurityDashboard/
    Services/
      VulnerabilityService.cs    # MODIFIED — add IsObsoleteForInstalledVersion + call in RunCheckAsync

tests/
  Umbraco.SecurityDashboard.Tests/
    Services/
      VulnerabilityServiceTests.cs    # MODIFIED — add filtering tests
```

## Phase 0: Research

See [research.md](research.md). No unknowns remain:

- **Relevance test**: Compare `VersionRange.MaxVersion` (already parsed) to the installed
  `NuGetVersion`. Exclude if installed > MaxVersion (inclusive) or installed >= MaxVersion
  (exclusive). If MaxVersion is null, keep.
- **Filter location**: Check time, inside the `RunCheckAsync` advisory-building loop.
  Only `NotAffected` entries enter the check.
- **Method placement**: New private static `IsObsoleteForInstalledVersion(string? rangeString, string installedVersionString)` on `VulnerabilityService`.

## Phase 1: Design

### `IsObsoleteForInstalledVersion` method

```
static bool IsObsoleteForInstalledVersion(string? rangeString, string installedVersionString):
  range = VersionRangeParser.Parse(rangeString)
  if range is null → return false   // can't determine → keep
  if range.MaxVersion is null → return false   // open-ended above → keep
  if NOT NuGetVersion.TryParse(installedVersionString, out version) → return false  // can't parse → keep
  if range.IsMaxInclusive: return version > range.MaxVersion
  else:                    return version >= range.MaxVersion
```

This returns `true` only when we are certain the installed version is above the entire
affected range.

### `RunCheckAsync` modification

After the existing `DetermineAffectedStatus` call, add:

```
if affectedStatus == "NotAffected"
   AND installedVersion is not null
   AND IsObsoleteForInstalledVersion(vuln.VulnerableVersionRange, installedVersion):
     continue   // skip — advisory is for a lower version, do not store
```

The `installedVersion` variable is assigned immediately after `DetermineAffectedStatus`
in the existing code and is safe to use here.

### Test plan

New tests in `VulnerabilityServiceTests.cs` (all via mocked `IGitHubAdvisoryClient`
and `IInstalledPackageProvider`):

| Test | Range | Installed | Expected stored count |
|------|-------|-----------|-----------------------|
| `RunCheckAsync_OldVersionAdvisory_IsExcluded` | `>= 16.0, < 17.0` | `17.1.0` | 0 |
| `RunCheckAsync_SpanningAdvisory_IsIncluded` | `>= 16.0, < 17.5` | `17.1.0` | 1 (Affected) |
| `RunCheckAsync_FutureAdvisory_IsIncluded` | `>= 17.5` | `17.1.0` | 1 (NotAffected) |
| `RunCheckAsync_UnknownRangeAdvisory_IsIncluded` | `"all"` (unparseable) | `17.1.0` | 1 (Unknown) |
| `RunCheckAsync_ExactVersionBelow_IsExcluded` | `= 16.5.0` | `17.1.0` | 0 |
| `RunCheckAsync_ExactVersionMatch_IsIncluded` | `= 17.1.0` | `17.1.0` | 1 (Affected) |

## Re-evaluated Constitution Check (post-design)

All five principles still pass. The design is the minimum viable change: one private
method, one conditional `continue`, new tests. No abstractions, no new types, no
schema migration.
