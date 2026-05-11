# Feature Specification: Security Audit Log

**Feature Branch**: `007-audit-log`  
**Created**: 2026-05-11  
**Status**: Draft  

## Clarifications

### Session 2026-05-11

- Q: What are the valid overall vulnerability states? → A: `Safe`, `Mitigated`, `Vulnerable`, `NeverChecked`
- Q: Should the first vulnerability check (NeverChecked → new state) produce an audit entry? → A: Yes — the transition away from NeverChecked is a state change and must be logged.
- Q: Who can access the audit log view? → A: Any back-office user with access to the Security Dashboard section (not restricted to Umbraco Administrator group).
- Q: Should each audit entry include a human-readable description of what triggered the change? → A: Yes — each entry stores a short description (e.g., which CVE was marked/unmarked, or that a scheduled scan ran).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Administrator Reviews Audit History (Priority: P1)

An administrator wants to understand how the site's overall vulnerability state has changed over time. They open the security dashboard and navigate to the audit log, where they see a chronological history of every state change — when it happened, what the new state was, whether it was triggered automatically by a vulnerability scan or manually by a user, and who made the change if it was manual.

**Why this priority**: Without visibility into the history, administrators cannot answer questions like "when did we become vulnerable?", "who cleared the flag?", or "was this an automated detection or a human decision?" The audit log is the primary accountability mechanism for the feature.

**Independent Test**: Can be fully tested by seeding the audit log with sample entries and verifying the admin UI displays them in correct chronological order with all required fields.

**Acceptance Scenarios**:

1. **Given** there are audit log entries, **When** a Security Dashboard user opens the audit history view, **Then** they see a list of entries ordered by date descending, each showing: date/time, resulting vulnerability state, action type (Manual or Automatic), actor name (for manual entries), and a description of the triggering action.
2. **Given** there are many entries, **When** an administrator browses the history, **Then** the list is paginated so performance is not degraded by large history volumes.
3. **Given** there are no audit log entries yet, **When** an administrator opens the history view, **Then** they see an appropriate empty-state message rather than a blank page.

---

### User Story 2 - System Logs Automatic State Changes (Priority: P1)

When the scheduled vulnerability check runs and determines that the overall vulnerability state has changed (e.g., from Safe to Vulnerable, or Vulnerable to Safe), the system automatically records an audit entry capturing the new state, the timestamp, and that the cause was an automated check — with no user attribution.

**Why this priority**: Automatic detection events are the primary trigger for state changes. Logging them faithfully is the foundation of the audit trail.

**Independent Test**: Can be fully tested by triggering a vulnerability check that changes the overall state and verifying a new audit entry is created with action type "Automatic" and no actor.

**Acceptance Scenarios**:

1. **Given** the overall vulnerability state is Safe, **When** a vulnerability check runs and detects a new vulnerability, **Then** an audit entry is created with the new state (Vulnerable), action type Automatic, timestamp of detection, and no actor.
2. **Given** the overall vulnerability state is Vulnerable, **When** a vulnerability check runs and finds no remaining vulnerabilities, **Then** an audit entry is created with the new state (Safe), action type Automatic, and no actor.
3. **Given** no vulnerability check has ever run (state is NeverChecked), **When** the first vulnerability check completes, **Then** an audit entry is created recording the transition from NeverChecked to the new state, with action type Automatic and no actor.
4. **Given** a vulnerability check runs and the overall state has not changed, **Then** no new audit entry is created for that check run.

---

### User Story 3 - System Logs Manual Mitigation Changes (Priority: P1)

When a user marks a vulnerability as mitigated or removes a mitigation marking, the system records an audit entry — regardless of whether that individual action changes the overall state. The entry captures who made the change, when, and the overall vulnerability state at that moment.

**Why this priority**: Manual actions are user-attributed decisions. Logging them, even when they don't flip the overall state, provides the accountability trail required for security governance.

**Independent Test**: Can be fully tested by marking/unmarking a mitigation as a known user and verifying audit entries are created with correct actor attribution and current overall state.

**Acceptance Scenarios**:

1. **Given** a logged-in user marks a vulnerability as mitigated, **When** the action is saved, **Then** an audit entry is created with action type Manual, the actor's identity, the timestamp, and the resulting overall vulnerability state.
2. **Given** a logged-in user removes a mitigation marking, **When** the action is saved, **Then** an audit entry is created with action type Manual, the actor's identity, the timestamp, and the resulting overall vulnerability state.
3. **Given** a manual action changes the overall vulnerability state, **Then** the audit entry reflects the new overall state (not the pre-action state).

---

### User Story 4 - Webhook Fires on Every Overall State Change (Priority: P2)

Whenever the overall vulnerability state changes — whether triggered by an automated check or a manual user action — the configured update webhook fires so external monitoring systems are notified in real time.

**Why this priority**: External monitors depend on webhook notifications to respond to state changes. This extends the existing webhook mechanism to cover manual state changes, not just automated ones. It is P2 because it depends on the overall state change detection already present in Story 2 and 3.

