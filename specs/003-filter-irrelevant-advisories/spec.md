# Feature Specification: Filter Irrelevant Version Advisories

**Feature Branch**: `003-filter-irrelevant-advisories`
**Created**: 2026-04-09
**Status**: Draft
**Input**: User description: "The vulnerability advisory list should only include advisories for the active package versions. If we're running Umbraco version 17, we should include all vulnerabilities that could potentially affect that version. But we should exclude vulnerabilities that can only affect version 16."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - See Only Relevant Advisories for Installed Version (Priority: P1)

A Settings-authorized user opens the Security Dashboard and sees only advisories that
are relevant to the versions of Umbraco packages currently installed. Advisories that
exclusively affect older versions no longer present in the installation do not appear in
the list — not even in the secondary "not affected" section. The dashboard is focused
and actionable rather than cluttered with historical advisories for superseded versions.

**Why this priority**: The primary value of the dashboard is signal quality. An
installation running Umbraco 17 has no use for an advisory that only ever affected
Umbraco 16; showing it alongside genuine concerns erodes trust and makes the dashboard
harder to read. Removing these stale entries is the core change.

**Independent Test**: Given a test dataset containing one advisory affecting only
versions < 17.0 and one advisory affecting versions >= 17.0, open the dashboard with an
Umbraco 17 installation. Confirm only the second advisory appears. The first is absent
from both the affected and the not-affected list.

**Acceptance Scenarios**:

1. **Given** an installation running Umbraco 17.1 and an advisory whose entire affected
   version range is `< 17.0` (e.g., `>= 16.0, < 17.0`), **When** a vulnerability check
   runs and the results are displayed, **Then** that advisory does NOT appear anywhere in
   the dashboard advisory list.

2. **Given** an installation running Umbraco 17.1 and an advisory whose affected version
   range includes 17.x (e.g., `>= 16.0, < 17.5`), **When** the dashboard is viewed,
   **Then** that advisory appears in the appropriate list (affected or not-affected based
   on whether 17.1 falls within the range).

3. **Given** an installation running Umbraco 17.1 and an advisory whose affected version
   range starts above the installed version (e.g., `>= 17.5`), **When** the dashboard
   is viewed, **Then** that advisory appears in the not-affected list (it does not
   currently affect the installation but is relevant to the installed major version line).

4. **Given** an installation running Umbraco 17.1 and an advisory for a package where
   the affected version range cannot be determined, **When** the dashboard is viewed,
   **Then** that advisory appears in the affected list with an "Unknown" badge (unknown
   relevance is treated as a potential risk — existing behaviour unchanged).

---

### User Story 2 - Overall Status Reflects Only Relevant Advisories (Priority: P2)

The red/green overall status indicator on the dashboard is calculated only from
advisories that are relevant to the installed versions. An advisory that cannot affect
the installed version does not contribute to a red status, and excluding such advisories
does not cause a false green status.

**Why this priority**: If the overall status counts were derived from all advisories
(including filtered-out ones), the counts and status could be misleading. This story
ensures the counts and status are internally consistent after filtering.

**Independent Test**: Given only one advisory in the dataset and it exclusively affects
versions below the installed version, confirm the dashboard shows green ("No Active
Vulnerabilities") with an advisory count of zero, and no advisory entries in the list.

**Acceptance Scenarios**:

1. **Given** all advisories in the dataset are irrelevant to the installed version (all
   version ranges fall entirely below the installed version), **When** the dashboard is
   viewed, **Then** the overall status is green ("No Active Vulnerabilities") and the
   advisory count is zero.

2. **Given** a mix of relevant and irrelevant advisories where the relevant ones include
   an advisory affecting the installed version, **When** the dashboard is viewed,
   **Then** the overall status is red and the count reflects only the relevant affected
   advisories — not the excluded ones.

---

### Edge Cases

- What if an advisory's affected version range cannot be parsed (malformed range string)?
  The existing "Unknown" badge behaviour applies — treat unresolvable ranges as
  potentially relevant and keep them in the list.
- What if the installed version of a package cannot be determined? The existing "Unknown"
  badge behaviour applies — the advisory is kept in the affected list.
- What if an advisory spans multiple major versions and includes both old and current
  versions (e.g., `>= 13.0, < 18.0`)? The advisory is relevant and must be included,
  since the range overlaps with the installed version.
- What if an advisory's affected range has no upper bound (e.g., `>= 17.0` with no
  ceiling)? The advisory is relevant to the installed version and must be included.
- What if an advisory is entirely above the installed version (e.g., affects `>= 18.0`
  only, but installed is `17.1`)? The advisory is included in the not-affected list —
  it is part of the active version line and represents a future upgrade consideration.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: During a vulnerability check, the system MUST evaluate whether each
  advisory's affected version range could overlap with or extend above the installed
  package version. Advisories whose entire affected version range falls strictly below
  the installed version MUST be excluded from the stored results for that check.
- **FR-002**: Advisories where the affected version range cannot be determined, or where
  the installed version cannot be determined, MUST be retained (treated as potentially
  relevant). Exclusion applies only when the installed version is known AND the upper
  bound of the affected range is known AND the installed version exceeds that upper bound.
- **FR-003**: The overall vulnerability status (red/green) and affected advisory count
  displayed on the dashboard MUST be computed using only the advisories that pass the
  relevance filter defined in FR-001.
- **FR-004**: Advisories already classified as "Affected" or "Unknown" MUST NOT be
  excluded by this filter (they are by definition relevant).
- **FR-005**: No existing advisory classification ("Affected", "NotAffected", "Unknown")
  or display behaviour changes as a result of this feature, except that irrelevant
  "NotAffected" entries are no longer stored or displayed.

### Key Entities *(include if feature involves data)*

- **VulnerabilityAdvisory**: An advisory record stored per check run. This feature adds
  a relevance determination step before storage — only advisories passing the relevance
  test are written. No new fields are added to the entity.
- **InstalledPackage**: The installed package version is already used for "Affected"
  determination; this feature extends its use to also gate whether an advisory is worth
  storing at all.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After a vulnerability check on an Umbraco 17 installation, zero advisories
  that exclusively affect versions below 17.0 appear anywhere in the dashboard advisory
  list (affected or not-affected).
- **SC-002**: Advisories that affect any version within or above the installed major
  version continue to appear in the list — no relevant advisory is silently dropped.
- **SC-003**: The overall vulnerability status and advisory count are consistent with the
  filtered advisory list; no status discrepancy occurs due to excluded entries.
- **SC-004**: The dashboard loads and displays correctly with zero advisories remaining
  after filtering (all were irrelevant), showing the green "No Active Vulnerabilities"
  state rather than an error or empty-data anomaly.

## Assumptions

- The filtering decision is made at check time (when advisories are fetched and
  evaluated), not at display time. Only relevant advisories are stored; excluded ones
  are never persisted.
- The relevance test uses the same version comparison mechanism already in place for
  determining "Affected" status. No new version-parsing logic is introduced.
- "Strictly below the installed version" means the upper bound of the affected range
  (exclusive or inclusive as declared) is less than or equal to the installed version,
  AND the range has no portion extending above the installed version.
- This filtering applies per-package: each advisory entry is evaluated against the
  installed version of the specific package it references.
- Advisories for packages that are not installed at all are handled by the existing
  "Unknown" classification and are not excluded by this feature.
- The feature applies only to the "NotAffected" classification path. "Affected" and
  "Unknown" advisories are never excluded.
