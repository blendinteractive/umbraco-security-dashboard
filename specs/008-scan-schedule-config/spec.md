# Feature Specification: Configurable Vulnerability Scan Schedule

**Feature Branch**: `008-scan-schedule-config`  
**Created**: 2026-06-02  
**Status**: Draft  
**Input**: User description: "We should allow developers to set the schedule for vulnerability checks in the appSettings. Let's add options for daily or weekly scans, and for setting the day of scan and time of day by hour and minute"

## Clarifications

### Session 2026-06-02

- Q: If a scheduled check was missed because the app was down, should it run immediately on the next startup, or wait for the next scheduled occurrence? → A: Run immediately on startup if the last check is older than one full configured period (24 h for Daily, 7 days for Weekly).
- Q: Should the dashboard stale-warning threshold adapt to the configured schedule frequency? → A: Auto-derive — Daily uses a 48-hour threshold, Weekly uses a 9-day threshold.
- Q: Should a `Disabled` frequency option be supported to suppress automatic scanning entirely? → A: Yes — support `Disabled` as a valid frequency value; when set, no automatic checks run and the dashboard displays a prominent warning that scanning is disabled.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Configure Daily Scan Time (Priority: P1)

A developer wants vulnerability checks to run every day at a specific time that suits their team (e.g., 2 AM to avoid peak load). They open `appsettings.json` and set the schedule to daily with a specific hour and minute. After saving and restarting, the system runs checks at the configured time each day.

**Why this priority**: Most teams use daily scans. Allowing the time to be configured is the core value of this feature and covers the majority of use cases.

**Independent Test**: Can be tested by configuring a daily schedule with a custom hour and minute, then verifying the next scheduled check time reflects the configured values.

**Acceptance Scenarios**:

1. **Given** no schedule is configured in appSettings, **When** the system runs, **Then** it defaults to a daily scan at 4:00 AM (preserving existing behavior).
2. **Given** a daily schedule is configured with hour=2 and minute=30, **When** the system runs, **Then** the next scheduled check is calculated as 2:30 AM of the next applicable day.
3. **Given** a daily schedule is configured, **When** the configured time is in the past for today, **Then** the next scheduled check is set to the configured time the following day.
4. **Given** a daily schedule is configured with invalid values (hour > 23 or minute > 59), **When** the application starts, **Then** a clear configuration error is reported and the application does not silently fall back.

---

### User Story 2 - Configure Weekly Scan on a Specific Day (Priority: P2)

A developer wants vulnerability checks to run once per week — say, every Monday morning at 3:00 AM. They configure the schedule as weekly, specify Monday as the scan day, and set the hour and minute. The system then only triggers checks on that day each week.

**Why this priority**: Weekly scans are a common choice for lower-traffic or compliance-oriented teams. Configuring the day of week is essential for this mode to be useful.

**Independent Test**: Can be tested by configuring a weekly schedule for a specific day, then verifying the next scheduled check time falls on the correct day of the week at the correct time.

**Acceptance Scenarios**:

1. **Given** a weekly schedule is configured for Monday at 3:00 AM, **When** the system calculates the next check, **Then** it returns the next upcoming Monday at 3:00 AM.
2. **Given** a weekly schedule is configured and today is the configured day but the time has already passed, **When** the system calculates the next check, **Then** it returns the same day of next week at the configured time.
3. **Given** a weekly schedule is configured and today is the configured day and the time has not yet passed, **When** the system calculates the next check, **Then** it returns today at the configured time.
4. **Given** a weekly schedule is configured with an invalid day value, **When** the application starts, **Then** a clear configuration error is reported.

---

### User Story 3 - View Scheduled Check Time in Dashboard (Priority: P3)

A developer visits the security dashboard and can see when the next vulnerability check is scheduled, confirming that their configuration change has taken effect without needing to inspect logs.

**Why this priority**: The dashboard already displays `NextScheduledCheckAt`. This story verifies that the displayed time reflects the newly configured schedule rather than the old hardcoded 4 AM value.

**Independent Test**: Can be tested by changing the schedule configuration, restarting the application, and confirming the "next check" time shown in the dashboard matches the configured schedule.

**Acceptance Scenarios**:

1. **Given** a custom schedule is configured, **When** the dashboard loads, **Then** the next scheduled check time reflects the configured schedule.
2. **Given** the default (no configuration) is in use, **When** the dashboard loads, **Then** the next scheduled check time shows 4:00 AM (the default).

---

### User Story 4 - Disable Automatic Scanning in Dev Environments (Priority: P3)

A developer working locally sets the frequency to `Disabled` in `appsettings.Development.json` to prevent background checks from running during development. The dashboard clearly indicates that automatic scanning is off, so the developer is aware and can trigger a manual check if needed.

**Why this priority**: Useful for reducing noise in development, but not needed for production use cases.

