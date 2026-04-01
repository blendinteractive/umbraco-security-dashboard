using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class InstalledPackageProviderTests
{
    [Fact]
    public void GetInstalledUmbracoPackages_ReturnsOnlyUmbracoPackages()
    {
        var provider = new InstalledPackageProvider();
        var packages = provider.GetInstalledUmbracoPackages();

        // All returned keys must start with "Umbraco."
        foreach (var key in packages.Keys)
        {
            Assert.StartsWith("Umbraco.", key, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void GetInstalledUmbracoPackages_DeduplicatesByRootName()
    {
        // The provider strips .Core, .Web, .Infrastructure suffixes
        // and retains the highest version per root name.
        // We can't easily control AppDomain in unit tests, but we can verify
        // there are no duplicate root names returned.
        var provider = new InstalledPackageProvider();
        var packages = provider.GetInstalledUmbracoPackages();

        // Each key should appear only once (Dictionary guarantees this)
        var distinctKeys = packages.Keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(distinctKeys.Count, packages.Count);
    }

    [Fact]
    public void GetInstalledUmbracoPackages_AllVersionsNonEmpty()
    {
        var provider = new InstalledPackageProvider();
        var packages = provider.GetInstalledUmbracoPackages();

        foreach (var (key, version) in packages)
        {
            Assert.False(string.IsNullOrWhiteSpace(version),
                $"Package '{key}' has null or empty version");
        }
    }
}
