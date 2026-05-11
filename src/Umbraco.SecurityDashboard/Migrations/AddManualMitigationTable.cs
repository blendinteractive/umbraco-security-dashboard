using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.SecurityDashboard.Migrations;

public class AddManualMitigationTable : AsyncMigrationBase
{
    public AddManualMitigationTable(IMigrationContext context) : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        if (!TableExists("SecurityDashboard_ManualMitigation"))
        {
            Create.Table("SecurityDashboard_ManualMitigation")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity().NotNullable()
                .WithColumn("GhsaId").AsString(50).NotNullable()
                .WithColumn("Description").AsString(2000).NotNullable()
                .WithColumn("MitigatedAt").AsDateTime().NotNullable()
                .WithColumn("MitigatedBy").AsString(500).NotNullable()
                .Do();

            Create.Index("IX_SecurityDashboard_ManualMitigation_GhsaId")
                .OnTable("SecurityDashboard_ManualMitigation")
                .OnColumn("GhsaId")
                .Ascending()
                .WithOptions().Unique()
                .Do();
        }

        return Task.CompletedTask;
    }
}
