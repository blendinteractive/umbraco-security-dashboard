# Quickstart: Filter Irrelevant Version Advisories

**Branch**: `003-filter-irrelevant-advisories`

## What this feature changes

`VulnerabilityService.RunCheckAsync` now skips advisory entries whose entire affected
version range falls strictly below the installed package version. Only one method is
added and one loop is modified — everything else is unchanged.

## Files changed

| File | Change |
|------|--------|
| `src/Umbraco.SecurityDashboard/Services/VulnerabilityService.cs` | **Modified** — add `IsObsoleteForInstalledVersion` private method + call it in `RunCheckAsync` |
| `tests/.../Services/VulnerabilityServiceTests.cs` | **Modified** — add tests for the new relevance filtering behaviour |

## Test scenarios

### Scenario A — Old-version advisory excluded

1. Mock GitHub client to return one advisory: `Umbraco.Cms`, range `>= 16.0, < 17.0`
2. Mock installed packages: `Umbraco.Cms = 17.1.0`
3. Run `RunCheckAsync`
4. Assert: `SaveAdvisoriesAsync` is called with **zero** advisory records (the advisory
   is excluded)

### Scenario B — Spanning advisory included

1. Mock GitHub client: one advisory, `Umbraco.Cms`, range `>= 16.0, < 17.5`
2. Mock installed packages: `Umbraco.Cms = 17.1.0`
3. Run `RunCheckAsync`
4. Assert: `SaveAdvisoriesAsync` is called with **one** advisory record with
   `AffectedStatus = "Affected"` (17.1.0 is within the range)

### Scenario C — Future advisory retained

1. Mock GitHub client: one advisory, `Umbraco.Cms`, range `>= 17.5`
2. Mock installed packages: `Umbraco.Cms = 17.1.0`
3. Run `RunCheckAsync`
4. Assert: `SaveAdvisoriesAsync` is called with **one** advisory record with
   `AffectedStatus = "NotAffected"` (17.1.0 is below the range, but range is open-ended
   and relevant to the v17 line)

### Scenario D — Unknown advisory never excluded

1. Mock GitHub client: one advisory, `Umbraco.Cms`, unparseable range (e.g., `"all"`)
2. Mock installed packages: `Umbraco.Cms = 17.1.0`
3. Run `RunCheckAsync`
4. Assert: `SaveAdvisoriesAsync` is called with **one** advisory record with
   `AffectedStatus = "Unknown"`

## Running tests

```bash
dotnet test tests/Umbraco.SecurityDashboard.Tests/
```