**Independent Test**: Can be fully tested by configuring a test webhook endpoint, triggering a state change (both automatic and manual), and verifying the webhook fires exactly once per state change.

**Acceptance Scenarios**:

1. **Given** a webhook URL is configured, **When** an automated vulnerability check changes the overall state, **Then** the webhook fires with the new state.
2. **Given** a webhook URL is configured, **When** a user's manual mitigation action changes the overall state, **Then** the webhook fires with the new state.
3. **Given** a webhook URL is configured, **When** a user's manual action does not change the overall state, **Then** the webhook does not fire.
4. **Given** no webhook URL is configured, **When** the overall state changes, **Then** no webhook attempt is made and no error is logged.

---

### Edge Cases

- What happens when a manual action and an automatic check occur near-simultaneously and both change the state? Each change is logged independently as a separate audit entry.
- What if the actor's account is deleted after they made a manual change? The audit entry retains the actor name as it was at the time of the action.
- What happens if the webhook endpoint is unreachable? The failure is logged but does not block the state change or the audit entry creation.
- What if the database is unavailable when an audit entry should be written? The action proceeds; the failure to write the audit entry is logged to the application error log.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST record an audit entry whenever the overall vulnerability state changes, including the first transition away from `NeverChecked`, regardless of whether the cause is automatic or manual.
- **FR-002**: Each audit entry MUST capture: date and time of the change, the resulting overall vulnerability state, whether the cause was Manual or Automatic, the actor's identity when the cause is Manual, and a short human-readable description of the triggering action (e.g., "Marked CVE-2024-1234 as mitigated", "Scheduled vulnerability check completed").
- **FR-003**: System MUST record an audit entry for every user-driven mitigation marking or unmarking action, including those that do not change the overall state.
- **FR-004**: Audit entries MUST be immutable once written — no editing or deletion through normal application flows.
- **FR-005**: The security dashboard MUST provide an audit history view, accessible to any back-office user who has access to the Security Dashboard section, that displays all audit entries in reverse-chronological order.
- **FR-006**: The audit history view MUST display for each entry: date/time, overall vulnerability state, action type (Manual or Automatic), actor name (Manual entries only), and the triggering action description.
- **FR-007**: The audit history view MUST support pagination to remain performant with large numbers of entries.
- **FR-008**: System MUST fire the configured update webhook exactly once each time the overall vulnerability state changes, whether caused by an automatic check or a manual action.
- **FR-009**: Webhook firing MUST NOT be triggered by manual actions that do not change the overall vulnerability state.
- **FR-010**: Webhook failures MUST NOT prevent the audit entry from being written or the state change from being persisted.

### Key Entities

- **Audit Log Entry**: A single immutable record of a state-changing or user-driven event. Key attributes: timestamp, overall vulnerability state at time of entry, action type (Manual | Automatic), actor identity (present only for Manual entries), and a required short description of the triggering action (e.g., "Marked CVE-2024-1234 as mitigated", "Removed mitigation for CVE-2024-5678", "Scheduled vulnerability check completed").
- **Overall Vulnerability State**: The aggregate security posture of the site derived from all active vulnerability findings and manual mitigations. Valid values: `Safe` (no active vulnerabilities), `Mitigated` (all vulnerabilities have manual mitigations applied), `Vulnerable` (unmitigated vulnerabilities exist), `NeverChecked` (no vulnerability check has ever run). This is the value captured in each audit entry.
- **Actor**: The authenticated user who performed a manual action. Stored by display name and/or identifier at the time of the action.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Every overall state change — whether from an automatic check or a manual action — produces an audit log entry within the same request/operation that caused the change, with no missed events.
- **SC-002**: Every manual mitigation marking or unmarking action produces an audit log entry, even when the overall state is unchanged.
- **SC-003**: Administrators can load and browse the full audit history without any page taking more than 2 seconds to display, regardless of log size.
- **SC-004**: The webhook fires for 100% of overall state changes and for 0% of non-state-changing manual actions.
- **SC-005**: The audit history view is accessible to any back-office user with Security Dashboard section access; users without that section access cannot reach the view.

## Assumptions

- The concept of "overall vulnerability state" is already defined by earlier features and is a single derived value (`Safe`, `Mitigated`, `Vulnerable`, or `NeverChecked`) that can be read after any change.
- Actor identity for manual entries is the currently authenticated back-office user; anonymous or unauthenticated changes are not possible for manual actions.
- The update webhook URL and any authentication configuration already exist from feature 004; this feature extends when it fires, not how it is configured.
- Audit log entries are append-only at the application level; database-level retention and archiving policies are out of scope for this feature.
- The audit history view is limited to the Umbraco back-office and is not exposed publicly or via the Delivery API.
- Performance targets assume typical Umbraco back-office usage (tens of administrators, not thousands of concurrent users).
