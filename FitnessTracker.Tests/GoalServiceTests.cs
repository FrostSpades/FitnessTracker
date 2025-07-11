using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using FitnessTracker.Services;
using FitnessTracker.Tests.Utils;
using Xunit;

namespace FitnessTracker.Tests;

public sealed class GoalServiceTests
{
    private static GoalService MakeSvc()
    {
        var db = DbContextFactory.CreateInMemoryDbContext();
        return new GoalService(new GoalRepository(db));
    }

    [Fact]
    public async Task SaveGoalAsync_ShouldSetGoalAsActive()
    {
        var svc = MakeSvc();

        var goal = await svc.SaveGoalAsync(Goal.FromWaterContent(new WaterContent { Value = 2, Unit = WaterUnit.Liters }));
        Assert.True(goal.IsActive);

        var active = await svc.GetActiveGoalByTypeAsync(GoalType.Water);
        Assert.Equal(goal.Id, active?.Id);
    }

    [Fact]
    public async Task SaveGoalAsync_ShouldDeactivatePreviousGoalOfSameType()
    {
        var svc = MakeSvc();

        var g1 = await svc.SaveGoalAsync(Goal.FromWaterContent(new WaterContent { Value = 1, Unit = WaterUnit.Liters }));
        var g2 = await svc.SaveGoalAsync(Goal.FromWaterContent(new WaterContent { Value = 2, Unit = WaterUnit.Liters }));

        var all = await svc.GetAllGoalsAsync();

        var g1Repo = all.First(g => g.Id == g1.Id);
        var g2Repo = all.First(g => g.Id == g2.Id);

        Assert.False(g1Repo.IsActive);
        Assert.True(g2Repo.IsActive);
    }

    [Fact]
    public async Task DeleteGoalAsync_ShouldRemoveItFromDatabase()
    {
        var svc = MakeSvc();

        var goal = await svc.SaveGoalAsync(Goal.FromRunningDistance(new RunningDistance { Value = 4, Unit = DistanceUnit.Miles }));
        var result = await svc.DeleteGoalAsync(goal.Id);

        var remaining = await svc.GetAllGoalsAsync();

        Assert.True(result);
        Assert.Empty(remaining);
    }
}