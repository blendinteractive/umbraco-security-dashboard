# Quickstart: Exposure-Based Vulnerability Checks

## What This Feature Does

Replaces the binary `Affected/NotAffected` advisory status with a three-level scale:

| Status | Colour | Meaning |
|--------|--------|---------|
| `Vulnerable` | Red | Package version matches the advisory AND the exposure condition is present |
| `Mitigated` | Yellow | Package version matches BUT the exposure condition is not present (risk controlled) |
| `NotAffected` | Green | Installed version is outside the vulnerable range |
| `Unknown` | Grey | Version parsing failed; cannot determine status |

## How It Works

1. **Advisory evaluation** (existing `RunCheckAsync`): for each advisory, the GitHub GHSA API `description` field is parsed for a `### Exposure` section containing `* *[Keyword]*` bullets.
2. **Check lookup**: registered `IExposureCheck` implementations keyed by keyword are invoked.
3. **Verdict**: worst-case result (`Vulnerable > Mitigated > NotAffected`) is stored as `AffectedStatus`.
4. **Default**: advisories with no `### Exposure` section, no matching checks, or any check error → `Vulnerable`.

## Adding a Custom Exposure Check

Implement `IExposureCheck` and register it in a Composer:

```csharp
using Umbraco.SecurityDashboard.Services.Exposure;

public class PublicRegistrationExposureCheck : IExposureCheck
{
    private readonly IOptions<MyFeatureSettings> _settings;

    public string Keyword => "Public Registration";

    public PublicRegistrationExposureCheck(IOptions<MyFeatureSettings> settings)
        => _settings = settings;

    public Task<ExposureVerdict> CheckAsync(CancellationToken cancellationToken = default)
    {
        var verdict = _settings.Value.AllowPublicRegistration
            ? ExposureVerdict.Vulnerable
            : ExposureVerdict.Mitigated;
        return Task.FromResult(verdict);
    }
}
```

```csharp
// In your IComposer:
public class MyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddExposureCheck<PublicRegistrationExposureCheck>();
    }
}
```

## Key Files

| File | Purpose |
|------|---------|
| `Services/Exposure/IExposureCheck.cs` | Check interface and `ExposureVerdict` enum |
| `Services/Exposure/ExposureKeywordParser.cs` | Parses `### Exposure` from advisory markdown |
| `Services/Exposure/ExposureCheckEvaluator.cs` | Runs checks, returns worst-case verdict |
| `Services/Exposure/Checks/ContentDeliveryApiExposureCheck.cs` | Built-in CDA check |
| `Services/Exposure/Checks/NonAdminUsersExposureCheck.cs` | Built-in non-admin users check |
| `Extensions/UmbracoBuilderExposureExtensions.cs` | `AddExposureCheck<T>()` helper |

## Keyword Reference (Built-In)

| Keyword | Returns Vulnerable When |
|---------|------------------------|
| `Non-Admin Backoffice Users` | Any backoffice user exists outside the `admin` group |
| `Content Delivery API` | `Umbraco:CMS:DeliveryApi:Enabled` is `true` |

## Testing the Feature

1. Seed a test advisory with a `### Exposure` section in its description.
2. Verify the check runs during `VulnerabilityService.RunCheckAsync()`.
3. Query the dashboard endpoint and assert the expected `affectedStatus`.

See acceptance scenarios in [`spec.md`](spec.md) for the full test matrix.
