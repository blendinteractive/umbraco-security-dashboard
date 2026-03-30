using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Umbraco.SecurityDashboard.Controllers;
using Umbraco.SecurityDashboard.Models.Api;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Controllers;

public class SecurityDashboardControllerTests
{
    private readonly IVulnerabilityService _service = Substitute.For<IVulnerabilityService>();

    private SecurityDashboardController CreateController()
    {
        var controller = new SecurityDashboardController(_service);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };
        return controller;
    }

    [Fact]
    public async Task GetStatus_ReturnsOkWithDashboardResponse()
    {
        var expected = new DashboardStatusResponse
        {
            OverallStatus = "Safe",
            IsStale = false,
            NextScheduledCheckAt = DateTime.UtcNow.AddHours(20),
            Advisories = [],
        };
        _service.GetDashboardStatusAsync().Returns(expected);

        var controller = CreateController();
        var result = await controller.GetStatus();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        var response = Assert.IsType<DashboardStatusResponse>(ok.Value);
        Assert.Equal("Safe", response.OverallStatus);
    }

    [Fact]
    public async Task GetStatus_WhenServiceThrows_ReturnsInternalServerError()
    {
        _service.GetDashboardStatusAsync().ThrowsAsync(new InvalidOperationException("DB error"));

        var controller = CreateController();

        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetStatus());
    }
}
