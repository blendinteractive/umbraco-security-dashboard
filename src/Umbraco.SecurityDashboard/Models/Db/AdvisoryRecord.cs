using NPoco;

namespace Umbraco.SecurityDashboard.Models.Db;

[TableName("SecurityDashboard_Advisory")]
[PrimaryKey("Id", AutoIncrement = true)]
public class AdvisoryRecord
{
    public int Id { get; set; }
    public int CheckResultId { get; set; }
    public string GhsaId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string AffectedVersionRange { get; set; } = string.Empty;
    public string AdvisoryUrl { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string? InstalledVersion { get; set; }
    public string AffectedStatus { get; set; } = string.Empty;
}
