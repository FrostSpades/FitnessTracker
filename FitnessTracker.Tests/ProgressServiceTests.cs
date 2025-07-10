using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using FitnessTracker.Services;
using Xunit;

namespace FitnessTracker.Tests
{
    public sealed class ProgressServiceTests : IDisposable
    {
        private static readonly string SaveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData");

        public ProgressServiceTests() => Clean();
        public void Dispose() => Clean();

        private static void Clean()
        {
            if (Directory.Exists(SaveDir))
                Directory.Delete(SaveDir, recursive: true);
        }

        private static ProgressService MakeSvc() =>
            new(new ProgressRepository());

        private static string GetFilePath(string goalType) =>
            Path.Combine(SaveDir, $"{goalType.ToLowerInvariant()}_progress.json");

        [Fact]
        public async Task SaveProgressAsync_ShouldCreateFile()
        {
            var svc = MakeSvc();
            await svc.SaveProgressAsync(GoalType.Running, 5f, DistanceUnit.Miles, WaterUnit.Ounces, null);

            Assert.True(File.Exists(GetFilePath("Running")));
        }

        [Fact]
        public async Task SaveProgressAsync_ShouldAssignIdAndTimestamp()
        {
            var svc = MakeSvc();

            var result = await svc.SaveProgressAsync(GoalType.Water, 2f, DistanceUnit.Miles, WaterUnit.Ounces, null);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.True(result.Timestamp > DateTime.MinValue);
        }

        [Fact]
        public async Task GetProgressHistoryAsync_ShouldReturnDescendingOrder()
        {
            var svc = MakeSvc();
            var type = "Running";

            await svc.SaveProgressAsync(GoalType.Running, 1f, DistanceUnit.Miles, WaterUnit.Ounces, null);
            await Task.Delay(10);
            await svc.SaveProgressAsync(GoalType.Running, 2f, DistanceUnit.Miles, WaterUnit.Ounces, null);

            var history = (await svc.GetProgressHistoryAsync(type)).ToList();

            Assert.Equal(2, history.Count);
            Assert.True(history[0].Timestamp >= history[1].Timestamp);
        }

        [Fact]
        public async Task GetProgressHistoryAsync_WithNoFile_ShouldReturnEmpty()
        {
            var svc = MakeSvc();
            var result = await svc.GetProgressHistoryAsync("NonExistentGoal");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetLatestProgressAsync_ShouldReturnMostRecent()
        {
            var svc = MakeSvc();
            var type = "Water";

            var older = await svc.SaveProgressAsync(GoalType.Water, 1f, DistanceUnit.Miles, WaterUnit.Ounces, null);
            await Task.Delay(10);
            var newer = await svc.SaveProgressAsync(GoalType.Water, 2f, DistanceUnit.Miles, WaterUnit.Ounces, null);

            var latest = await svc.GetLatestProgressAsync(type);

            Assert.NotNull(latest);
            Assert.Equal(newer.Id, latest!.Id);
        }
    }
}

