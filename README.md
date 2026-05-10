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
| `SiteUrl` | The public URL of this Umbraco instance. Included in the webhook payload to identify the source. |
| `EndpointUrl` | The URL to POST the result to. Leave empty to disable webhook notifications. |
| `Secret` | Shared secret included in the request header for payload verification. |
| `TimeoutSeconds` | HTTP timeout for the webhook request. Defaults to `10`. |

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



### Spec-Kit

