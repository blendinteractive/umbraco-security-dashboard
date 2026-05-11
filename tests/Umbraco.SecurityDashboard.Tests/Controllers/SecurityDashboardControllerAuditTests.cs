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

public class SecurityDashboardControllerAuditTests
{
    private static SecurityDashboardController CreateSut(
        IAuditLogRepository? auditRepo = null,
        IVulnerabilityService? vulnerabilityService = null,
        IMitigationRepository? mitigationRepo = null,
        IBackOfficeSecurityAccessor? backOfficeSecurityAccessor = null)
    {
        return new SecurityDashboardController(
            vulnerabilityService ?? Substitute.For<IVulnerabilityService>(),
            mitigationRepo ?? Substitute.For<IMitigationRepository>(),
            backOfficeSecurityAccessor ?? Substitute.For<IBackOfficeSecurityAccessor>(),
            auditRepo ?? Substitute.For<IAuditLogRepository>());
    }

    private static IBackOfficeSecurityAccessor CreateBackOfficeAccessor(string userName)
    {
        var accessor = Substitute.For<IBackOfficeSecurityAccessor>();
        var security = Substitute.For<IBackOfficeSecurity>();
        var user = Substitute.For<IUser>();
        user.Name.Returns(userName);
        security.CurrentUser.Returns(user);
        accessor.BackOfficeSecurity.Returns(security);
        return accessor;
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

    // T018 — US3: mitigation audit logging

    [Fact]
    public async Task CreateMitigation_AlwaysCallsAppendAsync_WithManualActionTypeAndActorName()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        var mitigationRepo = Substitute.For<IMitigationRepository>();
        var vulnService = Substitute.For<IVulnerabilityService>();
        vulnService.GetCurrentOverallStatusAsync().Returns("Mitigated");
        var backOfficeAccessor = CreateBackOfficeAccessor("Jane Admin");

        var controller = CreateSut(auditRepo, vulnService, mitigationRepo, backOfficeAccessor);
        await controller.CreateMitigation("GHSA-1234-5678-abcd", new CreateMitigationRequest { Description = "Fixed it" });

        await auditRepo.Received(1).AppendAsync(Arg.Is<AuditLogRecord>(r =>
            r.ActionType == "Manual" &&
            r.ActorName == "Jane Admin" &&
            r.OverallStatus == "Mitigated" &&
            r.Description == "Marked GHSA-1234-5678-abcd as mitigated"));
    }

    [Fact]
    public async Task CreateMitigation_WhenConflict_DoesNotCallAppendAsync()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        var mitigationRepo = Substitute.For<IMitigationRepository>();
        mitigationRepo.CreateMitigationAsync(Arg.Any<ManualMitigationRecord>())
            .Throws(new DuplicateMitigationException("GHSA-test"));

        var controller = CreateSut(auditRepo, mitigationRepo: mitigationRepo);
        await controller.CreateMitigation("GHSA-test", new CreateMitigationRequest { Description = "Fix" });

        await auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditLogRecord>());
    }

    [Fact]
    public async Task DeleteMitigation_AlwaysCallsAppendAsync_WithManualActionTypeAndActorName()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        var mitigationRepo = Substitute.For<IMitigationRepository>();
        mitigationRepo.DeleteMitigationAsync("GHSA-1234-5678-abcd").Returns(true);
        var vulnService = Substitute.For<IVulnerabilityService>();
        vulnService.GetCurrentOverallStatusAsync().Returns("Safe");
        var backOfficeAccessor = CreateBackOfficeAccessor("Bob Admin");

        var controller = CreateSut(auditRepo, vulnService, mitigationRepo, backOfficeAccessor);
        await controller.DeleteMitigation("GHSA-1234-5678-abcd");

        await auditRepo.Received(1).AppendAsync(Arg.Is<AuditLogRecord>(r =>
            r.ActionType == "Manual" &&
            r.ActorName == "Bob Admin" &&
            r.OverallStatus == "Safe" &&
            r.Description == "Removed mitigation for GHSA-1234-5678-abcd"));
    }

    [Fact]
    public async Task DeleteMitigation_WhenNotFound_DoesNotCallAppendAsync()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        var mitigationRepo = Substitute.For<IMitigationRepository>();
        mitigationRepo.DeleteMitigationAsync("GHSA-not-found").Returns(false);

        var controller = CreateSut(auditRepo, mitigationRepo: mitigationRepo);
        await controller.DeleteMitigation("GHSA-not-found");

        await auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditLogRecord>());
    }

    [Fact]
    public async Task CreateMitigation_ActorName_IsPulledFromBackOfficeSecurity()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        var vulnService = Substitute.For<IVulnerabilityService>();
        vulnService.GetCurrentOverallStatusAsync().Returns("Vulnerable");
        var backOfficeAccessor = CreateBackOfficeAccessor("Alice Security");

        var controller = CreateSut(auditRepo, vulnService, backOfficeSecurityAccessor: backOfficeAccessor);
        await controller.CreateMitigation("GHSA-abcd", new CreateMitigationRequest { Description = "desc" });

        await auditRepo.Received(1).AppendAsync(Arg.Is<AuditLogRecord>(r => r.ActorName == "Alice Security"));
    }

    [Fact]
    public async Task DeleteMitigation_ActorName_IsPulledFromBackOfficeSecurity()
    {
        var auditRepo = Substitute.For<IAuditLogRepository>();
        var mitigationRepo = Substitute.For<IMitigationRepository>();
        mitigationRepo.DeleteMitigationAsync(Arg.Any<string>()).Returns(true);
        var vulnService = Substitute.For<IVulnerabilityService>();
        vulnService.GetCurrentOverallStatusAsync().Returns("Safe");
        var backOfficeAccessor = CreateBackOfficeAccessor("Alice Security");

        var controller = CreateSut(auditRepo, vulnService, mitigationRepo, backOfficeAccessor);
        await controller.DeleteMitigation("GHSA-abcd");

        await auditRepo.Received(1).AppendAsync(Arg.Is<AuditLogRecord>(r => r.ActorName == "Alice Security"));
    }
}
