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
            // Umbraco's Alter.Table builder rejects SQLite; use raw SQL which both engines support
            var isSqlite = Database.DatabaseType.GetType().Name
                .Contains("SQLite", StringComparison.OrdinalIgnoreCase);
            var columnType = isSqlite ? "TEXT" : "NVARCHAR(MAX)";
            Database.Execute($"ALTER TABLE SecurityDashboard_Advisory ADD ExposureCheckDescription {columnType} NULL");
        }

        return Task.CompletedTask;
    }
}
