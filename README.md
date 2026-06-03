## What Is It

If you maintain a lot of Umbraco installs, managing your security health can be a big task. When a new advisory is released, you suddenly have a lot of work to do. 

The Umbraco Security Dashboard helps by providing regular automatic checks of the running code against the Github Vulnerability Database for Umbraco packages and allows you to report the security health status of the instance to a central location via webhook. 

The system also provides checks against common vulnerability exposure points to help you understand which advisories your instance is actually susceptible to. For instance, if the advisory reports an exposure in the Content Delivery API, but you have it disabled, then that exposure is mitigated. This helps you triage your efforts to focus on the instances where you're actually exposed.

## Installing

To add the package to your project, install it with NuGet:

```bash
dotnet add package BlendInteractive.SecurityDashboard
```

This will add a 'Security Health' tab in the 'Settings' section of your Backoffice.

## Configuration

All settings live under the `Umbraco:SecurityDashboard` key in `appsettings.json`.

```json
"Umbraco": {
  "SecurityDashboard": {
    "AdditionalPackageIds": [],
    "Webhook": {
      "SiteUrl": "https://yoursite.com",
      "EndpointUrl": "https://your-webhook-endpoint.com/hook",
      "Secret": "your-secret",
      "TimeoutSeconds": 10
    }
  }
}
```

### AdditionalPackageIds

A list of non-Umbraco NuGet package IDs to include in vulnerability checks. Versions are detected automatically from the runtime dependency graph.

```json
"AdditionalPackageIds": [ "Serilog", "Newtonsoft.Json" ]
```

### Webhook

When configured, the dashboard posts the scan result to an external endpoint after every check, making it easy to aggregate security status across multiple Umbraco instances.

| Property | Description |
|---|---|
| `SiteUrl` | The public URL of this Umbraco instance. Included in the webhook payload to identify the source. This is simply an identifying string since the web context isn't available in the scheduled check. |
| `EndpointUrl` | The URL to POST the result to. Leave empty to disable webhook notifications. |
| `Secret` | Shared secret included in the request header for payload verification. |
| `TimeoutSeconds` | HTTP timeout for the webhook request. Defaults to `10`. |

### ScanSchedule

Controls when vulnerability scans run. By default, scans run daily at 4:00 AM (server local time).

| Property | Description | Default |
|---|---|---|
| `Frequency` | `Daily`, `Weekly`, or `Disabled` | `Daily` |
| `Hour` | Hour of day (0–23) for the scan to run | `4` |
| `Minute` | Minute (0–59) for the scan to run | `0` |
| `DayOfWeek` | Day for weekly scans (`Monday`–`Sunday`) | `Monday` |

**Daily at 2:30 AM:**

```json
"ScanSchedule": {
  "Frequency": "Daily",
  "Hour": 2,
  "Minute": 30
}
```

**Weekly on Monday at 3:00 AM:**

```json
"ScanSchedule": {
  "Frequency": "Weekly",
  "DayOfWeek": "Monday",
  "Hour": 3,
  "Minute": 0
}
```

**Disabled (no automatic scans):**

```json
"ScanSchedule": {
  "Frequency": "Disabled"
}
```

When `Disabled`, the background job is not registered and the startup check is skipped entirely. The dashboard displays a prominent warning. This is useful for development or staging environments where you do not want background scans running.

> **Note**: Changes to `ScanSchedule` take effect only after an application restart.

### Development overrides

The `Development` subsection contains settings that are only applied when the application is running in the `Development` environment. They are silently ignored in all other environments.

```json
"Umbraco": {
  "SecurityDashboard": {
    "Development": {
      "PackageVersionOverrides": {
        "Umbraco.Cms": "17.1.1"
      },
      "ExposureCheckOverrides": {
        "GHSA-xxxx-yyyy-zzzz": [ "Content Delivery API" ]
      }
    }
  }
}
```

**`PackageVersionOverrides`** — substitute the detected version of any NuGet package with a fixed value. Useful for testing how the dashboard behaves against a specific advisory without actually downgrading the package.

**`ExposureCheckOverrides`** — force one or more exposure checks to run for a specific advisory, regardless of whether the corresponding keyword appears in the advisory description. The key is the GHSA ID (e.g. `GHSA-xxxx-yyyy-zzzz`) and the value is a list of exposure check keywords (e.g. `"Content Delivery API"`, `"Non-Admin Backoffice Users"`). Useful for manually exercising exposure checks against a chosen advisory during development.

## Contributing

### Extending ExposureChecks

Exposure checks determine whether your instance is actually susceptible to a given advisory. The package ships with two built-in checks, but you can add your own.

**1. Implement `IExposureCheck`:**

```csharp
using Umbraco.SecurityDashboard.Services.Exposure;

public class PublicRegistrationExposureCheck : IExposureCheck
{
    // Must match a keyword that appears in advisory descriptions.
    // The evaluator only runs this check when the keyword is found.
    public string Keyword => "Member Registration";

    public Task<ExposureVerdict> CheckAsync(CancellationToken cancellationToken = default)
    {
        // Return Vulnerable, Mitigated, or NotAffected
        bool registrationOpen = /* your logic here */ true;
        var verdict = registrationOpen ? ExposureVerdict.Vulnerable : ExposureVerdict.Mitigated;
        return Task.FromResult(verdict);
    }
}
```

**2. Register it in your composer or `Program.cs`:**

```csharp
builder.AddExposureCheck<PublicRegistrationExposureCheck>();
```

The check runs only when the `Keyword` appears in the advisory text, so it doesn't add overhead for unrelated advisories. If the check throws, the evaluator logs the exception and treats the result as `Vulnerable`.

**Built-in checks:**

| Keyword | What it checks |
|---|---|
| `Content Delivery API` | Whether the Umbraco Delivery API is enabled |
| `Non-Admin Backoffice Users` | Whether any non-admin backoffice users exist |

### Spec-Kit

This project uses [Spec-Kit](https://github.com/your-org/spec-kit) for structured feature development. Feature specifications live in `specs/` and drive the implementation workflow.

```bash
# Review the current feature plan
cat specs/007-audit-log/plan.md

# Run the implementation agent against the active plan
/speckit-implement
```

Each spec directory contains a `plan.md` with the feature design and a `tasks.md` with the implementation checklist. Completed specs are kept for reference.
