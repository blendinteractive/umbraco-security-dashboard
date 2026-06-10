using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.SecurityDashboard.Services.Exposure;
using Umbraco.SecurityDashboard.Services.Exposure.Checks;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class NonAdminUsersExposureCheckTests
{
    private static NonAdminUsersExposureCheck CreateSut(IEnumerable<IUser> users)
    {
        var userService = Substitute.For<IUserService>();
        long totalRecords;
        // Explicitly match overload: GetAll(long, int, out long, string, Direction, UserState[]?, string[]?, string?)
        userService
            .GetAll(Arg.Any<long>(), Arg.Any<int>(), out totalRecords, Arg.Any<string>(), Arg.Any<Direction>(),
                Arg.Any<UserState[]?>(), Arg.Any<string[]?>(), Arg.Any<string?>())
            .Returns(users);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IUserService)).Returns(userService);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        return new NonAdminUsersExposureCheck(scopeFactory);
    }

    private static IUser MakeUser(params string[] groupAliases)
    {
        var groups = groupAliases.Select(alias =>
        {
            var group = Substitute.For<IReadOnlyUserGroup>();
            group.Alias.Returns(alias);
            return group;
        }).Cast<IReadOnlyUserGroup>();

        var user = Substitute.For<IUser>();
        user.Groups.Returns(groups);
        return user;
    }

    [Fact]
    public async Task CheckAsync_EmptyUserList_ReturnsMitigated()
    {
        var sut = CreateSut([]);
        var result = await sut.CheckAsync();
        Assert.Equal(ExposureVerdict.Mitigated, result.Verdict);
    }

    [Fact]
    public async Task CheckAsync_AllUsersAreAdmins_ReturnsMitigated()
    {
        var adminUser = MakeUser(Constants.Security.AdminGroupAlias);
        var sut = CreateSut([adminUser]);
        var result = await sut.CheckAsync();
        Assert.Equal(ExposureVerdict.Mitigated, result.Verdict);
    }

    [Fact]
    public async Task CheckAsync_OneNonAdminUser_ReturnsVulnerable()
    {
        var nonAdminUser = MakeUser("editor");
        var sut = CreateSut([nonAdminUser]);
        var result = await sut.CheckAsync();
        Assert.Equal(ExposureVerdict.Vulnerable, result.Verdict);
    }

    [Fact]
    public async Task CheckAsync_MixedAdminAndNonAdmin_ReturnsVulnerable()
    {
        var adminUser = MakeUser(Constants.Security.AdminGroupAlias);
        var nonAdminUser = MakeUser("editor");
        var sut = CreateSut([adminUser, nonAdminUser]);
        var result = await sut.CheckAsync();
        Assert.Equal(ExposureVerdict.Vulnerable, result.Verdict);
    }

    // --- T016: Description tests ---

    [Fact]
    public async Task CheckAsync_EmptyUserList_ReturnsMitigationDescription()
    {
        var sut = CreateSut([]);
        var result = await sut.CheckAsync();
        Assert.Equal("All backoffice users are administrators", result.MitigationDescription);
    }

    [Fact]
    public async Task CheckAsync_AllUsersAreAdmins_ReturnsMitigationDescription()
    {
        var adminUser = MakeUser(Constants.Security.AdminGroupAlias);
        var sut = CreateSut([adminUser]);
        var result = await sut.CheckAsync();
        Assert.Equal("All backoffice users are administrators", result.MitigationDescription);
    }

    [Fact]
    public async Task CheckAsync_OneNonAdminUser_MitigationDescriptionIsNull()
    {
        var nonAdminUser = MakeUser("editor");
        var sut = CreateSut([nonAdminUser]);
        var result = await sut.CheckAsync();
        Assert.Null(result.MitigationDescription);
    }
}
