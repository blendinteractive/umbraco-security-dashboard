# Feature Specification: Exposure Check Mitigation Descriptions

**Feature Branch**: `009-exposure-mitigation-desc`  
**Created**: 2026-06-10  
**Status**: Draft  
**Input**: User description: "When a vulnerability is marked as mitigated by an exposure check, it should save a mitigation description so the user knows why the vulnerability is considered mitigated."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Why Vulnerability Is Mitigated (Priority: P1)

A security administrator views the vulnerability dashboard and sees one or more advisories marked as "Mitigated." They want to understand *why* each advisory has that status — whether it is because the Content Delivery API is disabled, there are no non-admin backoffice users, or some other configured check determined the site is not exposed.

**Why this priority**: Without a description, "Mitigated" is opaque. Administrators cannot verify or communicate the reason to stakeholders, and they may not notice if a configuration change later invalidates the mitigation.

**Independent Test**: Can be fully tested by running a vulnerability scan where at least one exposure check returns Mitigated, then viewing that advisory in the dashboard and confirming a plain-language description is shown explaining the reason.

**Acceptance Scenarios**:

1. **Given** a vulnerability scan has completed and an exposure check determined an advisory is mitigated, **When** the administrator views the advisory details on the dashboard, **Then** a description is shown explaining the specific reason the vulnerability is considered mitigated (e.g., "Content Delivery API is disabled").
2. **Given** an advisory is mitigated by an exposure check, **When** the administrator views the advisory, **Then** the description is human-readable and matches the condition evaluated by the check.
3. **Given** an advisory is mitigated manually (not by an exposure check), **When** the administrator views the advisory, **Then** the manually entered description continues to appear as before (no regression).

---

### User Story 2 - Mitigation Description Persisted Across Page Loads (Priority: P2)

A security administrator leaves the dashboard and returns later. Previously mitigated advisories should still show their mitigation descriptions without requiring another scan.

**Why this priority**: Descriptions must survive page navigation and be readable from stored scan results, not recomputed live on each view.

**Independent Test**: Can be fully tested by completing a scan, navigating away, returning to the dashboard, and confirming descriptions are still present for exposure-mitigated advisories.

**Acceptance Scenarios**:

1. **Given** a scan completed with exposure-check mitigation descriptions, **When** the administrator navigates away and returns to the dashboard, **Then** the same mitigation descriptions are still displayed.
2. **Given** a new scan is triggered and an exposure check now returns "Vulnerable" instead of "Mitigated," **When** the administrator views the dashboard, **Then** the description from the prior scan is no longer shown for that advisory.

---

### Edge Cases

- What happens when an exposure check returns "Mitigated" but provides no description? The advisory should still show "Mitigated" status but display a generic fallback (e.g., "Mitigated by exposure check").
- What happens when multiple exposure checks all return "Mitigated" for a single advisory? A combined or representative description should be shown rather than silently discarding details.
- What happens when a vulnerability is mitigated by both a manual mitigation and an exposure check? The manual mitigation description takes precedence (matching existing priority behavior).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Each exposure check MUST be able to provide a human-readable description of why it considers a vulnerability mitigated.
- **FR-002**: When the vulnerability scan records an advisory with a "Mitigated" status determined by an exposure check, the system MUST persist the corresponding mitigation description alongside the advisory result.
- **FR-003**: The dashboard MUST display the mitigation description for any advisory whose status was set to "Mitigated" by an exposure check.
- **FR-004**: The mitigation description MUST be stored as part of the scan result so it remains available after the scan completes without requiring a live re-evaluation.
- **FR-005**: Existing manual mitigation descriptions MUST continue to display correctly and take precedence when both a manual mitigation and an exposure-check mitigation exist for the same advisory.
- **FR-006**: If an exposure check determines "Mitigated" but does not supply a description, the system MUST display a sensible fallback description rather than a blank or missing value.

### Key Entities

- **Exposure Check Result**: The outcome produced by a single exposure check for a given advisory — includes the verdict (Mitigated / Vulnerable / NotAffected) and, when Mitigated, a plain-language description of the reason.
- **Advisory Record**: Stored result of a scan for a specific advisory/package combination — gains a field to hold the exposure-check mitigation description when applicable.
- **Mitigation Description**: A short, human-readable string (e.g., "Content Delivery API is disabled") explaining the configuration condition that eliminates exposure to this vulnerability.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of advisories marked "Mitigated" by an exposure check display a non-blank description on the dashboard.
- **SC-002**: Administrators can determine the reason for any exposure-check mitigation without leaving the dashboard or consulting external documentation.
- **SC-003**: No regressions: advisories mitigated manually continue to display their existing description without change.
- **SC-004**: Mitigation descriptions are available immediately after a completed scan, with no additional user action required.
- **SC-005**: When the configuration condition that caused mitigation changes (e.g., Content Delivery API is re-enabled), the description disappears after the next scan and the advisory returns to its correct status.

## Assumptions

- Each exposure check knows its own mitigation reason and can express it as a short string; no external lookup or translation layer is needed.
- The description is static per check verdict (the same check always produces the same description when returning "Mitigated") — it does not vary per advisory or per scan run.
- The description is stored in the scan result, not computed on each dashboard load, so it does not require the exposure checks to be re-run on every page view.
- The feature does not add a mitigation description for the "Vulnerable" or "NotAffected" verdicts — only "Mitigated" receives a description.
- The UI treatment for an exposure-check mitigation description matches or closely resembles the existing manual mitigation description display, minimizing UI complexity.
- Mobile / small-screen support is out of scope; the dashboard already targets desktop backoffice users.
