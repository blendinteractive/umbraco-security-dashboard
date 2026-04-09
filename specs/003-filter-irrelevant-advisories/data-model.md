# Data Model: Filter Irrelevant Version Advisories

**Branch**: `003-filter-irrelevant-advisories` | **Date**: 2026-04-09

## Schema Changes

**None.** No new tables, columns, or migrations.

The filtering is applied before advisory records are written; the stored data simply
contains fewer rows. The schema of `SecurityDashboard_Advisory` and
`SecurityDashboard_CheckResult` is unchanged.

## Existing Entities — Behavioural Changes

### AdvisoryRecord (`SecurityDashboard_Advisory`)

No structural change. The effect of this feature is that after a check run, rows with
`AffectedStatus = 'NotAffected'` where the installed version exceeds the advisory's
upper version bound will no longer be inserted. The stored row count decreases; the
row shape is identical.

## Relevance Logic (in-memory, no persistence)

| Input | Source | Role |
|-------|--------|------|
| `VulnerableVersionRange` | `GitHubAdvisory.Vulnerabilities[n].VulnerableVersionRange` | Parsed to extract `MaxVersion` and `IsMaxInclusive` |
| Installed version string | `InstalledPackageProvider` dictionary | Parsed to `NuGetVersion` for comparison |
| `AffectedStatus` | Result of `DetermineAffectedStatus` | Gate — only `"NotAffected"` entries enter the obsolescence check |

The relevance decision is transient — computed once during each check run and not stored.
