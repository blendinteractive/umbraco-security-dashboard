# Quickstart: Manual Vulnerability Mitigation Marking

## What's Being Built

Two API endpoints (POST + DELETE) and supporting UI so administrators can manually mark advisories as mitigated and later remove that marking.

## Back-End Quickstart

### 1. Add the DB model

Create `src/Umbraco.SecurityDashboard/Models/Db/ManualMitigationRecord.cs` — NPoco POCO with `[TableName("SecurityDashboard_ManualMitigation")]`.

### 2. Add the migration

Create `src/Umbraco.SecurityDashboard/Migrations/AddManualMitigationTable.cs` and register it as a new step in `SecurityDashboardMigrationPlan.cs`:

```csharp
From("SecurityDashboard-1.0.0")
    .To<AddManualMitigationTable>("SecurityDashboard-1.1.0");
```

### 3. Add the repository

Create `IMitigationRepository` with three methods:
- `GetAllMitigationsAsync()` → loads the full table (volume is always small)
- `CreateMitigationAsync(ManualMitigationRecord)` → insert; throws on UNIQUE violation
- `DeleteMitigationAsync(ghsaId)` → delete by GhsaId; returns bool (true = deleted, false = not found)

Register `IMitigationRepository → MitigationRepository` as `AddScoped` in `SecurityDashboardComposer`.

### 4. Extend the service

In `VulnerabilityService.GetDashboardStatusAsync()`, after building the `AdvisoryDto` list, fetch all mitigation records and join by `GhsaId`:

```csharp
var mitigations = await _mitigationRepository.GetAllMitigationsAsync();
var mitigationMap = mitigations.ToDictionary(m => m.GhsaId, StringComparer.OrdinalIgnoreCase);

foreach (var advisory in ordered)
{
    if (mitigationMap.TryGetValue(advisory.GhsaId, out var m))
    {
        advisory.AffectedStatus = "Mitigated";
        advisory.ManualMitigation = new ManualMitigationDto { ... };
    }
}
```

Recalculate `affectedCount`, `mitigatedCount`, and `overallStatus` after applying the overlay.

### 5. Add the API endpoints

In `SecurityDashboardController`, add:

```csharp
[HttpPost("advisories/{ghsaId}/mitigations")]
public async Task<IActionResult> CreateMitigation(string ghsaId, [FromBody] CreateMitigationRequest request) { ... }

[HttpDelete("advisories/{ghsaId}/mitigations")]
public async Task<IActionResult> DeleteMitigation(string ghsaId) { ... }
```

Get the current user via `IBackOfficeSecurityAccessor` injected into the controller.

## Front-End Quickstart

### 1. Extend `types.ts`

Add `ManualMitigationDto` interface and add `manualMitigation: ManualMitigationDto | null` to `AdvisoryDto`.

### 2. Create `mitigation-dialog.element.ts`

A Lit component that:
- Accepts `@property() mode: 'mark' | 'remove'` and `@property() ghsaId: string`
- Renders a `<uui-dialog>` with either a description textarea (mark) or a confirmation prompt (remove)
- Emits `mitigation-changed` custom event on success, `mitigation-cancelled` on cancel

### 3. Update `advisory-item.element.ts`

- Import and render `<security-dashboard-mitigation-dialog>` conditionally
- Add a "Mark As Mitigated" `<uui-button>` for Vulnerable/Unknown advisories without an existing mitigation
- Add mitigation attribution display (who, when, description) for Mitigated advisories
- Add a "Remove Mitigation" `<uui-button>` for Mitigated advisories
- Listen for `mitigation-changed` to bubble up to the parent (relay the event so `security-dashboard.element.ts` can re-fetch status)

### 4. Update `security-dashboard.element.ts`

Listen for `mitigation-changed` event on the host and call `_fetchStatus()` to refresh the data.

## Running Locally

```bash
# Build and watch frontend
cd client && npm run dev

# Run the Umbraco demo site
cd demo && dotnet run
```

The migration runs automatically on first startup after adding the new migration step. No manual SQL required.

## Key Acceptance Tests to Verify Manually

1. Mark an advisory in Vulnerable state → it shows Mitigated with attribution after page refresh
2. Try to submit with empty description → submission is blocked
3. Mark then remove → advisory reverts to Vulnerable
4. Two browser tabs: both admins can remove any mitigation
5. Mark an advisory, restart the site, rescan — mitigation persists
