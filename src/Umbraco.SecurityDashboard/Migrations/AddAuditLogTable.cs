using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.SecurityDashboard.Migrations;

public class AddAuditLogTable : AsyncMigrationBase
{
    public AddAuditLogTable(IMigrationContext context) : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        if (!TableExists("SecurityDashboard_AuditLog"))
        {
            Create.Table("SecurityDashboard_AuditLog")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
                .WithColumn("Timestamp").AsDateTime().NotNullable()
                .WithColumn("OverallStatus").AsString(20).NotNullable()
                .WithColumn("ActionType").AsString(10).NotNullable()
                .WithColumn("ActorName").AsString(500).Nullable()
                .WithColumn("Description").AsString(1000).NotNullable()
                .Do();

            Create.Index("IX_SecurityDashboard_AuditLog_Timestamp")
                .OnTable("SecurityDashboard_AuditLog")
                .OnColumn("Timestamp")
                .Descending()
                .Do();
        }

        return Task.CompletedTask;
    }
}
