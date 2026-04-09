# Data Model: Startup Vulnerability Check

**Branch**: `002-startup-security-check` | **Date**: 2026-04-02

## Schema Changes

**None.** This feature introduces no new database tables, columns, or migrations.

The startup check writes results using the same `CheckResultRecord` and `AdvisoryRecord`
entities already defined by feature `001-vulnerability-dashboard`. The dashboard API and
UI read those entities unchanged.

## Existing Entities Used

### CheckResultRecord (`SecurityDashboard_CheckResult`)

| Column | Type | Relevance |
|--------|------|-----------|
| `Id` | int (PK, auto) | — |
| `CheckedAt` | DateTime | **Read** by startup handler to evaluate the 24-hour condition; **written** by `RunCheckAsync` on completion |
| `Succeeded` | bool | Only successful records (`Succeeded = true`) are considered when evaluating the 24-hour threshold |
| `ErrorMessage` | string? | Written on failure, same as scheduled check |
| `NextScheduledCheckAt` | DateTime | Computed by `RunCheckAsync` as next 4 AM; written by startup check the same way as by the scheduled check |

### AdvisoryRecord (`SecurityDashboard_Advisory`)

Unchanged. Written by `RunCheckAsync` regardless of whether it was triggered by startup
or by the scheduler.

## In-memory State (no persistence)

| Field | Type | Location | Purpose |
|-------|------|----------|---------|
| `_checkInProgress` | `int` (0 or 1) | `VulnerabilityService` | Concurrency guard — prevents simultaneous checks on the same instance |

This field lives only in memory and resets to `0` on process restart, which is correct:
a fresh process restart always re-evaluates the 24-hour condition from the database.
