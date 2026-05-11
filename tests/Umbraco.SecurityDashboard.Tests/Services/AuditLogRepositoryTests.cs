using NSubstitute;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.SecurityDashboard.Models.Db;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class AuditLogRepositoryTests
{
    private static (AuditLogRepository Repo, IScope Scope) CreateSut()
    {
        var scopeProvider = Substitute.For<IScopeProvider>();
        var scope = Substitute.For<IScope>();
        scopeProvider.CreateScope().Returns(scope);
        return (new AuditLogRepository(scopeProvider), scope);
    }

    private static AuditLogRecord MakeRecord(int id = 1) => new()
    {
        Id = id,
        Timestamp = DateTime.UtcNow,
        OverallStatus = "Safe",
        ActionType = "Automatic",
        ActorName = null,
        Description = "Scheduled vulnerability check completed"
    };

    [Fact]
    public async Task AppendAsync_InsertsRecord()
    {
        var (repo, scope) = CreateSut();
        var record = MakeRecord();

        await repo.AppendAsync(record);

        scope.Database.Received(1).Insert(record);
        scope.Received(1).Complete();
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPageAndTotalCount()
    {
        var (repo, scope) = CreateSut();
        var records = new List<AuditLogRecord> { MakeRecord(1), MakeRecord(2) };

        scope.Database.Fetch<AuditLogRecord>(0L, 25L, Arg.Any<string>())
            .Returns(records);
        scope.Database.ExecuteScalar<int>(Arg.Any<string>())
            .Returns(5);

        var result = await repo.GetPagedAsync(0, 25);

        Assert.Equal(2, result.Entries.Count);
        Assert.Equal(5, result.TotalCount);
        scope.Received(1).Complete();
    }

    [Fact]
    public async Task GetPagedAsync_WhenNoRecords_ReturnsEmptyPageWithZeroCount()
    {
        var (repo, scope) = CreateSut();

        scope.Database.Fetch<AuditLogRecord>(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<string>())
            .Returns(new List<AuditLogRecord>());
        scope.Database.ExecuteScalar<int>(Arg.Any<string>())
            .Returns(0);

        var result = await repo.GetPagedAsync(0, 25);

        Assert.Empty(result.Entries);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_PassesSkipAndTakeToDatabase()
    {
        var (repo, scope) = CreateSut();

        scope.Database.Fetch<AuditLogRecord>(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<string>())
            .Returns(new List<AuditLogRecord>());
        scope.Database.ExecuteScalar<int>(Arg.Any<string>())
            .Returns(0);

        await repo.GetPagedAsync(50, 25);

        scope.Database.Received(1).Fetch<AuditLogRecord>(50L, 25L, Arg.Any<string>());
    }
}
