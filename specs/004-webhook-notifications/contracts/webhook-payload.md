# Contract: Outbound Webhook Payload

**Feature**: `004-webhook-notifications` | **Date**: 2026-05-05  
**Direction**: Outbound — server → administrator-configured endpoint  
**Trigger**: Each completed vulnerability scan (success path only)

---

## Request

```
POST <configured EndpointUrl>
Content-Type: application/json
X-Webhook-Secret: <value>   (omitted when no secret is configured)
```

### Body Schema

```json
{
  "siteUrl": "https://example.com",
  "status": "Vulnerable",
  "checkedAt": "2026-05-05T04:00:00Z",
  "affectedPackages": [
    {
      "packageName": "Umbraco.Cms",
      "installedVersion": "17.1.0",
      "advisoryUrl": "https://github.com/advisories/GHSA-xxxx-xxxx-xxxx",
      "severity": "High"
    }
  ]
}
```

### Field Definitions

| Field | Type | Always present | Description |
|---|---|---|---|
| `siteUrl` | string | Yes | The site's public URL from `WebhookSettings.SiteUrl`. |
| `status` | string | Yes | `"Vulnerable"` — one or more affected/unknown advisories found. `"Safe"` — no affected advisories. |
| `checkedAt` | string (ISO 8601 UTC) | Yes | The UTC timestamp of the completed scan (e.g., `"2026-05-05T04:00:00Z"`). |
| `affectedPackages` | array | Yes | Empty array `[]` when status is `"Safe"`. |
| `affectedPackages[].packageName` | string | Yes | NuGet package identifier. |
| `affectedPackages[].installedVersion` | string \| null | Yes | Detected installed version; `null` when not determinable. |
| `affectedPackages[].advisoryUrl` | string | Yes | URL to the GitHub Security Advisory page. |
| `affectedPackages[].severity` | string | Yes | One of: `"Critical"`, `"High"`, `"Moderate"`, `"Low"`. |

### Status Values

| Value | Meaning |
|---|---|
| `"Vulnerable"` | At least one advisory with AffectedStatus `Affected` or `Unknown` |
| `"Safe"` | No affected advisories found |

---

## Response

The server does not act on the response body. Only the HTTP status code is inspected:

| Status code range | Treatment |
|---|---|
| `2xx` | Success — delivery logged at Information level |
| `3xx` | Failure — redirects are not followed; logged at Error level |
| `4xx` / `5xx` | Failure — logged at Error level with status code |
| Timeout / network error | Failure — logged at Error level with exception message |

No retry is attempted after a failed delivery.

---

## Configuration (appsettings.json)

```json
{
  "Umbraco": {
    "SecurityDashboard": {
      "Webhook": {
        "SiteUrl": "https://example.com",
        "EndpointUrl": "https://hooks.example.com/security",
        "Secret": "optional-shared-secret",
        "TimeoutSeconds": 10
      }
    }
  }
}
```

`Secret` and `TimeoutSeconds` are optional. When `Secret` is omitted, no `X-Webhook-Secret` header is sent. When `TimeoutSeconds` is omitted, the default is 10.

Webhook dispatch is disabled (silently) when `EndpointUrl` or `SiteUrl` is absent or blank.
