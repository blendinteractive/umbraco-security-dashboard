# Quickstart: 009-exposure-mitigation-desc

## What this feature adds

Exposure checks that return `Mitigated` now also supply a plain-language description. These descriptions are stored in the database and shown on the dashboard so administrators can see *why* an advisory is auto-mitigated without leaving the dashboard.

## End-to-end flow

1. A scan runs. For each advisory, `ExposureCheckEvaluator.EvaluateAsync` calls all matching exposure checks.
2. Each check returns an `ExposureCheckResult` — verdict plus optional description.
3. The evaluator joins descriptions from all `Mitigated` results with `"; "` and returns an `ExposureEvaluationResult`.
4. `VulnerabilityService` stores the description in `AdvisoryRecord.ExposureCheckDescription`.
5. `GET /status` maps `ExposureCheckDescription` → `AdvisoryDto.ExposureCheckMitigationDescription`.
6. The front-end `advisory-item` component renders a description box when `affectedStatus == "Mitigated"` and `manualMitigation` is absent.

## Running locally

```bash
# From repo root
dotnet run --project src/Umbraco.SecurityDashboard.Demo
# Then visit http://localhost:5000/umbraco and trigger a manual scan
```

To force exposure-check overrides in development (appsettings.Development.json):

```json
"SecurityDashboard": {
  "Development": {
    "ExposureCheckOverrides": {
      "GHSA-xxxx-xxxx-xxxx": ["Content Delivery API"]
    }
  }
}
```

## Running tests

```bash
dotnet test tests/Umbraco.SecurityDashboard.Tests
```

Key test files for this feature:
- `tests/.../Services/Exposure/ExposureCheckEvaluatorTests.cs` — evaluator combining/fallback logic
- `tests/.../Services/Exposure/Checks/ContentDeliveryApiExposureCheckTests.cs` — description returned when Mitigated
- `tests/.../Services/Exposure/Checks/NonAdminUsersExposureCheckTests.cs` — description returned when Mitigated

## Key files changed

| File | Change |
|------|--------|
| `src/.../Services/Exposure/ExposureCheckResult.cs` | NEW — return type for individual checks |
| `src/.../Services/Exposure/ExposureEvaluationResult.cs` | NEW — return type for evaluator |
| `src/.../Services/Exposure/IExposureCheck.cs` | `CheckAsync` returns `Task<ExposureCheckResult>` |
| `src/.../Services/Exposure/IExposureCheckEvaluator.cs` | `EvaluateAsync` returns `Task<ExposureEvaluationResult>` |
| `src/.../Services/Exposure/ExposureCheckEvaluator.cs` | Collects and joins descriptions; applies fallback |
| `src/.../Services/Exposure/Checks/ContentDeliveryApiExposureCheck.cs` | Returns `"Content Delivery API is disabled"` when Mitigated |
| `src/.../Services/Exposure/Checks/NonAdminUsersExposureCheck.cs` | Returns `"All backoffice users are administrators"` when Mitigated |
| `src/.../Models/Db/AdvisoryRecord.cs` | Add `ExposureCheckDescription` property |
| `src/.../Models/Api/AdvisoryDto.cs` | Add `ExposureCheckMitigationDescription` property |
| `src/.../Migrations/AddExposureCheckDescriptionColumn.cs` | NEW — adds column (SecurityDashboard-1.3.0) |
| `src/.../Migrations/SecurityDashboardMigrationPlan.cs` | Register new step |
| `src/.../Services/VulnerabilityService.cs` | Store + map description |
| `client/src/types.ts` | Add `exposureCheckMitigationDescription` |
| `client/src/components/advisory-item.element.ts` | Render description for auto-mitigated advisories |
