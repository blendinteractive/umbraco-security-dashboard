using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.SecurityDashboard.Models.Api;
using Umbraco.SecurityDashboard.Models.Db;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("umbraco/management/api/v{version:apiVersion}/security-dashboard")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class SecurityDashboardController : ManagementApiControllerBase
{
    private readonly IVulnerabilityService _vulnerabilityService;
    private readonly IMitigationRepository _mitigationRepository;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public SecurityDashboardController(
        IVulnerabilityService vulnerabilityService,
        IMitigationRepository mitigationRepository,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _vulnerabilityService = vulnerabilityService;
        _mitigationRepository = mitigationRepository;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(DashboardStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _vulnerabilityService.GetDashboardStatusAsync();
        return Ok(result);
    }

    [HttpPost("advisories/{ghsaId}/mitigations")]
    [ProducesResponseType(typeof(ManualMitigationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateMitigation(string ghsaId, [FromBody] CreateMitigationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var mitigatedBy = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Name ?? "Unknown";

        var record = new ManualMitigationRecord
        {
            GhsaId = ghsaId,
            Description = request.Description,
            MitigatedAt = DateTime.UtcNow,
            MitigatedBy = mitigatedBy
        };

        try
        {
            await _mitigationRepository.CreateMitigationAsync(record);
        }
        catch (DuplicateMitigationException)
        {
            return Conflict(new ProblemDetails { Title = $"Advisory {ghsaId} is already manually mitigated." });
        }

        return StatusCode(StatusCodes.Status201Created, new ManualMitigationDto
        {
            Description = record.Description,
            MitigatedAt = record.MitigatedAt,
            MitigatedBy = record.MitigatedBy
        });
    }
}
