using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using FitnessTracker.Tests.Utils;
using Xunit;

namespace FitnessTracker.Tests;

public sealed class ProgressRepositoryTests
{
    private static ProgressRepository MakeRepo(out FitnessTracker.Data.FitnessTrackerDbContext db)
    {
        db = DbContextFactory.CreateInMemoryDbContext();
        return new ProgressRepository(db);
    }

    [Fact]
    public async Task LoadAsync_WithNoData_ShouldReturnEmptyList()
    {
        var repo = MakeRepo(out _);
        var result = await repo.LoadAsync("Running");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistEntries()
    {
        var repo = MakeRepo(out _);
        var goal = "Water";
        var entry = new ProgressEntry
        {
            Id = Guid.NewGuid(),
            GoalType = goal,
            Value = 1.5f,
            Unit = WaterUnit.Liters.ToString(),
            Timestamp = DateTime.UtcNow
        };

        await repo.SaveAsync(goal, new List<ProgressEntry> { entry });

        var loaded = await repo.LoadAsync(goal);
        Assert.Single(loaded);
        Assert.Equal(1.5f, loaded[0].Value);
    }

    [Fact]
    public async Task SaveAsync_ShouldOverwriteExistingEntries()
    {
        var repo = MakeRepo(out _);
        var goal = "Calories";

        await repo.SaveAsync(goal, new List<ProgressEntry> { new() { Id = Guid.NewGuid(), GoalType = goal, Value = 10, Unit = "kcal", Timestamp = DateTime.UtcNow } });
        await repo.SaveAsync(goal, new List<ProgressEntry> { new() { Id = Guid.NewGuid(), GoalType = goal, Value = 99, Unit = "kcal", Timestamp = DateTime.UtcNow } });

        var loaded = await repo.LoadAsync(goal);

        Assert.Single(loaded);
        Assert.Equal(99, loaded[0].Value);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnEntriesSortedByTimestampDescending()
    {
        var repo = MakeRepo(out _);
        var goal = "Running";

        var oldEntry = new ProgressEntry
        {
            Id = Guid.NewGuid(),
            GoalType = goal,
            Value = 1,
            Unit = "km",
            Timestamp = DateTime.UtcNow.AddMinutes(-10)
        };

        var newEntry = new ProgressEntry
        {
            Id = Guid.NewGuid(),
            GoalType = goal,
            Value = 2,
            Unit = "km",
            Timestamp = DateTime.UtcNow
        };

        await repo.SaveAsync(goal, new List<ProgressEntry> { oldEntry, newEntry });

        var result = await repo.LoadAsync(goal);
        Assert.Equal(2, result.Count);
        Assert.Equal(newEntry.Id, result[0].Id);
    }
}