**Independent Test**: Can be tested by setting frequency to `Disabled`, restarting, and confirming no scan runs and the dashboard shows a warning.

**Acceptance Scenarios**:

1. **Given** frequency is set to `Disabled`, **When** the application starts, **Then** no automatic vulnerability check runs on startup.
2. **Given** frequency is set to `Disabled`, **When** the dashboard loads, **Then** a prominent warning is displayed indicating that automatic scanning has been disabled.
3. **Given** frequency is set to `Disabled`, **When** the application is running, **Then** no scheduled check is ever triggered automatically.

---

### Edge Cases

- What happens when the system restarts and the configured scan time has already passed today (daily) or this week (weekly)? — If the last check was more than one configured period ago (24 h / 7 days), run immediately; otherwise calculate the next occurrence normally.
- What happens when hour=0 and minute=0 (midnight) is configured? — Should be treated as a valid time (00:00).
- What happens when a weekly scan is scheduled for the same day every time the app restarts mid-week? — The system should not re-trigger a check that already ran this week.
- What happens when the schedule configuration is changed while the application is running? — The new schedule takes effect on the next application restart (configuration is read at startup).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST support a configurable scan frequency of `Daily`, `Weekly`, or `Disabled`.
- **FR-002**: The system MUST allow the scan time to be specified by hour (0–23) and minute (0–59).
- **FR-003**: When frequency is `Weekly`, the system MUST allow the day of the week to be specified (e.g., Monday through Sunday).
- **FR-004**: When no schedule is configured, the system MUST default to a daily scan at 4:00 AM, preserving existing behavior.
- **FR-005**: The system MUST validate schedule configuration values at startup and report clear errors for invalid values (out-of-range hour/minute, unrecognized frequency or day).
- **FR-006**: The calculated next scheduled check time MUST be exposed through the dashboard status response so it can be displayed in the UI.
- **FR-007**: The schedule configuration MUST be set via the existing `appsettings.json` / `appsettings.{Environment}.json` configuration mechanism under the existing `Umbraco:SecurityDashboard` section.
- **FR-010**: When frequency is `Disabled`, the system MUST NOT run automatic vulnerability checks (neither on a schedule nor on startup). The dashboard MUST display a prominent warning indicating that automatic scanning has been disabled.
- **FR-009**: The dashboard stale-warning threshold MUST be derived from the configured frequency: 48 hours for `Daily` schedules and 9 days for `Weekly` schedules. A check is considered stale when the last successful check occurred more than this threshold ago.
- **FR-008**: On application startup, the system MUST run an immediate vulnerability check if the last successful check occurred more than one full configured period ago (24 hours for `Daily`; 7 days for `Weekly`). If the last check is within the configured period, the startup check is skipped and the system waits for the next scheduled occurrence.

### Key Entities

- **Schedule Configuration**: Represents the developer-defined scan schedule. Key attributes: frequency (Daily/Weekly), hour (0–23), minute (0–59), day of week (Monday–Sunday, applicable to Weekly only).
- **Next Scheduled Check**: A computed value derived from the current time and the schedule configuration, stored and surfaced through the dashboard.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can change the scan schedule without modifying code — only an `appsettings.json` change and application restart is required.
- **SC-002**: After configuring a schedule, the next scheduled check time shown in the dashboard matches the configured day and time within a 1-minute margin.
- **SC-003**: The system correctly calculates the next occurrence for both daily and weekly schedules across all valid hour/minute/day combinations (verified via automated tests).
- **SC-004**: Invalid schedule configuration values produce an error at startup rather than silently applying a fallback, ensuring misconfigurations are not hidden from developers.
- **SC-005**: The existing default behavior (daily at 4:00 AM) is preserved when no schedule configuration is provided, so existing deployments require no changes.
- **SC-006**: The dashboard stale warning does not fire for a correctly operating weekly schedule — a check completed within the last 9 days does not trigger the stale indicator.
- **SC-007**: When scanning is disabled, the dashboard displays a visible warning on every load, ensuring no developer overlooks that automatic checks are suppressed.

## Assumptions

- Configuration is read once at application startup; changing `appsettings.json` requires an application restart to take effect (no hot-reload support required).
- The schedule operates in the server's local time zone, consistent with the current `ComputeNext4Am()` behavior.
- Day-of-week configuration is only meaningful when frequency is `Weekly`; for `Daily` it is ignored.
- The `Weekly` frequency runs the scan once per week on the configured day; there is no support for multiple days per week in this feature.
- The existing `VulnerabilityCheckTask` recurring background job mechanism is used to poll whether a check is due; the schedule configuration controls the computed next-run time, not the polling interval itself.
- No UI in the Umbraco backoffice is needed to configure the schedule — `appsettings.json` is the sole configuration surface.
