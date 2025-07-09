using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using Xunit;

namespace FitnessTracker.Tests;

/// <summary>
/// Integration-style tests for <see cref="ProgressRepository"/> that hit the real file‑system.
/// </summary>
public sealed class ProgressRepositoryTests : IDisposable
{
    private static readonly string SaveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData");

    public ProgressRepositoryTests() => Clean();
    public void Dispose() => Clean();

    private static void Clean()
    {
        if (Directory.Exists(SaveDir))
            Directory.Delete(SaveDir, recursive: true);
    }

    private static ProgressRepository MakeRepo() => new();

    private static string GetFilePath(string goalType) =>
        Path.Combine(SaveDir, $"{goalType.ToLowerInvariant()}_progress.json");

    /* ------------------------------------------------------------------
     *  Standard CRUD scenarios
     * ------------------------------------------------------------------*/

    [Fact]
    public async Task SaveAsync_ShouldCreateFile()
    {
        var repo   = MakeRepo();
        var goal   = "Running";
        var values = new List<ProgressEntry> { new() { GoalType = goal, Value = 3.5f, Unit = DistanceUnit.Kilometers.ToString() } };

        await repo.SaveAsync(goal, values);

        Assert.True(File.Exists(GetFilePath(goal)));
    }

    [Fact]
    public async Task LoadAsync_WithNoFile_ShouldReturnEmpty()
    {
        var repo   = MakeRepo();
        var result = await repo.LoadAsync("DoesNotExist");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveThenLoad_ShouldPreserveEntryData()
    {
        // Arrange
        var repo  = MakeRepo();
        var goal  = "Water";
        var water = new WaterContent { Value = 1.5f, Unit = WaterUnit.Liters };

        var entry = ProgressEntry.FromWaterContent(water);
        var entries = new List<ProgressEntry> { entry };

        // Act
        await repo.SaveAsync(goal, entries);
        var loaded = await repo.LoadAsync(goal);

        // Assert
        Assert.Single(loaded);
        Assert.Equal(goal,             loaded[0].GoalType);
        Assert.Equal(water.Value,      loaded[0].Value);
        Assert.Equal(water.Unit.ToString(), loaded[0].Unit);
    }

    [Fact]
    public async Task SaveMultipleEntries_ShouldPreserveAll()
    {
        var repo = MakeRepo();
        var goal = "Running";

        var entries = new List<ProgressEntry>
        {
            new() { GoalType = goal, Value = 1, Unit = DistanceUnit.Miles.ToString() },
            new() { GoalType = goal, Value = 2, Unit = DistanceUnit.Miles.ToString() }
        };

        await repo.SaveAsync(goal, entries);
        var loaded = await repo.LoadAsync(goal);

        Assert.Equal(2, loaded.Count);
        Assert.Equal(1, loaded[0].Value);
        Assert.Equal(2, loaded[1].Value);
    }

    [Fact]
    public async Task SaveAsync_ShouldOverwritePreviousFile()
    {
        var repo = MakeRepo();
        var goal = "Calories";

        await repo.SaveAsync(goal, new List<ProgressEntry> { new() { GoalType = goal, Value = 1, Unit = "kcal" } });
        await repo.SaveAsync(goal, new List<ProgressEntry> { new() { GoalType = goal, Value = 99, Unit = "kcal" } });

        var loaded = await repo.LoadAsync(goal);

        Assert.Single(loaded);
        Assert.Equal(99, loaded[0].Value);
    }

    /* ------------------------------------------------------------------
     *  Repository‑specific behaviors (atomic save, error handling, concurrency)
     * ------------------------------------------------------------------*/

    [Fact]
    public async Task SaveAsync_ShouldRemoveTempFile()
    {
        var repo = MakeRepo();
        var goal = "Steps";

        await repo.SaveAsync(goal, new List<ProgressEntry> { new() { GoalType = goal, Value = 10_000, Unit = "count" } });

        var tmpPath = GetFilePath(goal) + ".tmp";
        Assert.False(File.Exists(tmpPath));
    }

    [Fact]
    public async Task LoadAsync_WithCorruptedJson_ShouldReturnEmptyAndNotThrow()
    {
        var goal    = "Corrupted";
        var badPath = GetFilePath(goal);
        Directory.CreateDirectory(Path.GetDirectoryName(badPath)!);
        await File.WriteAllTextAsync(badPath, "{ definitely: not valid json [");

        var repo   = MakeRepo();
        var loaded = await repo.LoadAsync(goal);

        Assert.Empty(loaded);
    }

    [Fact]
    public async Task ConcurrentSave_ShouldLeaveValidFinalFile()
    {
        var repo = MakeRepo();
        var goal = "Concurrent";

        async Task Save(int i)
            => await repo.SaveAsync(goal, new List<ProgressEntry> { new() { GoalType = goal, Value = i, Unit = "u" } });

        var tasks = Enumerable.Range(0, 10).Select(Save);
        await Task.WhenAll(tasks);

        var loaded = await repo.LoadAsync(goal);

        // File should be valid JSON with exactly one entry (last write wins).
        Assert.Single(loaded);
        Assert.InRange(loaded[0].Value, 0, 9);
    }
}
