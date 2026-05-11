namespace Umbraco.SecurityDashboard.Models.Api;

public class AuditLogEntryDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? ActorName { get; set; }
    public string Description { get; set; } = string.Empty;
}
