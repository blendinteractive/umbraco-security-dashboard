# Research: Configurable Vulnerability Scan Schedule

## 1. Current Scheduling Mechanism

**Decision**: Extend the existing `IRecurringBackgroundJob` pattern — do not introduce a new scheduling library.

**Findings**:
- `VulnerabilityCheckTask` implements `IRecurringBackgroundJob` with `Period = TimeSpan.FromHours(24)` and `Delay` computed from `ComputeNext4Am()` (local time → UTC).
- `StartupVulnerabilityCheckHandler` listens to `UmbracoApplicationStartedNotification` and fires an immediate check if last success was more than `VulnerabilityService.CheckInterval` (24 h) ago.
- Both the Period and the stale threshold are currently static constants on `VulnerabilityService`.

**Rationale**: The `IRecurringBackgroundJob.Period` field is read once by Umbraco's scheduler at startup. Injecting `IOptions<SecurityDashboardSettings>` into `VulnerabilityCheckTask` and reading the value at property access time is sufficient — no hot-reload is required per spec.

**Alternatives considered**: Background service / `System.Threading.Timer` — rejected; Umbraco's own `IRecurringBackgroundJob` abstraction is the established pattern in this codebase.

---

## 2. Configuration Validation at Startup

**Decision**: `IValidateOptions<SecurityDashboardSettings>` + `ValidateOnStart()` registered in the composer.

**Findings**:
- .NET provides `IValidateOptions<T>` for eager validation of `IOptions` registrations. When combined with `.ValidateOnStart()`, the validator runs during `IHost.StartAsync()` and throws `OptionsValidationException` before Umbraco fully starts.
- The exception message lists all violations concatenated, satisfying FR-005 ("clear errors … does not silently fall back").
- Registration: `builder.Services.AddOptions<SecurityDashboardSettings>().ValidateOnStart().Services.AddSingleton<IValidateOptions<SecurityDashboardSettings>, SecurityDashboardSettingsValidator>();` (or via the extension `.Validate(...)` builder).

**Rationale**: No external validation library needed — `IValidateOptions` is BCL.

**Alternatives considered**: `DataAnnotations` on the POCO — rejected; min/max values on `DayOfWeek` and cross-field validation (DayOfWeek only required for Weekly) are cleaner as imperative code.

---

## 3. Conditionally Disabling the Background Job

**Decision**: Read frequency from `IConfiguration` at compose-time and skip `AddRecurringBackgroundJob<VulnerabilityCheckTask>()` when `Disabled`.

**Findings**:
- In `SecurityDashboardComposer.Compose()`, `builder.Config` is available as `IConfiguration`. Reading `Umbraco:SecurityDashboard:ScanSchedule:Frequency` directly (string comparison) at compose time is straightforward.
- Not registering the background job at all is cleaner than registering a no-op or an infinite-period task.
- `StartupVulnerabilityCheckHandler` also skips its check when `Disabled` (guarded by injected settings, not compose-time config, so it handles late-binding edge cases correctly).

**Alternatives considered**: `Period = TimeSpan.MaxValue` hack — rejected; semantically wrong and fragile if Umbraco's scheduler has an upper bound.

---

## 4. Weekly Schedule Computation

**Decision**: `ScanSchedule.ComputeNextOccurrence` walks forward day-by-day until the matching `DayOfWeek` is found, then applies the configured hour/minute.

**Findings**:
- `DateTime.Now.DayOfWeek` returns `System.DayOfWeek` (Sunday=0 … Saturday=6). The configured `DayOfWeek` property on `ScanScheduleSettings` is typed as `System.DayOfWeek`.
- Edge case: if today is the configured day and the time hasn't passed, return today at the configured time. If today is the configured day and the time has already passed, return the same day next week. This matches the spec's acceptance scenarios for User Story 2.
- Local-time computation (matching existing `ComputeNext4Am` behavior) → convert to UTC before returning.

**Alternatives considered**: Cron expression parsing — rejected; a full cron library is overkill for two simple recurrence patterns and would add an external dependency.

---

## 5. Stale Threshold Derivation

**Decision**: `ScanSchedule.GetStaleThreshold(settings)` returns `TimeSpan.FromHours(48)` for Daily, `TimeSpan.FromDays(9)` for Weekly, and `TimeSpan.FromDays(9)` for Disabled (Disabled never triggers a stale check in practice since `IsStale` is only surfaced alongside a result).

**Findings**:
- FR-009 specifies 48 h for Daily, 9 days for Weekly. Disabled has no automatic checks, so the threshold is irrelevant — using the Weekly value is safe.
- `VulnerabilityService` already holds `_settings` (via `IOptions<SecurityDashboardSettings>`), so it can call `ScanSchedule.GetStaleThreshold(_settings.ScanSchedule)` without any new injection.

---

## 6. Dashboard `ScanningDisabled` Flag

**Decision**: Add `bool ScanningDisabled` to `DashboardStatusResponse` (C#) and `scanningDisabled: boolean` to the TypeScript interface.

**Findings**:
- FR-010 requires a "prominent warning" when `Frequency = Disabled`. Surfacing the flag via the existing status endpoint is the least-invasive approach — no new endpoint needed.
- The frontend `staleness-warning.element.ts` already renders conditional warning blocks; adding a `scanningDisabled` property follows the same pattern.
- `IsStale` behavior when `Disabled`: `VulnerabilityService` should set `IsStale = false` when scanning is disabled (a stale warning alongside a disabled warning would be redundant noise).

---

## 7. `VulnerabilityService.CheckInterval` Public Static

**Findings**:
- `StartupVulnerabilityCheckHandler` references `VulnerabilityService.CheckInterval` directly (line 28 of the current file).
- Three test files (`StartupVulnerabilityCheckHandlerTests`) also reference it.
- After this feature, `StartupVulnerabilityCheckHandler` will inject `IOptions<SecurityDashboardSettings>` and use `ScanSchedule.GetCheckInterval(settings.ScanSchedule)` instead.
- `VulnerabilityService.CheckInterval` can be removed; tests will be updated to pass the configured period directly.

---

## 8. No DB Migration Required

**Findings**:
- `NextScheduledCheckAt` is already a column on the `CheckResultRecord` table (from feature 001). The computed next-run time stored there will simply reflect the new schedule logic — no schema change needed.
