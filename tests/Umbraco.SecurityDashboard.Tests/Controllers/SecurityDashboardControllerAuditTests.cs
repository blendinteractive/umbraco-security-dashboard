using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Umbraco.Cms.Core.Security;
using Umbraco.SecurityDashboard.Controllers;
using Umbraco.SecurityDashboard.Models.Api;
using Umbraco.SecurityDashboard.Models.Db;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Controllers;

public class SecurityDashboardControllerAuditTests
{
    private static SecurityDashboardController CreateSut(
        IAuditLogRepository? auditRepo = null,
        IVulnerabilityService? vulnerabilityService = null)
    {
        return new SecurityDashboardController(
            vulnerabilityService ?? Substitute.For<IVulnerabilityService>(),
            Substitute.For<IMitigationRepository>(),
            Substitute.For<IBackOfficeSecurityAccessor>(),
            auditRepo ?? Substitute.For<IAuditLogRepository>());
    }

    [Fact]
    public async Task GetAuditLog_ReturnsOkWithAuditLogPageResponse()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        var entries = new List<AuditLogRecord>
        {
            new() { Id = 1, Timestamp = DateTime.UtcNow, OverallStatus = "Vulnerable", ActionType = "Automatic", ActorName = null, Description = "Scheduled check" }
        };
        auditRepo.GetPagedAsync(0, 25).Returns(new AuditLogPage(entries, 1));

        var controller = CreateSut(auditRepo);
        var result = await controller.GetAuditLog(0, 25);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuditLogPageResponse>(ok.Value);
        Assert.Equal(1, response.TotalCount);
        Assert.Single(response.Entries);
        Assert.Equal(1, response.Entries[0].Id);
        Assert.Equal("Vulnerable", response.Entries[0].OverallStatus);
        Assert.Equal("Automatic", response.Entries[0].ActionType);
        Assert.Null(response.Entries[0].ActorName);
        Assert.Equal("Scheduled check", response.Entries[0].Description);
    }

    [Fact]
    public async Task GetAuditLog_WhenLogIsEmpty_ReturnsEmptyEntriesAndZeroTotalCount()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        auditRepo.GetPagedAsync(0, 25).Returns(new AuditLogPage([], 0));

        var controller = CreateSut(auditRepo);
        var result = await controller.GetAuditLog(0, 25);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuditLogPageResponse>(ok.Value);
        Assert.Equal(0, response.TotalCount);
        Assert.Empty(response.Entries);
    }

    [Fact]
    public async Task GetAuditLog_ClampsTakeTo100()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        auditRepo.GetPagedAsync(0, 100).Returns(new AuditLogPage([], 0));

        var controller = CreateSut(auditRepo);
        await controller.GetAuditLog(0, 200);

        await auditRepo.Received(1).GetPagedAsync(0, 100);
    }

    [Fact]
    public async Task GetAuditLog_MapsAllDtoFieldsCorrectly()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        var ts = new DateTime(2026, 5, 11, 14, 32, 0, DateTimeKind.Utc);
        var entries = new List<AuditLogRecord>
        {
            new() { Id = 42, Timestamp = ts, OverallStatus = "Mitigated", ActionType = "Manual", ActorName = "Jane Admin", Description = "Marked GHSA-1234 as mitigated" }
        };
        auditRepo.GetPagedAsync(0, 25).Returns(new AuditLogPage(entries, 1));

        var controller = CreateSut(auditRepo);
        var result = await controller.GetAuditLog(0, 25);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuditLogPageResponse>(ok.Value);
        var dto = response.Entries[0];
        Assert.Equal(42, dto.Id);
        Assert.Equal(ts, dto.Timestamp);
        Assert.Equal("Mitigated", dto.OverallStatus);
        Assert.Equal("Manual", dto.ActionType);
        Assert.Equal("Jane Admin", dto.ActorName);
        Assert.Equal("Marked GHSA-1234 as mitigated", dto.Description);
    }

    [Fact]
    public async Task GetAuditLog_UsesDefaultSkipAndTake()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        auditRepo.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(new AuditLogPage([], 0));

        var controller = CreateSut(auditRepo);
        await controller.GetAuditLog();

        await auditRepo.Received(1).GetPagedAsync(0, 25);
    }
}
