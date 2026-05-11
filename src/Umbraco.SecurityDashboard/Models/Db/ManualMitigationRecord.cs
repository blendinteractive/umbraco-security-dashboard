using NPoco;

namespace Umbraco.SecurityDashboard.Models.Db;

[TableName("SecurityDashboard_ManualMitigation")]
[PrimaryKey("Id", AutoIncrement = true)]
public class ManualMitigationRecord
{
    public int Id { get; set; }
    public string GhsaId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime MitigatedAt { get; set; }
    public string MitigatedBy { get; set; } = string.Empty;
}
