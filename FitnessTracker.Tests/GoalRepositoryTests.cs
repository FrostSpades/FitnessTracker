using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using FitnessTracker.Tests.Utils;
using Xunit;

namespace FitnessTracker.Tests;

public sealed class GoalRepositoryTests
{
    [Fact]
    public async Task AddGoalAsync_ShouldStoreGoalInDatabase()
    {
        using var db = DbContextFactory.CreateInMemoryDbContext();
        var repo = new GoalRepository(db);

        var added = await repo.AddGoalAsync(
            Goal.FromRunningDistance(new RunningDistance { Value = 5, Unit = DistanceUnit.Kilometers }));

        var stored = await repo.GetGoalAsync(added.Id);

        Assert.NotNull(stored);
        Assert.Equal(added.Id, stored!.Id);
        Assert.Equal(5, stored.Value);
    }

    [Fact]
    public async Task DeleteGoalAsync_ShouldRemoveGoal()
    {
        using var db = DbContextFactory.CreateInMemoryDbContext();
        var repo = new GoalRepository(db);

        var g = await repo.AddGoalAsync(Goal.FromWaterContent(new WaterContent { Value = 3, Unit = WaterUnit.Liters }));
        var ok = await repo.DeleteGoalAsync(g.Id);

        var all = await repo.GetAllGoalsAsync();

        Assert.True(ok);
        Assert.Empty(all);
    }

    [Fact]
    public async Task GetGoalsByTypeAsync_ShouldFilterCorrectly()
    {
        using var db = DbContextFactory.CreateInMemoryDbContext();
        var repo = new GoalRepository(db);

        await repo.AddGoalAsync(Goal.FromRunningDistance(new RunningDistance { Value = 2, Unit = DistanceUnit.Miles }));
        await repo.AddGoalAsync(Goal.FromWaterContent(new WaterContent { Value = 8, Unit = WaterUnit.Cups }));

        var water = await repo.GetGoalsByTypeAsync(GoalType.Water);

        Assert.Single(water);
        Assert.Equal(GoalType.Water, water[0].Type);
    }
}