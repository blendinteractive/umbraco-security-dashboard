# Feature Specification: Startup Vulnerability Check

**Feature Branch**: `002-startup-security-check`
**Created**: 2026-04-02
**Status**: Draft
**Input**: User description: "When the site starts up, if there's been no check in the last 24 hours, the security vulnerability service should run its check, regardless of the normal schedule"

## Clarifications

### Session 2026-04-02

- Q: Should the startup check run if no successful check has ever been recorded (first run / fresh install)? → A: Yes — absence of any prior successful check is treated identically to a check that is more than 24 hours old; the startup check runs.
- Q: If a startup check is still in progress when the scheduled daily job fires, should the scheduled job skip that run? → A: Yes — the scheduled job must skip its run if a startup check is already in progress; the startup check result is sufficient.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Catch Missed Checks After Extended Downtime (Priority: P1)

When the site restarts after being offline for longer than 24 hours — due to maintenance,
deployment, or unplanned downtime — the vulnerability check runs automatically during
startup rather than waiting until the next scheduled time (4 AM). Backoffice users see
fresh vulnerability data as soon as possible after the site comes back up.

**Why this priority**: The existing scheduler only fires on a 24-hour cadence from its
last run. If the site is down when the job would have run, the data goes stale with no
recovery until the next scheduled window. This is the primary gap the feature closes.

**Independent Test**: Stop the site, wait until the last recorded check is more than 24
hours old, then restart. Confirm that a new check completes during startup — without
waiting for the next 4 AM window — and that fresh results appear on the dashboard.

**Acceptance Scenarios**:

1. **Given** the last successful vulnerability check completed more than 24 hours ago,
   **When** the site starts up, **Then** the vulnerability check runs automatically
   during startup initialisation.

2. **Given** the last successful vulnerability check completed less than 24 hours ago,
   **When** the site starts up, **Then** no startup check is triggered and the normal
   schedule continues uninterrupted.

3. **Given** no vulnerability check has ever been run (fresh installation),
   **When** the site starts up, **Then** the startup check runs as there is no prior
   check within the last 24 hours.

---

### User Story 2 - Graceful Handling During Startup Check (Priority: P2)

The startup check does not block the site from becoming available to users. The check
runs in the background and the dashboard continues to display the previously stored
results (or the "never checked" state) until the startup check completes.

**Why this priority**: A blocking startup check would degrade site availability and is
inconsistent with how the existing scheduled job behaves. The feature must be additive
without introducing latency to site startup.

**Independent Test**: Trigger a startup check condition and verify that the site becomes
fully available before the check completes, and that the dashboard updates once the
check finishes.

**Acceptance Scenarios**:

1. **Given** a startup check is triggered, **When** the site is starting up, **Then**
   the vulnerability check runs asynchronously and does not delay the site becoming
   accessible to users.

2. **Given** the startup check is in progress, **When** a user opens the dashboard,
   **Then** the previously stored results are shown (or "never checked" state for a
   fresh install) until the check completes.

3. **Given** the startup check fails (e.g., the advisory source is unreachable),
   **Then** the failure is recorded consistently with how a scheduled check failure
   is recorded, and the dashboard shows the failure warning.

---

### Edge Cases

- What happens if the site starts up multiple times in quick succession (e.g., rolling
  restart)? Only one startup check should run per qualifying 24-hour gap — once a check
  completes successfully, subsequent restarts within 24 hours must not re-trigger it.
- What if the startup check and the normal scheduled check collide due to timing?
  The system should avoid running two simultaneous checks; if a check completed within
  the last 24 hours (including one triggered moments earlier), the startup check must
  not fire.
- What if the site is in a multi-server (load-balanced) environment? Each server
  evaluates the condition against the shared last-check timestamp, so only the first
  server to start up will run the check; subsequent servers starting shortly after will
  see a recent successful check and skip it.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: On site startup, the system MUST determine whether the most recently
  completed successful vulnerability check occurred more than 24 hours ago (or has
  never occurred).
- **FR-002**: If the condition in FR-001 is true, the system MUST initiate a
  vulnerability check automatically during startup, independently of the normal
  daily schedule.
- **FR-003**: If the condition in FR-001 is false (a successful check ran within the
  last 24 hours), the system MUST NOT run a startup check; the normal schedule
  continues without modification.
- **FR-004**: The startup-triggered check MUST run asynchronously so it does not delay
  the site becoming available to end users or backoffice users.
- **FR-005**: The startup check MUST record its results (success or failure) using the
  same persistence mechanism as the normally scheduled check, so the dashboard reflects
  the outcome.
- **FR-006**: No two checks MUST run concurrently on the same server instance. If a
  startup check is already in progress when the scheduled daily job fires, the
  scheduled job MUST skip that run. Equally, if a scheduled check is already running
  at startup, no startup check is initiated.

### Key Entities *(include if feature involves data)*

- **VulnerabilityCheckResult**: The outcome of one check run — completion timestamp,
  success/failure status, and advisories found. The startup check writes to the same
  entity as the scheduled check.
- **CheckSchedule**: Tracks when the last successful check ran; the startup logic reads
  this to decide whether a check is needed.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: After a site restart where the last check is more than 24 hours old,
  fresh vulnerability results are available on the dashboard within the time a single
  check takes to complete — no manual intervention required.
- **SC-002**: After a site restart where the last check is less than 24 hours old,
  zero additional checks are triggered; the scheduled job fires at its normal next
  window unaffected.
- **SC-003**: Site startup time is not measurably increased; the check runs in the
  background and does not block site availability.
- **SC-004**: 100% of startup check outcomes (success or failure) are recorded and
  visible on the dashboard, consistent with outcomes from the scheduled check.

## Assumptions

- The 24-hour threshold matches the existing scheduled check period. If the schedule
  period changes in future, the startup threshold should be updated to match.
- "Last successful check" is the authoritative timestamp. A failed check attempt does
  not reset the 24-hour window; only a successful check does.
- The startup check fires once per process start (application host initialisation), not
  once per HTTP request.
- In multi-server environments, the shared record of the last successful check timestamp
  in the database prevents redundant checks — if server A ran a check recently, server B
  starting up shortly after will not re-run it.
- No user-facing notification or additional UI change is required to indicate that a
  startup check was triggered; the existing last-check timestamp and dashboard result
  display is sufficient.
- The startup check is always active when the package is installed; no configuration
  toggle is needed.
