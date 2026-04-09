# Research: Filter Irrelevant Version Advisories

**Branch**: `003-filter-irrelevant-advisories` | **Date**: 2026-04-09

## Decision 1: Relevance test — how to determine "entirely below installed version"

**Decision**: Compare the parsed `VersionRange.MaxVersion` against the installed version.
If `MaxVersion` is non-null and the installed version is strictly above it, the advisory
is irrelevant and should be excluded.

**Rationale**: `NuGet.Versioning.VersionRange` (already used by `VersionRangeParser`)
exposes `MaxVersion` and `IsMaxInclusive` directly. No additional parsing is needed.
The relevance test is:

```
installed > MaxVersion          (when MaxVersion is inclusive)
installed >= MaxVersion         (when MaxVersion is exclusive)
```

In both cases: if the installed version is above the range's ceiling, the advisory
cannot affect this installation. If `MaxVersion` is null (no upper bound), the range
extends indefinitely and always remains potentially relevant — no exclusion.

Existing cases handled correctly:
- Range `>= 17.0, < 17.5`, installed `17.1` → MaxVersion = 17.5 (exclusive), 17.1 < 17.5 → NOT excluded (Affected)
- Range `>= 16.0, < 17.0`, installed `17.1` → MaxVersion = 17.0 (exclusive), 17.1 >= 17.0 → EXCLUDED
- Range `>= 17.5`, installed `17.1` → MaxVersion = null → NOT excluded (NotAffected, kept)
- Range `= 16.5`, installed `17.1` → MaxVersion = 16.5 (inclusive), 17.1 > 16.5 → EXCLUDED
- Range unparseable → AffectedStatus = "Unknown" → never reaches relevance check

**Alternatives considered**:

- Comparing major version numbers only (e.g., discard if advisory's max major < installed
  major): simpler but less precise — an advisory for `< 17.0.1` on Umbraco `17.0.0`
  would be incorrectly excluded. Rejected in favour of the exact version comparison.

---

## Decision 2: Where to apply the filter (check time vs display time)

**Decision**: Filter at check time — exclude irrelevant entries before writing to
the database. Irrelevant advisories are never persisted.

**Rationale**: The spec assumption explicitly states filtering happens at check time.
This is cleaner: smaller stored dataset, no filtering logic needed in the display path,
and no changes to `GetDashboardStatusAsync`. The dashboard always reads what is stored,
so the display path remains untouched.

**Alternatives considered**:

- Filter at display time (in `GetDashboardStatusAsync`): preserves raw data but adds
  filtering to the read path and means irrelevant advisories accumulate in the database.
  Rejected.

---

## Decision 3: Where to place the new relevance logic

**Decision**: Add a new private static method `IsObsoleteForInstalledVersion` to
`VulnerabilityService`. Call it immediately after `DetermineAffectedStatus` returns
`"NotAffected"` in the `RunCheckAsync` foreach loop. Only `NotAffected` entries pass
through this check; `Affected` and `Unknown` entries skip it entirely.

**Rationale**: Keeps `DetermineAffectedStatus` single-responsibility (classifying
affected/not/unknown); the new method is responsible only for the obsolescence check.
The call site is the advisory-building loop in `RunCheckAsync`, which already has access
to the parsed range string and installed version.

Concretely, the logic in `RunCheckAsync` becomes:

```
if affectedStatus == "NotAffected" AND IsObsoleteForInstalledVersion(range, installed):
    continue  // skip — do not add to advisoryRecords
```

`IsObsoleteForInstalledVersion` receives the range string and installed version string
(both already available at the call site) and returns `true` only when exclusion is safe:
range successfully parsed, version successfully parsed, and installed > upper bound.

**Alternatives considered**:

- Adding a fourth return value to `DetermineAffectedStatus` (e.g., `"Obsolete"`): would
  require changing the method signature and all callers. Rejected — the new method keeps
  the change minimal.
