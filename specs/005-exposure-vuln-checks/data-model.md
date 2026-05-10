# Data Model: Exposure-Based Vulnerability Checks

## Existing Entities (unchanged schema)

### `SecurityDashboard_Advisory` table (unchanged)

`AffectedStatus` VARCHAR(20) accommodates all four new values without a schema change.

| Column | Type | Notes |
|--------|------|-------|
| Id | INT PK | |
| CheckResultId | INT FK | |
| GhsaId | VARCHAR(50) | |
| Title | VARCHAR(500) | |
| Severity | VARCHAR(20) | Critical \| High \| Moderate \| Low |
| PackageName | VARCHAR(200) | |
| AffectedVersionRange | VARCHAR(100) | |
| AdvisoryUrl | VARCHAR(500) | |
| PublishedAt | DATETIME | |
| InstalledVersion | VARCHAR(50) NULL | |
| AffectedStatus | VARCHAR(20) | **Vulnerable \| Mitigated \| NotAffected \| Unknown** |

> Status values change from `Affected \| NotAffected \| Unknown` to `Vulnerable \| Mitigated \| NotAffected \| Unknown`. No ALTER TABLE needed — column width already sufficient.

---

## New Domain Entities (in-memory only — no new tables)

### `ExposureVerdict` enum

```csharp
public enum ExposureVerdict
{
    NotAffected = 0,
    Mitigated   = 1,
    Vulnerable  = 2
}
```

**Rules**:
- Numeric ordering enables trivial worst-case selection: `checks.Max(v => verdict)`
- Maps to `AffectedStatus` strings: `NotAffected` → `"NotAffected"`, `Mitigated` → `"Mitigated"`, `Vulnerable` → `"Vulnerable"`

---

### `IExposureCheck` interface

```csharp
public interface IExposureCheck
{
    /// Keyword matched against advisory Exposure section (e.g. "Content Delivery API")
    string Keyword { get; }

    Task<ExposureVerdict> CheckAsync(CancellationToken cancellationToken = default);
}
```

**Rules**:
- One `IExposureCheck` per keyword; multiple registrations for the same keyword are allowed
- Exceptions in `CheckAsync` must be caught by the evaluator, logged, and treated as `Vulnerable`

---

### `IExposureCheckEvaluator` interface

```csharp
public interface IExposureCheckEvaluator
{
    /// Runs all checks whose keyword appears in <paramref name="keywords"/>.
    /// Returns "Vulnerable" if no matching checks exist or any check returns Vulnerable.
    /// Returns "Mitigated" if all matching checks return Mitigated or NotAffected.
    /// Returns "NotAffected" only if all matching checks return NotAffected.
    Task<string> EvaluateAsync(IEnumerable<string> keywords, CancellationToken cancellationToken = default);
}
```

**Rules**:
- If `keywords` is empty or no registered check matches any keyword → return `"Vulnerable"` (fail-safe per FR-007/FR-008)
- Worst-case verdict wins: `Vulnerable` > `Mitigated` > `NotAffected`

---

## Modified C# Models

### `GitHubAdvisory.cs` — add `Description` property

```csharp
[JsonPropertyName("description")]
public string? Description { get; set; }
```

---

### `AdvisoryDto.cs` — updated doc comment

```csharp
/// <summary>Consolidated status: Vulnerable | Mitigated | NotAffected | Unknown</summary>
public string AffectedStatus { get; set; } = string.Empty;
```

---

## Modified TypeScript Types

### `client/src/types.ts`

```typescript
export interface AdvisoryPackageDto {
  packageName: string;
  affectedVersionRange: string;
  installedVersion: string | null;
  affectedStatus: 'Vulnerable' | 'Mitigated' | 'NotAffected' | 'Unknown';
}

export interface AdvisoryDto {
  ghsaId: string;
  title: string;
  severity: 'Critical' | 'High' | 'Moderate' | 'Low';
  advisoryUrl: string;
  publishedAt: string;
  affectedStatus: 'Vulnerable' | 'Mitigated' | 'NotAffected' | 'Unknown';
  packages: AdvisoryPackageDto[];
}
```

---

## State Transitions

```
Advisory version-matched
        │
        ▼
  advisory.Description parsed for ### Exposure keywords
        │
   ┌────┴────────────────────┐
   │ No keywords found       │ Keywords found
   │ (or no ### Exposure)    ▼
   │                  Matching checks run
   │                         │
   │              ┌──────────┴──────────────┐
   │              │ No matching checks      │ Checks run
   │              │ registered              ▼
   │              │                 Worst-case verdict
   └──────────────┴──────────────┐
                                 ▼
                        AffectedStatus stored:
                        Vulnerable | Mitigated | NotAffected
```

---

## Validation Rules

| Rule | Detail |
|------|--------|
| Keyword format | `* *[Keyword]*` bullet in `### Exposure` section; parsing is case-sensitive to match advisory format |
| Fail-safe | Any parse or check exception → treat as `Vulnerable` |
| Worst-case | Multiple checks → `Max(ExposureVerdict)` |
| No checks registered | Treat as `Vulnerable` |
| Empty/null description | Treat as `Vulnerable` |
