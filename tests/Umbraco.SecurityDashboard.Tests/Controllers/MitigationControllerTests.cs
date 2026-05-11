using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.SecurityDashboard.Controllers;
using Umbraco.SecurityDashboard.Models.Api;
using Umbraco.SecurityDashboard.Models.Db;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Controllers;

public class MitigationControllerTests
{
    private static (SecurityDashboardController Controller, IMitigationRepository Repo) CreateSut(
        string currentUserName = "Test Admin")
    {
        var vulnerabilityService = Substitute.For<IVulnerabilityService>();
        var mitigationRepo = Substitute.For<IMitigationRepository>();

        var backOfficeAccessor = Substitute.For<IBackOfficeSecurityAccessor>();
        var backOfficeSecurity = Substitute.For<IBackOfficeSecurity>();
        var user = Substitute.For<IUser>();
        user.Name.Returns(currentUserName);
        backOfficeSecurity.CurrentUser.Returns(user);
        backOfficeAccessor.BackOfficeSecurity.Returns(backOfficeSecurity);

        var controller = new SecurityDashboardController(vulnerabilityService, mitigationRepo, backOfficeAccessor, Substitute.For<IAuditLogRepository>());
        return (controller, mitigationRepo);
    }

    [Fact]
    public async Task CreateMitigation_WithValidDescription_Returns201WithDto()
    {
        var (controller, repo) = CreateSut("Jane Admin");

        var result = await controller.CreateMitigation("GHSA-1234-5678-abcd",
            new CreateMitigationRequest { Description = "Applied WAF rule." });

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
        var dto = Assert.IsType<ManualMitigationDto>(objectResult.Value);
        Assert.Equal("Applied WAF rule.", dto.Description);
        Assert.Equal("Jane Admin", dto.MitigatedBy);
        await repo.Received(1).CreateMitigationAsync(Arg.Is<ManualMitigationRecord>(r =>
            r.GhsaId == "GHSA-1234-5678-abcd" &&
            r.Description == "Applied WAF rule." &&
            r.MitigatedBy == "Jane Admin"));
    }

    [Fact]
    public async Task CreateMitigation_WithEmptyDescription_Returns400()
    {
        var (controller, _) = CreateSut();
        controller.ModelState.AddModelError("Description", "The Description field is required.");

        var result = await controller.CreateMitigation("GHSA-test", new CreateMitigationRequest { Description = "" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateMitigation_WhenAlreadyMitigated_Returns409()
    {
        var (controller, repo) = CreateSut();
        repo.CreateMitigationAsync(Arg.Any<ManualMitigationRecord>())
            .Throws(new DuplicateMitigationException("GHSA-test"));

        var result = await controller.CreateMitigation("GHSA-test",
            new CreateMitigationRequest { Description = "Some description" });

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public async Task CreateMitigation_WhenUserNameIsNull_StoresFallbackName()
    {
        var vulnerabilityService = Substitute.For<IVulnerabilityService>();
        var mitigationRepo = Substitute.For<IMitigationRepository>();
        var backOfficeAccessor = Substitute.For<IBackOfficeSecurityAccessor>();
        backOfficeAccessor.BackOfficeSecurity.Returns((IBackOfficeSecurity?)null);

        var controller = new SecurityDashboardController(vulnerabilityService, mitigationRepo, backOfficeAccessor, Substitute.For<IAuditLogRepository>());

        await controller.CreateMitigation("GHSA-test", new CreateMitigationRequest { Description = "Fix" });

        await mitigationRepo.Received(1).CreateMitigationAsync(
            Arg.Is<ManualMitigationRecord>(r => r.MitigatedBy == "Unknown"));
    }

    [Fact]
    public async Task DeleteMitigation_WhenMitigationExists_Returns204()
    {
        var (controller, repo) = CreateSut();
        repo.DeleteMitigationAsync("GHSA-1234-5678-abcd").Returns(true);

        var result = await controller.DeleteMitigation("GHSA-1234-5678-abcd");

        Assert.IsType<NoContentResult>(result);
        await repo.Received(1).DeleteMitigationAsync("GHSA-1234-5678-abcd");
    }

    [Fact]
    public async Task DeleteMitigation_WhenMitigationNotFound_Returns404()
    {
        var (controller, repo) = CreateSut();
        repo.DeleteMitigationAsync("GHSA-not-found").Returns(false);

        var result = await controller.DeleteMitigation("GHSA-not-found");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task DeleteMitigation_ControllerRequiresBackOfficeAccess()
    {
        var attributes = typeof(SecurityDashboardController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        Assert.NotEmpty(attributes);
    }
}
