using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Services;
using Xunit;

// -----------------------------------------------------------------------------
// All tests in this assembly run sequentially (shared goals.json path).
// -----------------------------------------------------------------------------
[assembly: CollectionBehavior(DisableTestParallelization = true)]

public class GoalServiceTests : IDisposable
{
    // Path that GoalService uses internally
    private static readonly string SavePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                     "FitnessTracker", "SaveData", "goals.json");

    // ──────────────────────────────────────────────────────────────────────────
    // CLEANUP: delete the file before *and* after each test
    // ──────────────────────────────────────────────────────────────────────────
    public GoalServiceTests()  => DeleteSaveFile();
    public void Dispose()      => DeleteSaveFile();

    private static void DeleteSaveFile()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // TEST #1 – File location
    // ──────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task SaveGoalAsync_ShouldCreateFile_InFitnessTrackerSaveDataFolder()
    {
        // arrange
        var service = new GoalService();
        var goal    = Goal.FromRunningDistance(new RunningDistance
        {
            Value = 10,
            Unit  = DistanceUnit.Kilometers
        });

        // act
        await service.SaveGoalAsync(goal);

        // assert
        Assert.True(File.Exists(SavePath),
            $"Expected goals.json at: {SavePath}");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // TEST #2 – Round-trip serialize / deserialize
    // ──────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task SaveThenLoad_ShouldYieldSameRunningGoal()
    {
        // arrange
        var originalDistance = new RunningDistance
        {
            Value = 5,
            Unit  = DistanceUnit.Miles
        };
        var goal     = Goal.FromRunningDistance(originalDistance);
        var service1 = new GoalService();

        // act
        var saved   = await service1.SaveGoalAsync(goal);

        var service2 = new GoalService();       // fresh instance
        await service2.LoadGoalsAsync();
        var loaded   = await service2.GetActiveGoalByTypeAsync("Running");

        // assert
        Assert.NotNull(loaded);
        Assert.Equal(saved.Id, loaded.Id);
        Assert.True(loaded.IsRunningGoal);

        var dist = loaded.ToRunningDistance();
        Assert.Equal(originalDistance.Value, dist.Value);
        Assert.Equal(originalDistance.Unit,  dist.Unit);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // TEST #3 – Only one active goal per type
    // ──────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task SavingSecondGoalOfSameType_DeactivatesFirst()
    {
        var service = new GoalService();

        var first  = await service.SaveGoalAsync(
                        Goal.FromWaterContent(new WaterContent { Value = 8, Unit = WaterUnit.Cups }));
        var second = await service.SaveGoalAsync(
                        Goal.FromWaterContent(new WaterContent { Value = 2, Unit = WaterUnit.Liters }));

        // act
        var active = await service.GetActiveGoalByTypeAsync("Water");

        // assert
        Assert.NotNull(active);
        Assert.Equal(second.Id, active.Id);
        Assert.False(first.IsActive);
        Assert.True(second.IsActive);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // TEST #4 – DeleteGoalAsync removes goal and returns true
    // ──────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task DeleteGoalAsync_ShouldRemoveGoal_AndReturnTrue()
    {
        var service = new GoalService();
        var goal    = await service.SaveGoalAsync(
                          Goal.FromRunningDistance(new RunningDistance { Value = 3, Unit = DistanceUnit.Kilometers }));

        // act
        var removed = await service.DeleteGoalAsync(goal.Id);
        var all     = await service.GetAllGoalsAsync();

        // assert
        Assert.True(removed);
        Assert.Empty(all);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // TEST #5 – Ids are incremental per save
    // ──────────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task SaveGoalAsync_ShouldAssignIncrementalIds()
    {
        var service = new GoalService();

        var g1 = await service.SaveGoalAsync(Goal.FromWaterContent(
                     new WaterContent { Value = 1, Unit = WaterUnit.Liters }));
        var g2 = await service.SaveGoalAsync(Goal.FromWaterContent(
                     new WaterContent { Value = 2, Unit = WaterUnit.Liters }));

        Assert.Equal(1, g1.Id);
        Assert.Equal(2, g2.Id);
    }
}
