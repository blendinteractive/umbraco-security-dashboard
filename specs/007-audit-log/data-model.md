# Data Model: Security Audit Log (007)

## New Entity: AuditLogRecord

**Table**: `SecurityDashboard_AuditLog`  
**Migration**: `AddAuditLogTable` → plan version `SecurityDashboard-1.2.0`

### Schema

| Column | SQL Type | Nullable | Notes |
|--------|----------|----------|-------|
| `Id` | `INT` | NOT NULL | PK, IDENTITY |
| `Timestamp` | `DATETIME` | NOT NULL | UTC; stored via `DateTime.UtcNow` |
| `OverallStatus` | `VARCHAR(20)` | NOT NULL | One of: `Safe`, `Mitigated`, `Vulnerable`, `NeverChecked` |
| `ActionType` | `VARCHAR(10)` | NOT NULL | One of: `Manual`, `Automatic` |
| `ActorName` | `VARCHAR(500)` | NULL | NULL for `Automatic` entries; display name at write time |
| `Description` | `VARCHAR(1000)` | NOT NULL | Human-readable trigger description |

**Index**: `IX_SecurityDashboard_AuditLog_Timestamp` on `(Timestamp DESC)` — supports reverse-chronological paging.

### C# POCO

```csharp
[TableName("SecurityDashboard_AuditLog")]
[PrimaryKey("Id")]
public class AuditLogRecord
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? ActorName { get; set; }
    public string Description { get; set; } = string.Empty;
}
```

### Validation Rules

- `OverallStatus` must be one of `Safe | Mitigated | Vulnerable | NeverChecked` (enforced at write time by call sites, not the DB constraint)
- `ActionType` must be one of `Manual | Automatic`
- `ActorName` must be non-null when `ActionType == "Manual"`; must be null when `ActionType == "Automatic"`
- `Description` must be non-empty (max 1000 chars)
- `Timestamp` must be UTC

---

## Repository Interface

```csharp
public interface IAuditLogRepository
{
    Task AppendAsync(AuditLogRecord record);
    Task<AuditLogPage> GetPagedAsync(int skip, int take);
}

public record AuditLogPage(IReadOnlyList<AuditLogRecord> Entries, int TotalCount);
```

---

## API DTOs

### AuditLogEntryDto

```csharp
public class AuditLogEntryDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? ActorName { get; set; }
    public string Description { get; set; } = string.Empty;
}
```

### AuditLogPageResponse

```csharp
public class AuditLogPageResponse
{
    public IReadOnlyList<AuditLogEntryDto> Entries { get; set; } = [];
    public int TotalCount { get; set; }
}
```

---

## State Transitions That Produce Audit Entries

### Automatic (vulnerability scan)

| Previous State | New State | Entry Written? | Webhook Fired? |
|---------------|-----------|----------------|----------------|
| NeverChecked | Any | Yes | Yes |
| Safe | Vulnerable / Mitigated | Yes | Yes |
| Vulnerable | Safe / Mitigated | Yes | Yes |
| Mitigated | Safe / Vulnerable | Yes | Yes |
| Any | Same state | **No** | **No** |

### Manual (mitigation marking)

| Action | Entry Written? | Webhook Fired if state changed? |
|--------|---------------|--------------------------------|
| Mark mitigation (any state change) | **Always** | Yes if overall state changed |
| Remove mitigation (any state change) | **Always** | Yes if overall state changed |

---

## Modified Interfaces

### IVulnerabilityService (addition)

```csharp
Task<string> GetCurrentOverallStatusAsync();
```

This extracts the existing inline status-computation from `GetDashboardStatusAsync()` into a reusable method. Returns one of `Safe | Mitigated | Vulnerable | NeverChecked`.

---

## Frontend Types (additions to `client/src/types.ts`)

```typescript
export interface AuditLogEntryDto {
  id: number;
  timestamp: string; // ISO 8601 UTC
  overallStatus: 'Safe' | 'Mitigated' | 'Vulnerable' | 'NeverChecked';
  actionType: 'Manual' | 'Automatic';
  actorName: string | null;
  description: string;
}

export interface AuditLogPageResponse {
  entries: AuditLogEntryDto[];
  totalCount: number;
}
```
