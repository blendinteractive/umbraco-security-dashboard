# Data Model: Manual Vulnerability Mitigation Marking

## New Database Table: `SecurityDashboard_ManualMitigation`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `Id` | INT | PK, IDENTITY, NOT NULL | Auto-increment surrogate key |
| `GhsaId` | VARCHAR(50) | NOT NULL, UNIQUE | GitHub Advisory ID; links to advisory by identity, not FK |
| `Description` | NVARCHAR(2000) | NOT NULL | Administrator's explanation of the mitigation applied |
| `MitigatedAt` | DATETIME | NOT NULL | UTC timestamp when the mitigation was recorded |
| `MitigatedBy` | NVARCHAR(500) | NOT NULL | Display name of the administrator at the time of action |

**Index**: `UNIQUE IX_SecurityDashboard_ManualMitigation_GhsaId` on `GhsaId` — enforces one active mitigation per advisory.

**Lifecycle**: Rows are created when an administrator marks an advisory as mitigated. They are hard-deleted when the mitigation is removed. They survive vulnerability rescans (advisory records are replaced; mitigation records are not).

---

## New C# Model: `ManualMitigationRecord`

```csharp
// src/Umbraco.SecurityDashboard/Models/Db/ManualMitigationRecord.cs
[TableName("SecurityDashboard_ManualMitigation")]
[PrimaryKey("Id", AutoIncrement = true)]
public class ManualMitigationRecord
{
    public int Id { get; set; }
    public string GhsaId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime MitigatedAt { get; set; }
    public string MitigatedBy { get; set; } = string.Empty;
}
```

---

## New C# Models: API DTOs

```csharp
// src/Umbraco.SecurityDashboard/Models/Api/ManualMitigationDto.cs
public class ManualMitigationDto
{
    public string Description { get; set; } = string.Empty;
    public DateTime MitigatedAt { get; set; }
    public string MitigatedBy { get; set; } = string.Empty;
}

// src/Umbraco.SecurityDashboard/Models/Api/CreateMitigationRequest.cs
public class CreateMitigationRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;
}
```

---

## Modified C# Model: `AdvisoryDto`

Add one nullable field:

```csharp
// src/Umbraco.SecurityDashboard/Models/Api/AdvisoryDto.cs
public class AdvisoryDto
{
    // ... existing fields ...

    /// <summary>Populated when AffectedStatus is "Mitigated" due to a manual mitigation record.</summary>
    public ManualMitigationDto? ManualMitigation { get; set; }
}
```

---

## Modified TypeScript Types: `types.ts`

```typescript
// client/src/types.ts — additions

export interface ManualMitigationDto {
  description: string;
  mitigatedAt: string; // ISO 8601 UTC
  mitigatedBy: string;
}

export interface AdvisoryDto {
  // ... existing fields ...
  manualMitigation: ManualMitigationDto | null;
}
```

---

## Entity Relationships

```
SecurityDashboard_CheckResult (1) ──── (N) SecurityDashboard_Advisory
                                            GhsaId ────────────────── SecurityDashboard_ManualMitigation (0..1)
```

`ManualMitigation` is related to `Advisory` by `GhsaId` (a logical, not physical, foreign key). No FK constraint in the DB — advisory rows are ephemeral; mitigation rows are durable.

---

## State Transitions: Advisory `AffectedStatus`

```
Calculated status (from scan)     Manual mitigation present?     Displayed status
─────────────────────────────     ──────────────────────────     ────────────────
Vulnerable                        Yes                            Mitigated
Unknown                           Yes                            Mitigated
Mitigated (from scan)             Yes (or no)                    Mitigated
NotAffected                       (button not shown)             NotAffected
Vulnerable                        No                             Vulnerable
Unknown                           No                             Unknown
```

The `ManualMitigation` overlay is applied in `VulnerabilityService.GetDashboardStatusAsync()` after loading both the scan advisory records and the mitigation table rows. It does not modify advisory records in the DB.

---

## Validation Rules

| Field | Rule |
|-------|------|
| `Description` (create request) | Required; 1–2000 characters; whitespace-only is rejected |
| `GhsaId` (path param) | Must match an advisory in the current scan result; must not already have an active mitigation |
| Identity (`MitigatedBy`) | Server-assigned from `IBackOfficeSecurityAccessor`; not user-supplied |
| `MitigatedAt` | Server-assigned UTC `DateTime.UtcNow`; not user-supplied |
