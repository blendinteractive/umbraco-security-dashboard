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
    private readonly IAuditLogRepository _auditLogRepository;

    public SecurityDashboardController(
        IVulnerabilityService vulnerabilityService,
        IMitigationRepository mitigationRepository,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAuditLogRepository auditLogRepository)
    {
        _vulnerabilityService = vulnerabilityService;
        _mitigationRepository = mitigationRepository;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _auditLogRepository = auditLogRepository;
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(DashboardStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _vulnerabilityService.GetDashboardStatusAsync();
        return Ok(result);
    }

    /// <summary>Returns a reverse-chronological paginated list of security audit log entries.</summary>
    [HttpGet("audit-log")]
    [ProducesResponseType(typeof(AuditLogPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAuditLog([FromQuery] int skip = 0, [FromQuery] int take = 25)
    {
        var clampedTake = Math.Min(take, 100);
        var page = await _auditLogRepository.GetPagedAsync(skip, clampedTake);

        return Ok(new AuditLogPageResponse
        {
            Entries = page.Entries.Select(e => new AuditLogEntryDto
            {
                Id = e.Id,
                Timestamp = e.Timestamp,
                OverallStatus = e.OverallStatus,
                ActionType = e.ActionType,
                ActorName = e.ActorName,
                Description = e.Description
            }).ToList(),
            TotalCount = page.TotalCount
        });
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

        var currentStatus = await _vulnerabilityService.GetCurrentOverallStatusAsync();
        await _auditLogRepository.AppendAsync(new AuditLogRecord
        {
            Timestamp = DateTime.UtcNow,
            OverallStatus = currentStatus,
            ActionType = "Manual",
            ActorName = mitigatedBy,
            Description = $"Marked {ghsaId} as mitigated"
        });

        return StatusCode(StatusCodes.Status201Created, new ManualMitigationDto
        {
            Description = record.Description,
            MitigatedAt = record.MitigatedAt,
            MitigatedBy = record.MitigatedBy
        });
    }

    [HttpDelete("advisories/{ghsaId}/mitigations")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMitigation(string ghsaId)
    {
        var deleted = await _mitigationRepository.DeleteMitigationAsync(ghsaId);
        if (!deleted)
            return NotFound(new ProblemDetails { Title = $"No manual mitigation found for advisory {ghsaId}." });

        var actorName = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Name ?? "Unknown";
        var currentStatus = await _vulnerabilityService.GetCurrentOverallStatusAsync();
        await _auditLogRepository.AppendAsync(new AuditLogRecord
        {
            Timestamp = DateTime.UtcNow,
            OverallStatus = currentStatus,
            ActionType = "Manual",
            ActorName = actorName,
            Description = $"Removed mitigation for {ghsaId}"
        });

        return NoContent();
    }
}
