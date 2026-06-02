# API Contract Diff: 008-scan-schedule-config

## GET /umbraco/backoffice/security-dashboard/status

**Change**: One new field added to the response body. All existing fields unchanged.

### Before

```json
{
  "overallStatus": "Safe",
  "isStale": false,
  "lastSuccessfulCheckAt": "2026-06-01T04:00:12Z",
  "lastCheckAttemptAt": "2026-06-01T04:00:15Z",
  "lastCheckSucceeded": true,
  "lastCheckError": null,
  "nextScheduledCheckAt": "2026-06-02T04:00:00Z",
  "affectedAdvisoryCount": 0,
  "mitigatedAdvisoryCount": 0,
  "advisories": []
}
```

### After

```json
{
  "overallStatus": "Safe",
  "isStale": false,
  "lastSuccessfulCheckAt": "2026-06-01T04:00:12Z",
  "lastCheckAttemptAt": "2026-06-01T04:00:15Z",
  "lastCheckSucceeded": true,
  "lastCheckError": null,
  "nextScheduledCheckAt": "2026-06-02T04:00:00Z",
  "affectedAdvisoryCount": 0,
  "mitigatedAdvisoryCount": 0,
  "scanningDisabled": false,
  "advisories": []
}
```

**`scanningDisabled`**: `boolean`. `true` when `Umbraco:SecurityDashboard:ScanSchedule:Frequency` is `"Disabled"`. When `true`, `nextScheduledCheckAt` is not meaningful.

---

## Configuration Schema

### Umbraco:SecurityDashboard:ScanSchedule

New nested section. All fields have defaults; the section is optional.

```jsonc
"ScanSchedule": {
  // "Daily" (default) | "Weekly" | "Disabled"
  "Frequency": "Daily",

  // Hour of day for the scan. Range: 0–23. Default: 4.
  "Hour": 4,

  // Minute of the hour for the scan. Range: 0–59. Default: 0.
  "Minute": 0,

  // Day of week (Weekly only). One of: "Sunday" | "Monday" | ... | "Saturday".
  // Default: "Monday". Ignored when Frequency is not "Weekly".
  "DayOfWeek": "Monday"
}
```

### Example — Daily at 2:30 AM

```json
"Umbraco": {
  "SecurityDashboard": {
    "ScanSchedule": {
      "Frequency": "Daily",
      "Hour": 2,
      "Minute": 30
    }
  }
}
```

### Example — Weekly on Monday at 3:00 AM

```json
"Umbraco": {
  "SecurityDashboard": {
    "ScanSchedule": {
      "Frequency": "Weekly",
      "Hour": 3,
      "Minute": 0,
      "DayOfWeek": "Monday"
    }
  }
}
```

### Example — Disabled (development)

```json
"Umbraco": {
  "SecurityDashboard": {
    "ScanSchedule": {
      "Frequency": "Disabled"
    }
  }
}
```

### Validation Errors (thrown at startup as `OptionsValidationException`)

| Config Value | Error |
|---|---|
| `Hour: 24` | `ScanSchedule.Hour must be between 0 and 23.` |
| `Minute: 60` | `ScanSchedule.Minute must be between 0 and 59.` |
| `Frequency: "Hourly"` | `ScanSchedule.Frequency 'Hourly' is not valid. Use Daily, Weekly, or Disabled.` |
| `DayOfWeek: "Someday"` | `ScanSchedule.DayOfWeek 'Someday' is not a valid day of week.` |

---

## Backward Compatibility

- Omitting `ScanSchedule` entirely is equivalent to `{ "Frequency": "Daily", "Hour": 4, "Minute": 0 }` — preserves current behavior (SC-005).
- All existing fields in the status response are unchanged in name, type, and semantics.
- `isStale` threshold now adapts to the configured frequency (48 h for Daily, 9 days for Weekly) — this is a behavioral change, but not a breaking API change.
