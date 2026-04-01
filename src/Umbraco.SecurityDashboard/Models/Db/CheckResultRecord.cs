using NPoco;

namespace Umbraco.SecurityDashboard.Models.Db;

[TableName("SecurityDashboard_CheckResult")]
[PrimaryKey("Id", AutoIncrement = true)]
public class CheckResultRecord
{
    public int Id { get; set; }
    public DateTime CheckedAt { get; set; }
    public bool Succeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime NextScheduledCheckAt { get; set; }
}
