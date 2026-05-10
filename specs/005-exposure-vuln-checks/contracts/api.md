# API Contract: Exposure-Based Vulnerability Checks

## Endpoint (unchanged)

```
GET /umbraco/management/api/v1.0/security-dashboard/status
Authorization: BackOfficeAccess
```

## Breaking Changes

The `affectedStatus` field on `AdvisoryDto` and `AdvisoryPackageDto` changes its value set.

| Old values | New values |
|-----------|-----------|
| `Affected` | `Vulnerable` |
| `NotAffected` | `NotAffected` |
| `Unknown` | `Unknown` |
| *(none)* | `Mitigated` *(new)* |

All other fields and the response envelope are unchanged.

---

## `DashboardStatusResponse`

```json
{
  "overallStatus": "Safe | Vulnerable | NeverChecked",
  "isStale": false,
  "lastSuccessfulCheckAt": "2026-05-09T04:00:00Z",
  "lastCheckAttemptAt": "2026-05-09T04:00:00Z",
  "lastCheckSucceeded": true,
  "lastCheckError": null,
  "nextScheduledCheckAt": "2026-05-10T04:00:00Z",
  "affectedAdvisoryCount": 2,
  "advisories": [ /* AdvisoryDto[] — see below */ ]
}
```

`affectedAdvisoryCount` counts advisories whose consolidated `affectedStatus` is `Vulnerable` or `Unknown` (same logic as before, using new value names).

Advisory ordering (descending priority):
1. `Vulnerable`
2. `Unknown`
3. `Mitigated`
4. `NotAffected`

---

## `AdvisoryDto`

```json
{
  "ghsaId": "GHSA-xxxx-xxxx-xxxx",
  "title": "Advisory title",
  "severity": "Critical | High | Moderate | Low",
  "advisoryUrl": "https://github.com/advisories/GHSA-xxxx",
  "publishedAt": "2026-01-15T00:00:00Z",
  "affectedStatus": "Vulnerable | Mitigated | NotAffected | Unknown",
  "packages": [ /* AdvisoryPackageDto[] */ ]
}
```

`affectedStatus` on `AdvisoryDto` is the consolidated worst-case status across all packages:
- Any package `Vulnerable` → consolidated `Vulnerable`
- Any package `Unknown` (but none `Vulnerable`) → consolidated `Unknown`
- Any package `Mitigated` (none `Vulnerable` or `Unknown`) → consolidated `Mitigated`
- All packages `NotAffected` → consolidated `NotAffected`

---

## `AdvisoryPackageDto`

```json
{
  "packageName": "Umbraco.Cms",
  "affectedVersionRange": "[13.0.0, 13.3.0)",
  "installedVersion": "13.2.1",
  "affectedStatus": "Vulnerable | Mitigated | NotAffected | Unknown"
}
```

---

## Webhook Payload (updated status values)

The `affectedPackages[].affectedStatus` field in webhook payloads uses the same new value set (`Vulnerable`, `Mitigated`, `NotAffected`, `Unknown`).

Webhooks are triggered when overall status is `Vulnerable` (replacing the old trigger on `"Affected"` records).

---

## UI Display Contract

| `affectedStatus` value | Label shown | `uui-tag` colour |
|-----------------------|-------------|-----------------|
| `Vulnerable` | `Vulnerable` | `danger` (red) |
| `Mitigated` | `Mitigated` | `caution` (yellow) |
| `Unknown` | `Unknown` | `warning` (amber) |
| `NotAffected` | `Not Affected` | `positive` (green) |

---

## Extension API (C# only — no HTTP surface)

Third-party packages register exposure checks via:

```csharp
// In a Composer or Program.cs:
builder.AddExposureCheck<MyCustomExposureCheck>();
```

`AddExposureCheck<T>()` is defined on `IUmbracoBuilder` in `Umbraco.SecurityDashboard.Extensions`.

`T` must implement `IExposureCheck` (from `Umbraco.SecurityDashboard.Services.Exposure`).
