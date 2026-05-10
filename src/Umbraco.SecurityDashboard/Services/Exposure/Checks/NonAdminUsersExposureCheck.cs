using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.SecurityDashboard.Services.Exposure.Checks;

public class NonAdminUsersExposureCheck : IExposureCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public NonAdminUsersExposureCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Keyword => "Non-Admin Backoffice Users";

    public async Task<ExposureVerdict> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var users = await Task.Run(
            () => userService.GetAll(0L, int.MaxValue, out _, "username", Direction.Ascending, userGroups: null, filter: (string?)null),
            cancellationToken);

        return users.Any(u => !u.Groups.Any(g => g.Alias == Constants.Security.AdminGroupAlias))
            ? ExposureVerdict.Vulnerable
            : ExposureVerdict.Mitigated;
    }
}
