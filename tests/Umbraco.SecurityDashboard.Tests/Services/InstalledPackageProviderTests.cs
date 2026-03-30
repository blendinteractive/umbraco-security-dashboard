using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

/// <summary>
/// Tests for InstalledPackageProvider. Because the provider uses AppDomain.CurrentDomain.GetAssemblies(),
/// we test the root-name deduplication and filtering logic by exercising it against assemblies
/// actually loaded in the test process (which includes Umbraco.SecurityDashboard itself).
/// </summary>
public class InstalledPackageProviderTests
{
    private readonly InstalledPackageProvider _provider = new();

    [Fact]
    public void GetInstalledUmbracoPackages_ReturnsOnlyUmbracoAssemblies()
    {
        var packages = _provider.GetInstalledUmbracoPackages();

        // All returned keys must start with "Umbraco." (case-insensitive)
        Assert.All(packages.Keys, key =>
            Assert.True(key.StartsWith("Umbraco.", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void GetInstalledUmbracoPackages_DoesNotReturnNonUmbracoAssemblies()
    {
        var packages = _provider.GetInstalledUmbracoPackages();

        Assert.DoesNotContain(packages.Keys, k =>
            k.StartsWith("xunit", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("NSubstitute", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("System.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetInstalledUmbracoPackages_ReturnsNonNullVersionStrings()
    {
        var packages = _provider.GetInstalledUmbracoPackages();

        Assert.All(packages.Values, v => Assert.False(string.IsNullOrWhiteSpace(v)));
    }

    [Fact]
    public void GetInstalledUmbracoPackages_DeduplicatesBySuffixStripping()
    {
        var packages = _provider.GetInstalledUmbracoPackages();

        // If both "Umbraco.SecurityDashboard.Core" and "Umbraco.SecurityDashboard" were present,
        // only "Umbraco.SecurityDashboard" root should appear (no duplicate roots)
        var roots = packages.Keys.ToList();
        Assert.Equal(roots.Count, roots.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void GetInstalledUmbracoPackages_IncludesSecurityDashboardAssembly()
    {
        var packages = _provider.GetInstalledUmbracoPackages();

        // The main project assembly starts with Umbraco.SecurityDashboard
        Assert.Contains(packages.Keys, k =>
            k.StartsWith("Umbraco.SecurityDashboard", StringComparison.OrdinalIgnoreCase));
    }
}
