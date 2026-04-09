# Quickstart: Startup Vulnerability Check

**Branch**: `002-startup-security-check`

## What this feature adds

A startup hook that runs the vulnerability check automatically when the site starts up,
if the last successful check is more than 24 hours old (or has never run). The check
runs in the background and does not block site availability.

## Files changed

| File | Change |
|------|--------|
| `src/Umbraco.SecurityDashboard/Scheduling/StartupVulnerabilityCheckHandler.cs` | **New** — notification handler |
| `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs` | **Modified** — add concurrency guard + shared `CheckInterval` constant |
| `src/Umbraco.SecurityDashboard/Scheduling/VulnerabilityCheckTask.cs` | **Modified** — reference shared `CheckInterval` constant |
| `src/Umbraco.SecurityDashboard/Composers/SecurityDashboardComposer.cs` | **Modified** — register new handler |
| `tests/.../Scheduling/StartupVulnerabilityCheckHandlerTests.cs` | **New** — unit tests |
| `tests/.../Services/VulnerabilityServiceTests.cs` | **Modified** — concurrency tests |

## Testing locally

### Trigger the startup check (last check > 24h ago)

1. In the test database, update the most recent successful `CheckResult` record so its
   `CheckedAt` is more than 24 hours in the past (or delete all records for a fresh-install
   simulation).
2. Start the site.
3. Observe application logs for `"Startup check triggered — last successful check was X hours ago"`.
4. Wait for the check to complete; confirm `CheckedAt` in the database is updated to ~now.

### Verify no spurious check (last check < 24h ago)

1. Ensure a successful `CheckResult` record exists with `CheckedAt` within the last 24 hours.
2. Start the site.
3. Confirm no startup-check log entry appears and no new `CheckResult` record is written.

### Verify concurrency guard

1. Set `CheckedAt` to > 24 hours ago.
2. Start the site; immediately stop it before the check completes (or use a debugger
   breakpoint in `RunCheckAsync`).
3. The scheduled job timer should show no second concurrent run started.

## Running tests

```bash
dotnet test tests/Umbraco.SecurityDashboard.Tests/
```

All new tests are in `Scheduling/StartupVulnerabilityCheckHandlerTests.cs`.
Concurrency tests are added to `Services/VulnerabilityServiceTests.cs`.
