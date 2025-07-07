using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using Xunit;

namespace FitnessTracker.Tests
{
    public sealed class GoalRepositoryTests : IDisposable
    {
        private static readonly string SaveDir  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData");
        private static readonly string SavePath = Path.Combine(SaveDir, "goals.json");

        public GoalRepositoryTests() => Clean();
        public void Dispose()        => Clean();

        private static void Clean()
        {
            if (Directory.Exists(SaveDir))
                Directory.Delete(SaveDir, true);
        }

        /* -------- File is created ---------------------------------------- */
        [Fact]
        public async Task AddGoalAsync_ShouldCreateGoalsJsonFile()
        {
            var repo = new GoalRepository();
            await repo.AddGoalAsync(
                Goal.FromRunningDistance(new RunningDistance { Value = 1, Unit = DistanceUnit.Kilometers }));

            Assert.True(File.Exists(SavePath));
        }

        /* -------- Round-trip --------------------------------------------- */
        [Fact]
        public async Task SaveThenLoad_ShouldRoundTripGoal()
        {
            var original = Goal.FromWaterContent(
                               new WaterContent { Value = 8, Unit = WaterUnit.Ounces });

            var repo1 = new GoalRepository();
            var saved = await repo1.AddGoalAsync(original);

            var repo2 = new GoalRepository();
            await repo2.LoadAsync();
            var all   = await repo2.GetAllGoalsAsync();
            var loaded = all.Single();

            Assert.Equal(saved.Id, loaded.Id);
            Assert.Equal(original.Value, loaded.Value);
            Assert.Equal(original.Unit,  loaded.Unit);
        }

        /* -------- Delete -------------------------------------------------- */
        [Fact]
        public async Task DeleteGoalAsync_ShouldRemoveGoal()
        {
            var repo = new GoalRepository();
            var g    = await repo.AddGoalAsync(
                           Goal.FromRunningDistance(new RunningDistance { Value = 2, Unit = DistanceUnit.Miles }));

            var ok  = await repo.DeleteGoalAsync(g.Id);
            var all = await repo.GetAllGoalsAsync();

            Assert.True(ok);
            Assert.Empty(all);
        }

        /* -------- Incremental IDs across restarts ------------------------ */
        [Fact]
        public async Task AddGoalAsync_ShouldIncrementIdsAcrossReloads()
        {
            var r1 = new GoalRepository();
            var g1 = await r1.AddGoalAsync(
                         Goal.FromWaterContent(new WaterContent { Value = 1, Unit = WaterUnit.Liters }));
            await r1.AddGoalAsync(
                Goal.FromWaterContent(new WaterContent { Value = 2, Unit = WaterUnit.Liters }));

            var r2 = new GoalRepository();
            await r2.LoadAsync();
            var g3 = await r2.AddGoalAsync(
                         Goal.FromWaterContent(new WaterContent { Value = 3, Unit = WaterUnit.Liters }));

            Assert.Equal(1, g1.Id);
            Assert.Equal(3, g3.Id);
        }

        /* -------- No stray *.tmp files ----------------------------------- */
        [Fact]
        public async Task PersistAsync_ShouldNotLeaveTempFiles()
        {
            var repo = new GoalRepository();
            await repo.AddGoalAsync(
                Goal.FromRunningDistance(new RunningDistance { Value = 7, Unit = DistanceUnit.Kilometers }));

            var hasTmp = Directory.EnumerateFiles(SaveDir, "*.tmp").Any();
            Assert.False(hasTmp);
        }

        /* -------- Thread-safety smoke test ------------------------------- */
        [Fact]
        public async Task AddGoalAsync_ShouldBeThreadSafe()
        {
            var repo = new GoalRepository();
            var tasks = Enumerable.Range(0, 10).Select(i =>
                repo.AddGoalAsync(
                    Goal.FromWaterContent(new WaterContent { Value = i + 1, Unit = WaterUnit.Liters })));

            await Task.WhenAll(tasks);

            var all = await repo.GetAllGoalsAsync();
            Assert.Equal(10, all.Count);
            Assert.Equal(10, all.Select(g => g.Id).Distinct().Count());
        }
    }
}
