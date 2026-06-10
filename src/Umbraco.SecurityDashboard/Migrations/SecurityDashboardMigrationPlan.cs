using Umbraco.Cms.Core.Packaging;

namespace Umbraco.SecurityDashboard.Migrations;

public class SecurityDashboardMigrationPlan : PackageMigrationPlan
{
    public SecurityDashboardMigrationPlan() : base("SecurityDashboard")
    {
    }

    protected override void DefinePlan()
    {
        From(string.Empty)
            .To<CreateSecurityDashboardTables>("SecurityDashboard-1.0.0")
            .To<AddManualMitigationTable>("SecurityDashboard-1.1.0")
            .To<AddAuditLogTable>("SecurityDashboard-1.2.0")
            .To<AddExposureCheckDescriptionColumn>("SecurityDashboard-1.3.0");
    }
}
