# API Contracts: Manual Vulnerability Mitigation Marking

Base URL: `/umbraco/management/api/v1/security-dashboard`  
Auth: All endpoints require `Authorization: Bearer <token>` with `BackOfficeAccess` policy.

---

## Existing Endpoint (unchanged contract, extended response)

### GET `/status`

Response body now includes `manualMitigation` on each advisory where a manual record exists:

```json
{
  "overallStatus": "Vulnerable",
  "advisories": [
    {
      "ghsaId": "GHSA-1234-5678-abcd",
      "title": "Example advisory",
      "severity": "High",
      "advisoryUrl": "https://github.com/advisories/GHSA-1234-5678-abcd",
      "publishedAt": "2026-01-01T00:00:00Z",
      "affectedStatus": "Mitigated",
      "packages": [...],
      "manualMitigation": {
        "description": "Applied WAF rule to block exploitation path.",
        "mitigatedAt": "2026-05-11T10:30:00Z",
        "mitigatedBy": "Jane Admin"
      }
    },
    {
      "ghsaId": "GHSA-9999-0000-zzzz",
      "affectedStatus": "Vulnerable",
      "manualMitigation": null,
      ...
    }
  ]
}
```

`manualMitigation` is `null` for all non-manually-mitigated advisories.

---

## New Endpoint: Create Manual Mitigation

### POST `/advisories/{ghsaId}/mitigations`

Creates a manual mitigation record for the specified advisory.

**Path parameter**: `ghsaId` — the GHSA ID of the advisory (e.g., `GHSA-1234-5678-abcd`)

**Request body**:
```json
{
  "description": "Applied WAF rule to block exploitation path."
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `description` | string | Yes | 1–2000 characters; whitespace-only rejected |

**Responses**:

`201 Created`
```json
{
  "ghsaId": "GHSA-1234-5678-abcd",
  "description": "Applied WAF rule to block exploitation path.",
  "mitigatedAt": "2026-05-11T10:30:00Z",
  "mitigatedBy": "Jane Admin"
}
```

`400 Bad Request` — description missing or empty
```json
{
  "title": "One or more validation errors occurred.",
  "errors": {
    "description": ["The Description field is required."]
  }
}
```

`409 Conflict` — advisory is already manually mitigated
```json
{
  "title": "Advisory GHSA-1234-5678-abcd is already manually mitigated."
}
```

`401 Unauthorized` — missing or invalid auth token

---

## New Endpoint: Remove Manual Mitigation

### DELETE `/advisories/{ghsaId}/mitigations`

Removes the manual mitigation record for the specified advisory. The advisory reverts to its automatically calculated status.

**Path parameter**: `ghsaId` — the GHSA ID of the advisory

**Request body**: none

**Responses**:

`204 No Content` — mitigation successfully removed

`404 Not Found` — no active manual mitigation exists for this advisory
```json
{
  "title": "No manual mitigation found for advisory GHSA-1234-5678-abcd."
}
```

`401 Unauthorized` — missing or invalid auth token

---

## Controller Route Summary

```
GET    /umbraco/management/api/v1/security-dashboard/status                          → existing
POST   /umbraco/management/api/v1/security-dashboard/advisories/{ghsaId}/mitigations → new
DELETE /umbraco/management/api/v1/security-dashboard/advisories/{ghsaId}/mitigations → new
```

---

## Frontend API Call Shapes

```typescript
// Create mitigation
async function createMitigation(ghsaId: string, description: string, token: string): Promise<void> {
  const response = await fetch(
    `/umbraco/management/api/v1/security-dashboard/advisories/${encodeURIComponent(ghsaId)}/mitigations`,
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ description }),
    }
  );
  if (!response.ok) throw new Error(`Failed to create mitigation: ${response.status}`);
}

// Remove mitigation
async function removeMitigation(ghsaId: string, token: string): Promise<void> {
  const response = await fetch(
    `/umbraco/management/api/v1/security-dashboard/advisories/${encodeURIComponent(ghsaId)}/mitigations`,
    {
      method: 'DELETE',
      headers: { 'Authorization': `Bearer ${token}` },
    }
  );
  if (!response.ok) throw new Error(`Failed to remove mitigation: ${response.status}`);
}
```
