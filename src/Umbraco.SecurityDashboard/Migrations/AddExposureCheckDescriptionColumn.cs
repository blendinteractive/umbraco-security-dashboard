using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.SecurityDashboard.Migrations;

public class AddExposureCheckDescriptionColumn : AsyncMigrationBase
{
    public AddExposureCheckDescriptionColumn(IMigrationContext context) : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        if (!ColumnExists("SecurityDashboard_Advisory", "ExposureCheckDescription"))
        {
            Alter.Table("SecurityDashboard_Advisory")
                .AddColumn("ExposureCheckDescription").AsString(int.MaxValue).Nullable()
                .Do();
        }

        return Task.CompletedTask;
    }
}
