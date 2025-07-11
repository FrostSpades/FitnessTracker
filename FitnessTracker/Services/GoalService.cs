using FitnessTracker.Models;
using FitnessTracker.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessTracker.Services;

public interface IGoalService
{
    Task<Goal>  SaveGoalAsync(Goal goal);
    Task<Goal>  SaveGoalAsync(GoalType type, float value, DistanceUnit distUnit, WaterUnit waterUnit);
    Task<List<Goal>> GetAllGoalsAsync();
    Task<Goal?> GetActiveGoalByTypeAsync(GoalType type);
    Task<bool>  DeleteGoalAsync(int goalId);
    float AdjustDistanceValue(GoalType type, float value, DistanceUnit from, DistanceUnit to);
    float AdjustWaterValue   (GoalType type, float value, WaterUnit    from, WaterUnit    to);

    float ConvertDistance(float value, DistanceUnit from, DistanceUnit to);
    float ConvertWater   (float value, WaterUnit    from, WaterUnit    to);
}

public sealed class GoalService : IGoalService, IDisposable
{
    private readonly IGoalRepository _repo;

    public GoalService(IGoalRepository repo) =>
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));

    public float ConvertDistance(float v, DistanceUnit from, DistanceUnit to) =>
        from == to ? v
                   : new RunningDistance { Unit = from, Value = v }.ConvertTo(to);

    public float ConvertWater(float v, WaterUnit from, WaterUnit to) =>
        from == to ? v
                   : new WaterContent { Unit = from, Value = v }.ConvertTo(to);

    public Task<List<Goal>> GetAllGoalsAsync() => _repo.GetAllGoalsAsync();
    public Task<bool> DeleteGoalAsync(int id) => _repo.DeleteGoalAsync(id);

    public async Task<Goal> SaveGoalAsync(
        GoalType type, float value, DistanceUnit distUnit, WaterUnit waterUnit)
    {
        if (value <= 0 || float.IsNaN(value) || float.IsInfinity(value))
            throw new ArgumentException(nameof(value));

        var goal = type switch
        {
            GoalType.Running => Goal.FromRunningDistance(
                new RunningDistance { Unit = distUnit, Value = value }),
            GoalType.Water => Goal.FromWaterContent(
                new WaterContent { Unit = waterUnit, Value = value }),
            _ => throw new ArgumentException(nameof(type))
        };

        return await SaveGoalAsync(goal);
    }

    public async Task<Goal> SaveGoalAsync(Goal goal)
    {
        if (goal == null) throw new ArgumentNullException(nameof(goal));

        var all = await _repo.GetAllGoalsAsync();
        var modified = false;

        foreach (var g in all.Where(g => g.Type == goal.Type && g.IsActive))
        {
            g.Deactivate();
            modified = true;
        }

        if (modified)
            await _repo.SaveAllGoalsAsync(all);

        goal.MarkAsActive();
        return await _repo.AddGoalAsync(goal);
    }

    public float AdjustDistanceValue(GoalType type, float value, DistanceUnit from, DistanceUnit to)
    {
        if (type != GoalType.Running || value <= 0 || from == to) return value;
        return ConvertDistance(value, from, to);
    }

    public float AdjustWaterValue(GoalType type, float value, WaterUnit from, WaterUnit to)
    {
        if (type != GoalType.Water || value <= 0 || from == to) return value;
        return ConvertWater(value, from, to);
    }

    public async Task<Goal?> GetActiveGoalByTypeAsync(GoalType type) =>
        (await _repo.GetAllGoalsAsync()).FirstOrDefault(g => g.Type == type && g.IsActive);

    public void Dispose() => (_repo as IDisposable)?.Dispose();
}

