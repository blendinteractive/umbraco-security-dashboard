namespace Umbraco.SecurityDashboard.Models.Api;

public class AuditLogPageResponse
{
    public IReadOnlyList<AuditLogEntryDto> Entries { get; set; } = [];
    public int TotalCount { get; set; }
}
