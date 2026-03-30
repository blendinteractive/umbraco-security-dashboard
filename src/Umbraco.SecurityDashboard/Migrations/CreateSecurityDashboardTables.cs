using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.SecurityDashboard.Migrations;

public class CreateSecurityDashboardTables : AsyncMigrationBase
{
    public CreateSecurityDashboardTables(IMigrationContext context)
        : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        if (!TableExists("SecurityDashboard_CheckResult"))
        {
            Create.Table("SecurityDashboard_CheckResult")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("CheckedAt").AsDateTime().NotNullable()
                .WithColumn("Succeeded").AsBoolean().NotNullable()
                .WithColumn("ErrorMessage").AsString(2000).Nullable()
                .WithColumn("NextScheduledCheckAt").AsDateTime().NotNullable()
                .Do();
        }

        if (!TableExists("SecurityDashboard_Advisory"))
        {
            Create.Table("SecurityDashboard_Advisory")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("CheckResultId").AsInt32().NotNullable()
                .WithColumn("GhsaId").AsString(50).NotNullable()
                .WithColumn("Title").AsString(500).NotNullable()
                .WithColumn("Severity").AsString(20).NotNullable()
                .WithColumn("PackageName").AsString(200).NotNullable()
                .WithColumn("AffectedVersionRange").AsString(100).NotNullable()
                .WithColumn("AdvisoryUrl").AsString(500).NotNullable()
                .WithColumn("PublishedAt").AsDateTime().NotNullable()
                .WithColumn("InstalledVersion").AsString(50).Nullable()
                .WithColumn("AffectedStatus").AsString(20).NotNullable()
                .Do();

            Create.Index("IX_SecurityDashboard_Advisory_CheckResultId")
                .OnTable("SecurityDashboard_Advisory")
                .OnColumn("CheckResultId")
                .Ascending()
                .Do();

            Create.Index("IX_SecurityDashboard_Advisory_AffectedStatus_PublishedAt")
                .OnTable("SecurityDashboard_Advisory")
                .OnColumn("AffectedStatus").Ascending()
                .OnColumn("PublishedAt").Descending()
                .Do();
        }

        return Task.CompletedTask;
    }
}
