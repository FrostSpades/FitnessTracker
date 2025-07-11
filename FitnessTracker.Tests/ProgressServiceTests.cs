using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using FitnessTracker.Services;
using FitnessTracker.Tests.Utils;
using Xunit;

namespace FitnessTracker.Tests;

public sealed class ProgressServiceTests
{
    private static ProgressService MakeSvc()
    {
        var db = DbContextFactory.CreateInMemoryDbContext();
        var repo = new ProgressRepository(db);
        return new ProgressService(repo);
    }

    [Fact]
    public async Task SaveProgressAsync_ShouldAssignIdAndTimestamp()
    {
        var svc = MakeSvc();
        var result = await svc.SaveProgressAsync(GoalType.Water, 1.0f, DistanceUnit.Kilometers, WaterUnit.Liters, "note");

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.Timestamp > DateTime.MinValue);
    }

    [Fact]
    public async Task GetProgressHistoryAsync_ShouldReturnDescendingTimestamps()
    {
        var svc = MakeSvc();

        await svc.SaveProgressAsync(GoalType.Running, 1, DistanceUnit.Miles, WaterUnit.Ounces, null);
        await Task.Delay(10); // ensure different timestamps
        await svc.SaveProgressAsync(GoalType.Running, 2, DistanceUnit.Miles, WaterUnit.Ounces, null);

        var history = (await svc.GetProgressHistoryAsync("Running")).ToList();

        Assert.Equal(2, history.Count);
        Assert.True(history[0].Timestamp >= history[1].Timestamp);
    }

    [Fact]
    public async Task GetLatestProgressAsync_ShouldReturnMostRecent()
    {
        var svc = MakeSvc();

        var first = await svc.SaveProgressAsync(GoalType.Water, 1f, DistanceUnit.Miles, WaterUnit.Ounces, null);
        await Task.Delay(10);
        var second = await svc.SaveProgressAsync(GoalType.Water, 2f, DistanceUnit.Miles, WaterUnit.Ounces, null);

        var latest = await svc.GetLatestProgressAsync("Water");

        Assert.NotNull(latest);
        Assert.Equal(second.Id, latest!.Id);
    }

    [Fact]
    public async Task GetProgressHistoryAsync_WithNoData_ShouldReturnEmpty()
    {
        var svc = MakeSvc();
        var result = await svc.GetProgressHistoryAsync("FakeGoal");

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
