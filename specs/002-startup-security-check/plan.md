# Implementation Plan: Startup Vulnerability Check

**Branch**: `002-startup-security-check` | **Date**: 2026-04-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-startup-security-check/spec.md`

## Summary

On application startup, a notification handler evaluates whether the last successful
vulnerability check is more than 24 hours old (or has never run). If so, it fires
`IVulnerabilityService.RunCheckAsync` in a background task — non-blocking to startup.
A shared `CheckInterval` constant on `VulnerabilityService` keeps the startup threshold
and the scheduled job period in sync. A concurrency guard (`Interlocked.CompareExchange`)
on `VulnerabilityService` ensures only one check runs at a time, covering all trigger
sources.

## Technical Context

**Language/Version**: C# / .NET 10 (Umbraco 17)
**Primary Dependencies**: Umbraco.Cms 17.x (existing) — `UmbracoApplicationStartedNotification` from `Umbraco.Cms.Core.Notifications`
**Storage**: Umbraco database (SQL Server / SQLite) via NPoco + IScopeProvider (existing)
**Testing**: xUnit, NSubstitute (existing)
**Target Platform**: .NET 10 web server
**Project Type**: Umbraco NuGet package — class library extension (existing)
**Performance Goals**: Startup handler returns in <10ms (DB query only); check itself is unchanged
**Constraints**: Non-blocking startup; zero new NuGet dependencies; no schema changes
**Scale/Scope**: Single Umbraco instance per process; one concurrent check maximum

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Clean Code & Simplicity | ✅ PASS | One new class (`StartupVulnerabilityCheckHandler`) with a single responsibility; concurrency guard added in one location (`VulnerabilityService`); shared constant removes duplication |
| II. Minimal External Dependencies | ✅ PASS | No new NuGet packages; `UmbracoApplicationStartedNotification` is part of `Umbraco.Cms.Core` already referenced |
| III. Test-First Development | ✅ PASS | Unit tests for `StartupVulnerabilityCheckHandler` and concurrency guard in `VulnerabilityService` are defined before implementation tasks |
| IV. Umbraco UX Consistency | ✅ PASS | No UI changes; feature is entirely server-side |
| V. Security by Design | ✅ PASS | No new attack surface; startup handler only calls existing `RunCheckAsync`; no new inputs, no new permissions |

**Complexity Tracking**: No violations. No new dependencies.

## Project Structure

### Documentation (this feature)

```text
specs/002-startup-security-check/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

No `contracts/` directory — this feature adds no new API endpoints or public interfaces.

### Source Code (changes only)

```text
src/
  Umbraco.SecurityDashboard/
    Scheduling/
      StartupVulnerabilityCheckHandler.cs   # NEW — INotificationAsyncHandler<UmbracoApplicationStartedNotification>
      VulnerabilityCheckTask.cs             # MODIFIED — Period references VulnerabilityService.CheckInterval
    Services/
      VulnerabilityService.cs               # MODIFIED — add CheckInterval const + _checkInProgress guard
    Composers/
      SecurityDashboardComposer.cs          # MODIFIED — register StartupVulnerabilityCheckHandler

tests/
  Umbraco.SecurityDashboard.Tests/
    Scheduling/
      StartupVulnerabilityCheckHandlerTests.cs   # NEW
    Services/
      VulnerabilityServiceTests.cs               # MODIFIED — add concurrency tests
```

## Phase 0: Research

See [research.md](research.md). All unknowns resolved:

- **Startup hook**: `INotificationAsyncHandler<UmbracoApplicationStartedNotification>` with `Task.Run` fire-and-forget for the actual check.
- **Concurrency guard**: `Interlocked.CompareExchange` on `_checkInProgress: int` in `VulnerabilityService` — skip semantics.
- **Threshold constant**: `VulnerabilityService.CheckInterval = TimeSpan.FromHours(24)` shared by handler and scheduled task.

## Phase 1: Design

### StartupVulnerabilityCheckHandler

```
Class:  StartupVulnerabilityCheckHandler
Implements: INotificationAsyncHandler<UmbracoApplicationStartedNotification>
Dependencies: IVulnerabilityCheckRepository, IVulnerabilityService, ILogger

HandleAsync(notification, cancellationToken):
  1. latestSuccess = await _repository.GetLatestSuccessfulCheckAsync()
  2. if latestSuccess is not null AND latestSuccess.CheckedAt >= UtcNow - CheckInterval:
       log debug "Startup check skipped — last check was {elapsed} ago"
       return
  3. log information "Startup check triggered — last successful check was {elapsed} ago (or never)"
  4. _ = Task.Run(() => _vulnerabilityService.RunCheckAsync(CancellationToken.None))
```

Key points:
- Step 2 short-circuits if a check ran within the threshold. A `null` result (never checked) passes through to step 3.
- `Task.Run` without `await` makes the handler non-blocking. `RunCheckAsync` handles all exceptions internally and persists failures.
- `CancellationToken.None` is intentional: the check should complete even if the startup notification pipeline is cancelled.

### VulnerabilityService changes

```
Add: public static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);
Add: private int _checkInProgress = 0;

Modify RunCheckAsync:
  At entry: if Interlocked.CompareExchange(ref _checkInProgress, 1, 0) != 0:
              log information "Check already in progress — skipping"
              return
  In finally: Interlocked.Exchange(ref _checkInProgress, 0)
```

### VulnerabilityCheckTask change

```
Period property: return VulnerabilityService.CheckInterval;
  (was: TimeSpan.FromHours(24))
```

### SecurityDashboardComposer change

```
Add: builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification,
         StartupVulnerabilityCheckHandler>();
```

### Test plan

`StartupVulnerabilityCheckHandlerTests`:

| Test | Condition | Expected |
|------|-----------|----------|
| `HandleAsync_WhenNeverChecked_RunsCheck` | `GetLatestSuccessfulCheckAsync` returns null | `RunCheckAsync` called (via fire-and-forget; use `Task.Delay` in mock to confirm invoked) |
| `HandleAsync_WhenLastCheckOver24hAgo_RunsCheck` | `CheckedAt = UtcNow - 25h` | `RunCheckAsync` called |
| `HandleAsync_WhenLastCheckUnder24hAgo_SkipsCheck` | `CheckedAt = UtcNow - 23h` | `RunCheckAsync` NOT called |
| `HandleAsync_WhenLastCheckExactly24hAgo_RunsCheck` | `CheckedAt = UtcNow - 24h` | `RunCheckAsync` called (boundary: >24h old, so 24h exactly should run) |

`VulnerabilityServiceTests` additions:

| Test | Condition | Expected |
|------|-----------|----------|
| `RunCheckAsync_WhenAlreadyRunning_SkipsSecondCall` | Two concurrent calls | Second call returns immediately; GitHub client called once |

## Re-evaluated Constitution Check (post-design)

All five principles still pass. Design is minimal: one new class, three small modifications to existing classes, no new dependencies, no schema changes.
