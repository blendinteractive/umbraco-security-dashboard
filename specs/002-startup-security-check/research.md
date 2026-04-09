# Research: Startup Vulnerability Check

**Branch**: `002-startup-security-check` | **Date**: 2026-04-02

## Decision 1: Umbraco 17 startup hook pattern

**Decision**: Use `INotificationAsyncHandler<UmbracoApplicationStartedNotification>`

**Rationale**: `UmbracoApplicationStartedNotification` fires after Umbraco has fully
initialised — database migrations are complete, all services are resolved, and the DI
container is ready. This is the correct point at which to query `GetLatestSuccessfulCheckAsync`
(which requires the database) and conditionally start the check. It is the Umbraco-native
pattern for post-startup hooks and requires no additional packages.

The handler itself awaits the fast database query, then uses `Task.Run` to fire
`RunCheckAsync` without awaiting it, so the notification pipeline returns immediately and
does not delay the site becoming available (satisfying FR-004).

**Alternatives considered**:

- `IHostedService` (.NET BCL): Fires before Umbraco migrations complete; the database
  is not guaranteed to be ready at that point. Rejected.
- `IApplicationEventHandler` (legacy Umbraco): Deprecated in current Umbraco; not
  available in Umbraco 17. Rejected.
- `IUmbracoApplicationStartingNotification`: Fires before Umbraco has finished startup;
  database not ready. Rejected.

---

## Decision 2: Concurrency guard mechanism

**Decision**: `Interlocked.CompareExchange` on a shared `int` field (`_checkInProgress`)
in `VulnerabilityService`, with "skip" semantics.

**Rationale**: FR-006 requires that no two checks run concurrently on the same instance,
and that a scheduled job skips its run when a startup check is in progress (and vice
versa). "Skip" (return immediately if the slot is taken) rather than "wait" is correct
here — waiting would block the scheduled job's thread for the entire check duration.

`Interlocked.CompareExchange` is a BCL primitive — no external dependency, no lock
allocation, and the pattern is well-understood. The flag is set to `1` at the start of
`RunCheckAsync` and reset to `0` in a `finally` block, ensuring the slot is always
released even on exception.

Because both the startup handler and the scheduled `VulnerabilityCheckTask` call the
same `RunCheckAsync` method, the guard covers all concurrency scenarios from a single
code path.

**Alternatives considered**:

- `SemaphoreSlim(1,1)` with `Wait(0)`: Also correct, but slightly heavier allocation
  and `WaitAsync(0)` returns `false` on failure requiring the caller to handle it.
  Functionally equivalent; `Interlocked` is marginally simpler for this use case. Rejected.
- No guard (rely on timing alone): The 24-hour threshold already prevents most
  redundant runs, but a guard is still needed for the edge case where the scheduled
  job fires within seconds of a startup check (e.g., startup at 03:59:55, scheduled
  job fires at 04:00:00 while check is still in progress). Rejected.

---

## Decision 3: Threshold source of truth

**Decision**: The `VulnerabilityService` will expose a `public const TimeSpan CheckInterval`
(24 hours) that both the startup handler and the `VulnerabilityCheckTask.Period` property
reference, rather than two independent hardcoded values.

**Rationale**: The spec assumption states "if the schedule period changes, the startup
threshold should be updated to match." A shared constant makes this a single-point
change and eliminates the risk of the two values drifting. `VulnerabilityCheckTask.Period`
currently returns `TimeSpan.FromHours(24)` directly — this can be replaced with a
reference to the shared constant.

**Alternatives considered**:

- Keep values independent (both hardcoded to 24 hours): Works today but creates a
  latent drift risk. Rejected.
- Read from configuration: Adds complexity and a configuration key with no current
  requirement to be user-configurable. Rejected.
