using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Umbraco.Cms.Core.Security;
using Umbraco.SecurityDashboard.Controllers;
using Umbraco.SecurityDashboard.Models.Api;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Controllers;

public class SecurityDashboardControllerTests
{
    private static SecurityDashboardController CreateController(IVulnerabilityService? service = null)
    {
        return new SecurityDashboardController(
            service ?? Substitute.For<IVulnerabilityService>(),
            Substitute.For<IMitigationRepository>(),
            Substitute.For<IBackOfficeSecurityAccessor>());
    }

    [Fact]
    public async Task GetStatus_ReturnsOkWithDashboardResponse()
    {
        var service = Substitute.For<IVulnerabilityService>();
        var expected = new DashboardStatusResponse
        {
            OverallStatus = "Safe",
            IsStale = false,
            NextScheduledCheckAt = DateTime.UtcNow.AddDays(1)
        };
        service.GetDashboardStatusAsync().Returns(expected);

        var controller = CreateController(service);
        var result = await controller.GetStatus();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, ok.Value);
    }

    [Fact]
    public async Task GetStatus_WhenServiceThrows_PropagatesException()
    {
        var service = Substitute.For<IVulnerabilityService>();
        service.GetDashboardStatusAsync().Throws(new InvalidOperationException("DB error"));

        var controller = CreateController(service);

        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetStatus());
    }
}
