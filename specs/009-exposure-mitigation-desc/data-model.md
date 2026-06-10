# Data Model: 009-exposure-mitigation-desc

## New C# Records

### `ExposureCheckResult`
**Location**: `src/Umbraco.SecurityDashboard/Services/Exposure/ExposureCheckResult.cs`

```csharp
public record ExposureCheckResult(ExposureVerdict Verdict, string? MitigationDescription = null);
```

Fields:
- `Verdict` — the check outcome (`NotAffected` / `Mitigated` / `Vulnerable`)
- `MitigationDescription` — plain-language reason; only set when `Verdict == Mitigated`, `null` otherwise

Validation rules:
- Description must be `null` when `Verdict != Mitigated` (enforced by convention in each check implementation; not validated at runtime to keep checks simple)

---

### `ExposureEvaluationResult`
**Location**: `src/Umbraco.SecurityDashboard/Services/Exposure/ExposureEvaluationResult.cs`

```csharp
public record ExposureEvaluationResult(string Verdict, string? MitigationDescription);
```

Fields:
- `Verdict` — `"NotAffected"` | `"Mitigated"` | `"Vulnerable"`
- `MitigationDescription` — semicolon-joined descriptions from all checks that returned `Mitigated`; fallback `"Mitigated by exposure check"` when verdict is `Mitigated` but no check supplied a description; `null` when verdict is not `Mitigated`

---

## Modified Types

### `IExposureCheck` — interface change
```csharp
// Before
Task<ExposureVerdict> CheckAsync(CancellationToken cancellationToken = default);

// After
Task<ExposureCheckResult> CheckAsync(CancellationToken cancellationToken = default);
```

### `IExposureCheckEvaluator` — interface change
```csharp
// Before
Task<string> EvaluateAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default);

// After
Task<ExposureEvaluationResult> EvaluateAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default);
```

### `ExposureCheckEvaluator` — implementation change
Key logic:
1. Run all matching checks, collecting `ExposureCheckResult` instances.
2. Apply `RunSafeAsync` exception-handling (unchanged; on exception returns `ExposureCheckResult(Vulnerable)` with null description).
3. Find worst verdict as before.
4. If worst verdict is `Mitigated`:
   - Collect non-null `MitigationDescription` values from results that returned `Mitigated`.
   - Join with `"; "`.
   - If the joined string is empty (all null), use fallback `"Mitigated by exposure check"`.
5. Return `ExposureEvaluationResult(worst.ToString(), description)`.

### `AdvisoryRecord` — new column
```csharp
// Added to SecurityDashboard_Advisory table
public string? ExposureCheckDescription { get; set; }
```

DB column: `ExposureCheckDescription NVARCHAR(MAX) NULL`  
Default for existing rows: `NULL` (migration adds column nullable with no default)

### `AdvisoryDto` — new field
```csharp
public string? ExposureCheckMitigationDescription { get; set; }
```

Populated from `AdvisoryRecord.ExposureCheckDescription` during `GetDashboardStatusAsync`.

### TypeScript `AdvisoryDto` (`client/src/types.ts`)
```typescript
// Added to AdvisoryDto interface
exposureCheckMitigationDescription?: string;
```

---

## Database Migration

**Migration class**: `AddExposureCheckDescriptionColumn`  
**Plan step**: `SecurityDashboard-1.3.0`

```sql
ALTER TABLE SecurityDashboard_Advisory
ADD ExposureCheckDescription NVARCHAR(MAX) NULL;
```

Registered in `SecurityDashboardMigrationPlan.DefinePlan()`:
```csharp
.To<AddExposureCheckDescriptionColumn>("SecurityDashboard-1.3.0")
```

---

## Entity Relationships (unchanged)

```
CheckResultRecord (1) ──< AdvisoryRecord (many, per package per scan)
                                │
                                └── ExposureCheckDescription (new nullable column)

ManualMitigationRecord (global, per GHSA ID — not scan-scoped)
```

---

## State Transitions

| Advisory Scenario | `AffectedStatus` in DB | `ExposureCheckDescription` in DB | `ManualMitigation` in API response | `ExposureCheckMitigationDescription` in API response |
|---|---|---|---|---|
| Vulnerable, no exposure check match | `Vulnerable` | `NULL` | `null` | `null` |
| Vulnerable, exposure check → Mitigated | `Mitigated` | `"Content Delivery API is disabled"` | `null` | `"Content Delivery API is disabled"` |
| Vulnerable, exposure check → Mitigated, also manually mitigated | `Mitigated` (set by exposure check at scan time; manual mitigation does not re-override since status is already Mitigated) | `"Content Delivery API is disabled"` | `null` (manual mitigation only overrides Vulnerable/Unknown) | `"Content Delivery API is disabled"` |
| Vulnerable, manually mitigated only | `Vulnerable` (scan) → `Mitigated` (runtime override) | `NULL` | `{ description, mitigatedAt, mitigatedBy }` | `null` |
| NotAffected | `NotAffected` | `NULL` | `null` | `null` |

> Note: The priority rule "manual mitigation takes precedence" applies when a Vulnerable advisory is manually mitigated. An advisory already stored as `Mitigated` by an exposure check is not overridden by the manual mitigation map at read time (the current `GetDashboardStatusAsync` only applies manual mitigations to `Vulnerable`/`Unknown` rows).
