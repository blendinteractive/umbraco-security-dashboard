using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.SecurityDashboard.Models.Db;
using Umbraco.SecurityDashboard.Services;

namespace Umbraco.SecurityDashboard.Tests.Services;

public class MitigationRepositoryTests
{
    private static (MitigationRepository Repo, IScope Scope) CreateSut()
    {
        var scopeProvider = Substitute.For<IScopeProvider>();
        var scope = Substitute.For<IScope>();
        scopeProvider.CreateScope().Returns(scope);
        return (new MitigationRepository(scopeProvider), scope);
    }

    [Fact]
    public async Task GetAllMitigationsAsync_ReturnsMappedRecords()
    {
        var (repo, scope) = CreateSut();
        var records = new List<ManualMitigationRecord>
        {
            new() { Id = 1, GhsaId = "GHSA-1111-2222-aaaa", Description = "Fixed", MitigatedAt = DateTime.UtcNow, MitigatedBy = "Admin" }
        };
        scope.Database.Fetch<ManualMitigationRecord>(Arg.Any<string>()).Returns(records);

        var result = await repo.GetAllMitigationsAsync();

        Assert.Single(result);
        Assert.Equal("GHSA-1111-2222-aaaa", result[0].GhsaId);
        scope.Received(1).Complete();
    }

    [Fact]
    public async Task GetAllMitigationsAsync_WhenNoRecords_ReturnsEmptyList()
    {
        var (repo, scope) = CreateSut();
        scope.Database.Fetch<ManualMitigationRecord>(Arg.Any<string>())
            .Returns(new List<ManualMitigationRecord>());

        var result = await repo.GetAllMitigationsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateMitigationAsync_InsertsRecord()
    {
        var (repo, scope) = CreateSut();
        var record = new ManualMitigationRecord
        {
            GhsaId = "GHSA-1234-5678-abcd",
            Description = "Applied WAF rule.",
            MitigatedAt = DateTime.UtcNow,
            MitigatedBy = "Jane Admin"
        };

        await repo.CreateMitigationAsync(record);

        scope.Database.Received(1).Insert(record);
        scope.Received(1).Complete();
    }

    [Fact]
    public async Task CreateMitigationAsync_WhenUniqueConstraintViolated_ThrowsDuplicateMitigationException()
    {
        var (repo, scope) = CreateSut();
        var record = new ManualMitigationRecord { GhsaId = "GHSA-already-exists" };
        scope.Database.Insert(record).Throws(new Exception("UNIQUE constraint failed"));

        await Assert.ThrowsAsync<DuplicateMitigationException>(() => repo.CreateMitigationAsync(record));
    }

    [Fact]
    public async Task DeleteMitigationAsync_WhenRecordExists_ReturnsTrueAndDeletes()
    {
        var (repo, scope) = CreateSut();
        scope.Database.Execute(Arg.Any<string>(), Arg.Any<object[]>()).Returns(1);

        var result = await repo.DeleteMitigationAsync("GHSA-1234-5678-abcd");

        Assert.True(result);
        scope.Received(1).Complete();
    }

    [Fact]
    public async Task DeleteMitigationAsync_WhenRecordNotFound_ReturnsFalse()
    {
        var (repo, scope) = CreateSut();
        scope.Database.Execute(Arg.Any<string>(), Arg.Any<object[]>()).Returns(0);

        var result = await repo.DeleteMitigationAsync("GHSA-does-not-exist");

        Assert.False(result);
        scope.Received(1).Complete();
    }
}
