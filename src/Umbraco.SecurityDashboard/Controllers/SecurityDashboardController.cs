using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.SecurityDashboard.Models.Api;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("umbraco/management/api/v{version:apiVersion}/security-dashboard")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class SecurityDashboardController : ManagementApiControllerBase
{
    private readonly IVulnerabilityService _vulnerabilityService;

    public SecurityDashboardController(IVulnerabilityService vulnerabilityService)
    {
        _vulnerabilityService = vulnerabilityService;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(DashboardStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _vulnerabilityService.GetDashboardStatusAsync();
        return Ok(result);
    }
}
