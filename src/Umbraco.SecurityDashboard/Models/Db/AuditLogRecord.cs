using NPoco;

namespace Umbraco.SecurityDashboard.Models.Db;

[TableName("SecurityDashboard_AuditLog")]
[PrimaryKey("Id", AutoIncrement = true)]
public class AuditLogRecord
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? ActorName { get; set; }
    public string Description { get; set; } = string.Empty;
}
