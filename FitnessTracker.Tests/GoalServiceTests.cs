using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using FitnessTracker.Services;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace FitnessTracker.Tests
{
    public sealed class GoalServiceTests : IDisposable
    {
        private static readonly string SaveDir  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData");
        private static readonly string SavePath = Path.Combine(SaveDir, "goals.json");

        public GoalServiceTests() => Clean();
        public void Dispose()     => Clean();

        private static void Clean()
        {
            if (Directory.Exists(SaveDir))
                Directory.Delete(SaveDir, true);
        }

        private static GoalService MakeSvc() => new(new GoalRepository());

        /* -------- File location ------------------------------------------ */
        [Fact]
        public async Task SaveGoalAsync_ShouldCreateFile_InSaveDataFolder()
        {
            var svc  = MakeSvc();
            await svc.SaveGoalAsync(
                Goal.FromRunningDistance(new RunningDistance { Value = 10, Unit = DistanceUnit.Kilometers }));

            Assert.True(File.Exists(SavePath));
        }

        /* -------- Round-trip --------------------------------------------- */
        [Fact]
        public async Task SaveThenLoad_ShouldYieldSameRunningGoal()
        {
            var dist = new RunningDistance { Value = 5, Unit = DistanceUnit.Miles };
            var svc1 = MakeSvc();
            var saved = await svc1.SaveGoalAsync(Goal.FromRunningDistance(dist));

            var svc2 = MakeSvc();
            await svc2.LoadGoalsAsync();
            var loaded = await svc2.GetActiveGoalByTypeAsync("Running");

            Assert.NotNull(loaded);
            Assert.Equal(saved.Id, loaded!.Id);

            var back = loaded.ToRunningDistance();
            Assert.Equal(dist.Value, back.Value);
            Assert.Equal(dist.Unit,  back.Unit);
        }

        // De-activation rule
        [Fact]
        public async Task SavingSecondGoalOfSameType_DeactivatesFirst()
        {
            var svc = MakeSvc();

            var first  = await svc.SaveGoalAsync(
                Goal.FromWaterContent(new WaterContent { Value = 8, Unit = WaterUnit.Cups }));
            var second = await svc.SaveGoalAsync(
                Goal.FromWaterContent(new WaterContent { Value = 2, Unit = WaterUnit.Liters }));

            // ---- fetch a fresh snapshot ----------------------------------------
            var all    = await svc.GetAllGoalsAsync();
            var firstFromRepo  = all.Single(g => g.Id == first.Id);
            var secondFromRepo = all.Single(g => g.Id == second.Id);

            Assert.False(firstFromRepo.IsActive);
            Assert.True (secondFromRepo.IsActive);

            var active = await svc.GetActiveGoalByTypeAsync("Water");
            Assert.Equal(second.Id, active!.Id);
        }

        /* -------- Delete -------------------------------------------------- */
        [Fact]
        public async Task DeleteGoalAsync_ShouldRemoveGoal_AndReturnTrue()
        {
            var svc  = MakeSvc();
            var g    = await svc.SaveGoalAsync(
                           Goal.FromRunningDistance(new RunningDistance { Value = 3, Unit = DistanceUnit.Kilometers }));

            var ok   = await svc.DeleteGoalAsync(g.Id);
            var all  = await svc.GetAllGoalsAsync();

            Assert.True(ok);
            Assert.Empty(all);
        }

        /* -------- Incremental Ids ---------------------------------------- */
        [Fact]
        public async Task SaveGoalAsync_ShouldAssignIncrementalIds()
        {
            var svc = MakeSvc();
            var g1  = await svc.SaveGoalAsync(
                          Goal.FromWaterContent(new WaterContent { Value = 1, Unit = WaterUnit.Liters }));
            var g2  = await svc.SaveGoalAsync(
                          Goal.FromWaterContent(new WaterContent { Value = 2, Unit = WaterUnit.Liters }));

            Assert.Equal(1, g1.Id);
            Assert.Equal(2, g2.Id);
        }
    }
}
