using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.SecurityDashboard.Models.Api;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Controllers;

[ApiController]
[Authorize(Policy = "BackOfficeAccess")]
public class SecurityDashboardController : ManagementApiControllerBase
{
    private readonly IVulnerabilityService _vulnerabilityService;

    public SecurityDashboardController(IVulnerabilityService vulnerabilityService)
    {
        _vulnerabilityService = vulnerabilityService;
    }

    /// <summary>
    /// Returns the current vulnerability status for the Umbraco installation.
    /// </summary>
    [HttpGet("umbraco/management/api/v1/security-dashboard/status")]
    [ProducesResponseType(typeof(DashboardStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _vulnerabilityService.GetDashboardStatusAsync();
        return Ok(result);
    }
}
