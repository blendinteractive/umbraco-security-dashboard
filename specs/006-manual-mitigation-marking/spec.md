# Feature Specification: Manual Vulnerability Mitigation Marking

**Feature Branch**: `006-manual-mitigation-marking`  
**Created**: 2026-05-11  
**Status**: Draft  

## Clarifications

### Session 2026-05-11

- Q: Which advisory states should show the "Mark As Mitigated" button? → A: Only advisories in the "Vulnerable" state. Advisories in the "NotAffected" state must NOT show the button.
- Q: Should advisories in the "Unknown" state also show the "Mark As Mitigated" button? → A: Yes — both "Vulnerable" and "Unknown" advisories show the button.
- Q: Should the removal of a manual mitigation itself be audited (who removed it, when)? → A: No — the record is simply deleted. Removal audit logging is deferred to a future feature.
- Q: Can any administrator remove any manual mitigation, or only the one who created it? → A: Any administrator can remove any manual mitigation.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Mark Vulnerability as Mitigated (Priority: P1)

An administrator is reviewing the security dashboard and sees an advisory that has already been addressed — perhaps through a configuration change, network isolation, or compensating control. They click "Mark As Mitigated" on that advisory, enter a description explaining what was done to resolve or mitigate the risk, and confirm. The advisory is now shown as Mitigated in the dashboard, with the description, the administrator's name, and the date/time of the action recorded and visible.

**Why this priority**: This is the core capability of the feature. Without it, nothing else in this feature has value.

**Independent Test**: Can be fully tested by clicking "Mark As Mitigated" on any advisory, submitting a description, and confirming that the advisory status changes and the mitigation record is stored with correct attribution.

**Acceptance Scenarios**:

1. **Given** an advisory item in the "Vulnerable" or "Unknown" state is visible in the dashboard, **When** an administrator clicks "Mark As Mitigated", **Then** a prompt appears asking for a description of the resolution
2. **Given** the mitigation prompt is open, **When** the administrator submits a non-empty description, **Then** the advisory status changes to Mitigated and the record stores the description, the timestamp, and the current user's identity
3. **Given** the mitigation prompt is open, **When** the administrator attempts to submit with an empty description, **Then** submission is prevented and an error message prompts them to provide a description
4. **Given** an advisory has been manually marked as mitigated, **When** any administrator views the dashboard, **Then** the advisory displays its Mitigated status along with the resolution description, the name of the administrator who marked it, and the date/time it was marked

---

### User Story 2 - Remove Manual Mitigation (Priority: P2)

An administrator realizes that a previously applied manual mitigation is no longer valid — perhaps circumstances changed or it was applied in error. They find the advisory (now showing as Mitigated) and click the option to remove the manual mitigation. After confirming, the advisory reverts to its automatically calculated status.

**Why this priority**: Without the ability to remove a manual mitigation, administrators can paint themselves into a corner, hiding real vulnerabilities permanently.

**Independent Test**: Can be fully tested by first marking an advisory as mitigated, then removing that mitigation, and confirming the advisory reverts to its prior calculated status.

**Acceptance Scenarios**:

1. **Given** an advisory is manually marked as mitigated, **When** an administrator views the advisory, **Then** an option to remove the manual mitigation is clearly visible
2. **Given** an administrator clicks to remove a manual mitigation, **When** they confirm the action, **Then** the mitigation record is deleted and the advisory status reverts to its automatically calculated value
3. **Given** an administrator clicks to remove a manual mitigation, **When** they cancel the action, **Then** the advisory remains marked as mitigated with no changes

---

### Edge Cases

- What happens when an advisory already marked as manually mitigated is clicked for "Mark As Mitigated" again? (Assumed: not possible — button should not be visible on already-mitigated items)
- What happens if the administrator who originally set the mitigation has since been deleted? (Assumed: the stored name/identity remains visible as a string; it is not a live lookup)
- What if two administrators attempt to mark the same advisory at the same time? (Assumed: last write wins; no special conflict resolution required)
- Does removing a manual mitigation affect other advisories? (No — mitigation records are scoped per advisory item)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Advisory items in the "Vulnerable" or "Unknown" state MUST display a "Mark As Mitigated" button (when not already manually mitigated). Advisories in the "NotAffected" state MUST NOT show this button.
- **FR-002**: When an administrator activates "Mark As Mitigated", the system MUST present a prompt requiring a description of the resolution before the action can be completed
- **FR-003**: The description field MUST be required — submission with an empty description MUST be prevented
- **FR-004**: Upon successful submission, the system MUST store a mitigation record containing: the resolution description, the exact date and time the mitigation was recorded, and the identity (name) of the administrator who performed the action
- **FR-005**: After a manual mitigation is recorded, the advisory's displayed status MUST reflect Mitigated
- **FR-006**: Manually mitigated advisories MUST display the stored mitigation details (who, when, description) to any administrator viewing the dashboard
- **FR-007**: Manually mitigated advisories MUST offer an option to remove the manual mitigation
- **FR-008**: When a manual mitigation is removed (and the removal is confirmed), the system MUST delete the mitigation record and revert the advisory's displayed status to its automatically calculated value. No audit record of the removal is stored (deferred to a future audit trail feature).
- **FR-009**: Only users with administrator-level access to the security dashboard MUST be able to create or remove manual mitigations. Any administrator may remove any mitigation, regardless of who created it.

### Key Entities

- **Manual Mitigation Record**: Represents an administrator's deliberate decision to mark a specific advisory as resolved through non-package-update means. Key attributes: the advisory it applies to, a human-readable resolution description, the date and time the record was created, and the name/identity of the administrator who created it. Only one active record per advisory is permitted at a time.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An administrator can mark an advisory as mitigated — from clicking the button to seeing the confirmed Mitigated status — in under 60 seconds
- **SC-002**: All manually mitigated advisories display complete attribution (resolver name, date/time, description) to any administrator on any subsequent visit without data loss
- **SC-003**: Removing a manual mitigation updates the advisory's displayed status without requiring a full page reload
- **SC-004**: The "Mark As Mitigated" option is accessible on every advisory item in the "Vulnerable" or "Unknown" state that is not already manually mitigated
- **SC-005**: An administrator cannot accidentally submit an empty mitigation — the system prevents it 100% of the time

## Assumptions

- The currently logged-in administrator's name or identity is available to the system at the time of marking
- A mitigation description is mandatory to ensure every manual mitigation has a meaningful audit trail
- Only one active manual mitigation record per advisory is supported; marking an already-mitigated advisory again is not permitted (the button is not shown)
- Manual mitigation records persist independently of package version changes — they remain until explicitly removed by an administrator
- All users with access to the security dashboard section are considered administrators for the purposes of this feature; no sub-role distinction is required
- "Remove manual mitigation" requires a confirmation step to prevent accidental removals
- The mitigation details panel (who, when, description) is visible inline on the advisory item, not behind an additional navigation step
