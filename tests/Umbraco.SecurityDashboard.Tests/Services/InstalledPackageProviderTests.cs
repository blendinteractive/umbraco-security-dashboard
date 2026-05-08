using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Umbraco.SecurityDashboard.Configuration;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class InstalledPackageProviderTests
{
    private static InstalledPackageProvider CreateProvider(
        SecurityDashboardSettings? settings = null,
        bool isDevelopment = false)
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(isDevelopment ? Environments.Development : Environments.Production);
        var options = Options.Create(settings ?? new SecurityDashboardSettings());
        return new InstalledPackageProvider(env, options);
    }

    [Fact]
    public void GetInstalledUmbracoPackages_ReturnsOnlyUmbracoPackagesWhenNoAdditional()
    {
        var provider = CreateProvider();
        var packages = provider.GetInstalledUmbracoPackages();

        foreach (var key in packages.Keys)
        {
            Assert.StartsWith("Umbraco.", key, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void GetInstalledUmbracoPackages_DeduplicatesByRootName()
    {
        var provider = CreateProvider();
        var packages = provider.GetInstalledUmbracoPackages();

        var distinctKeys = packages.Keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(distinctKeys.Count, packages.Count);
    }

    [Fact]
    public void GetInstalledUmbracoPackages_AllVersionsNonEmpty()
    {
        var provider = CreateProvider();
        var packages = provider.GetInstalledUmbracoPackages();

        foreach (var (key, version) in packages)
        {
            Assert.False(string.IsNullOrWhiteSpace(version),
                $"Package '{key}' has null or empty version");
        }
    }

    [Fact]
    public void GetInstalledUmbracoPackages_IncludesAdditionalPackageIds()
    {
        var settings = new SecurityDashboardSettings
        {
            AdditionalPackageIds = ["NSubstitute"]
        };
        var provider = CreateProvider(settings);
        var packages = provider.GetInstalledUmbracoPackages();

        Assert.True(packages.ContainsKey("NSubstitute"),
            "Expected 'NSubstitute' to be included via AdditionalPackageIds");
        Assert.False(string.IsNullOrWhiteSpace(packages["NSubstitute"]),
            "Expected 'NSubstitute' to have a non-empty version");
    }

    [Fact]
    public void GetInstalledUmbracoPackages_EmptyAdditionalPackageIds_ReturnsOnlyUmbraco()
    {
        var provider = CreateProvider();
        var packages = provider.GetInstalledUmbracoPackages();

        foreach (var key in packages.Keys)
        {
            Assert.StartsWith("Umbraco.", key, StringComparison.OrdinalIgnoreCase);
        }
    }
}
