namespace Umbraco.SecurityDashboard.Configuration;

public class DevelopmentSettings
{
    /// <summary>
    /// Override installed package versions for testing.
    /// Keys are NuGet package names; values are version strings that replace the detected version.
    /// </summary>
    public Dictionary<string, string> PackageVersionOverrides { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Force specific exposure check keywords for a given advisory, regardless of whether the keyword
    /// appears in the advisory description.
    /// Keys are GHSA IDs (e.g. "GHSA-xxxx-yyyy-zzzz"); values are lists of exposure check keywords.
    /// </summary>
    public Dictionary<string, List<string>> ExposureCheckOverrides { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}
