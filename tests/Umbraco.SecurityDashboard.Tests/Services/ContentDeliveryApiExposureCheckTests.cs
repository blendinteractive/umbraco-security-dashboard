using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.SecurityDashboard.Services.Exposure;
using Umbraco.SecurityDashboard.Services.Exposure.Checks;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class ContentDeliveryApiExposureCheckTests
{
    private static ContentDeliveryApiExposureCheck CreateSut(bool enabled) =>
        new(Options.Create(new DeliveryApiSettings { Enabled = enabled }));

    [Fact]
    public async Task CheckAsync_CdaEnabled_ReturnsVulnerable()
    {
        var sut = CreateSut(enabled: true);
        var result = await sut.CheckAsync();
        Assert.Equal(ExposureVerdict.Vulnerable, result.Verdict);
    }

    [Fact]
    public async Task CheckAsync_CdaDisabled_ReturnsMitigated()
    {
        var sut = CreateSut(enabled: false);
        var result = await sut.CheckAsync();
        Assert.Equal(ExposureVerdict.Mitigated, result.Verdict);
    }

    // --- T015: Description tests ---

    [Fact]
    public async Task CheckAsync_CdaDisabled_ReturnsMitigationDescription()
    {
        var sut = CreateSut(enabled: false);
        var result = await sut.CheckAsync();
        Assert.Equal("Content Delivery API is disabled", result.MitigationDescription);
    }

    [Fact]
    public async Task CheckAsync_CdaEnabled_MitigationDescriptionIsNull()
    {
        var sut = CreateSut(enabled: true);
        var result = await sut.CheckAsync();
        Assert.Null(result.MitigationDescription);
    }
}
