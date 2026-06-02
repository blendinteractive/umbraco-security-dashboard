# Data Model: Configurable Vulnerability Scan Schedule

## No DB Schema Change

`NextScheduledCheckAt` is already persisted on `CheckResultRecord`. The schedule logic only changes how that value is computed — no migration is needed.

---

## New: `ScanFrequency` Enum

```csharp
// Configuration/ScanFrequency.cs
namespace Umbraco.SecurityDashboard.Configuration;

public enum ScanFrequency
{
    Daily,
    Weekly,
    Disabled
}
```

---

## New: `ScanScheduleSettings` Class

```csharp
// Configuration/ScanScheduleSettings.cs
namespace Umbraco.SecurityDashboard.Configuration;

public class ScanScheduleSettings
{
    /// <summary>Daily (default) | Weekly | Disabled</summary>
    public ScanFrequency Frequency { get; set; } = ScanFrequency.Daily;

    /// <summary>Hour of day for the scan (0–23). Default: 4.</summary>
    public int Hour { get; set; } = 4;

    /// <summary>Minute of the hour for the scan (0–59). Default: 0.</summary>
    public int Minute { get; set; } = 0;

    /// <summary>Day of week for weekly scans. Default: Monday. Ignored for Daily/Disabled.</summary>
    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
}
```

**Validation rules** (enforced at startup by `SecurityDashboardSettingsValidator`):
- `Hour` ∈ [0, 23]
- `Minute` ∈ [0, 59]
- `DayOfWeek` ∈ valid `System.DayOfWeek` values (0–6)
- `Frequency` ∈ `ScanFrequency` enum values

---

## Modified: `SecurityDashboardSettings`

Add one property:

```csharp
public ScanScheduleSettings ScanSchedule { get; set; } = new();
```

This makes the config section `Umbraco:SecurityDashboard:ScanSchedule`.

---

## New: `ScanSchedule` Static Helper

```csharp
// Scheduling/ScanSchedule.cs
namespace Umbraco.SecurityDashboard.Scheduling;

public static class ScanSchedule
{
    /// <summary>
    /// Computes the next UTC scan occurrence from <paramref name="from"/> (local time; defaults to now).
    /// Returns DateTime.MaxValue when Disabled.
    /// </summary>
    public static DateTime ComputeNextOccurrence(ScanScheduleSettings settings, DateTime? from = null)

    /// <summary>Recurrence interval used for Period in IRecurringBackgroundJob and startup check.</summary>
    public static TimeSpan GetCheckInterval(ScanScheduleSettings settings)
        // Daily → 24 h; Weekly → 7 days; Disabled → TimeSpan.MaxValue

    /// <summary>Age threshold beyond which a check result is considered stale.</summary>
    public static TimeSpan GetStaleThreshold(ScanScheduleSettings settings)
        // Daily → 48 h; Weekly → 9 days; Disabled → 9 days (irrelevant in practice)
}
```

**`ComputeNextOccurrence` algorithm**:

_Daily_:
1. `candidate = from.Date.AddHours(settings.Hour).AddMinutes(settings.Minute)`
2. If `from >= candidate`, advance by 1 day
3. Return `candidate.ToUniversalTime()`

_Weekly_:
1. `candidate = from.Date.AddHours(settings.Hour).AddMinutes(settings.Minute)`
2. Walk forward (0–6 days) until `candidate.DayOfWeek == settings.DayOfWeek`
3. If `candidate <= from`, add 7 days
4. Return `candidate.ToUniversalTime()`

_Disabled_:
- Return `DateTime.MaxValue`

---

## Modified: `DashboardStatusResponse`

Add one property:

```csharp
/// <summary>True when automatic scanning has been disabled via configuration.</summary>
public bool ScanningDisabled { get; set; }
```

---

## Modified: TypeScript `DashboardStatusResponse`

```typescript
// client/src/types.ts — add to DashboardStatusResponse interface
scanningDisabled: boolean;
```

---

## Modified: `staleness-warning.element.ts`

Add `@property({ type: Boolean }) scanningDisabled = false;`

New render block (rendered independently of `isStale`):

```html
<div class="scanning-disabled-warning">
  <strong>Automatic scanning is disabled</strong> — 
  set <code>ScanSchedule.Frequency</code> to <code>Daily</code> or <code>Weekly</code> 
  in <code>appsettings.json</code> to enable scheduled checks.
</div>
```

Styled with `--uui-color-danger-surface` / `--uui-color-danger` tokens to make it prominent (matching the failure-notice pattern).

When `scanningDisabled = true`, suppress the `isStale` warning (redundant).

---

## State Transitions

```
App starts
  ├─ Frequency = Disabled
  │   ├─ StartupVulnerabilityCheckHandler: SKIP (log warning)
  │   ├─ VulnerabilityCheckTask: NOT REGISTERED
  │   └─ DashboardStatusResponse.ScanningDisabled = true
  │
  ├─ Frequency = Daily | Weekly
  │   ├─ StartupVulnerabilityCheckHandler: run if last check > checkInterval ago
  │   ├─ VulnerabilityCheckTask: Period = checkInterval, Delay = time until next occurrence
  │   └─ DashboardStatusResponse.ScanningDisabled = false
```
