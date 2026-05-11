# API Contract: Audit Log

**Base URL**: `/umbraco/management/api/v1/security-dashboard`  
**Authentication**: Bearer token (Umbraco back-office JWT); `[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]`

---

## GET `/audit-log`

Returns a reverse-chronological, paginated list of audit log entries.

### Query Parameters

| Parameter | Type | Default | Max | Description |
|-----------|------|---------|-----|-------------|
| `skip` | `int` | `0` | — | Number of entries to skip (for pagination offset) |
| `take` | `int` | `25` | `100` | Number of entries to return per page |

### Response: `200 OK`

```json
{
  "entries": [
    {
      "id": 42,
      "timestamp": "2026-05-11T14:32:00Z",
      "overallStatus": "Vulnerable",
      "actionType": "Automatic",
      "actorName": null,
      "description": "Scheduled vulnerability check completed"
    },
    {
      "id": 41,
      "timestamp": "2026-05-10T09:15:00Z",
      "overallStatus": "Mitigated",
      "actionType": "Manual",
      "actorName": "Jane Admin",
      "description": "Marked GHSA-1234-5678-9abc as mitigated"
    }
  ],
  "totalCount": 42
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `entries` | `AuditLogEntryDto[]` | Page of entries, newest first |
| `totalCount` | `int` | Total number of entries across all pages |
| `entries[].id` | `int` | Auto-increment primary key |
| `entries[].timestamp` | `string (ISO 8601 UTC)` | When the entry was written |
| `entries[].overallStatus` | `"Safe" \| "Mitigated" \| "Vulnerable" \| "NeverChecked"` | Site's overall vulnerability state at time of entry |
| `entries[].actionType` | `"Manual" \| "Automatic"` | Whether the change was user-driven or scan-driven |
| `entries[].actorName` | `string \| null` | Display name of the actor (Manual only; null for Automatic) |
| `entries[].description` | `string` | Human-readable trigger description |

### Response: `401 Unauthorized`

Returned when the request has no valid back-office bearer token.

### Notes

- The `take` parameter is silently clamped to 100 server-side.
- When `totalCount` is `0`, `entries` is an empty array (not a 404).
- This endpoint is read-only; there are no POST/PUT/DELETE operations on audit entries.

---

## Existing Endpoints — Audit Behaviour Changes

### `POST /advisories/{ghsaId}/mitigations`

Unchanged response contract. Side effects added by feature 007:
- Audit log entry written (always).
- Webhook fired if overall status changed.

### `DELETE /advisories/{ghsaId}/mitigations`

Unchanged response contract. Side effects added by feature 007:
- Audit log entry written (always).
- Webhook fired if overall status changed.
