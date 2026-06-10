# Contract: Advisory API — 009-exposure-mitigation-desc

**Endpoint**: `GET /umbraco/management/api/v1/security-dashboard/status`  
**Change type**: Additive (new nullable field on `AdvisoryDto`)

---

## Updated `AdvisoryDto` shape

```json
{
  "ghsaId": "GHSA-xxxx-xxxx-xxxx",
  "title": "Some Umbraco vulnerability",
  "severity": "High",
  "advisoryUrl": "https://github.com/advisories/GHSA-xxxx-xxxx-xxxx",
  "publishedAt": "2025-01-15T00:00:00Z",
  "affectedStatus": "Mitigated",
  "packages": [
    {
      "packageName": "Umbraco.Cms",
      "affectedVersionRange": "[10.0.0, 10.8.0)",
      "installedVersion": "10.7.0",
      "affectedStatus": "Mitigated"
    }
  ],
  "manualMitigation": null,
  "exposureCheckMitigationDescription": "Content Delivery API is disabled"
}
```

### New field

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `exposureCheckMitigationDescription` | `string` | Yes | Plain-language reason(s) why an exposure check determined this advisory is mitigated. Multiple reasons are joined with `"; "`. Present only when `affectedStatus == "Mitigated"` and the status was set by an exposure check (not a manual mitigation). `null` in all other cases. |

---

## Field co-existence rules

| `affectedStatus` | `manualMitigation` | `exposureCheckMitigationDescription` | Meaning |
|---|---|---|---|
| `"Mitigated"` | `null` | `"Content Delivery API is disabled"` | Auto-mitigated by exposure check |
| `"Mitigated"` | `{ … }` | `null` | Manually mitigated (Vulnerable → overridden at read time) |
| `"Vulnerable"` | `null` | `null` | Not mitigated |
| `"NotAffected"` | `null` | `null` | Outside vulnerable version range |
| `"Unknown"` | `null` | `null` | Version range could not be parsed |

---

## Backwards compatibility

- `exposureCheckMitigationDescription` is a new nullable field. Existing clients that ignore unknown fields are unaffected.
- All other fields and their types are unchanged.
- No request-side changes. All existing write endpoints (`POST /mitigations`, `DELETE /mitigations`) are unchanged.

---

## Example: multiple exposure checks both mitigated

```json
{
  "affectedStatus": "Mitigated",
  "manualMitigation": null,
  "exposureCheckMitigationDescription": "Content Delivery API is disabled; All backoffice users are administrators"
}
```
