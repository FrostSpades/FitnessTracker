// FitnessTracker/Services/GoalService.cs
using FitnessTracker.Models;
using FitnessTracker.Repositories;

namespace FitnessTracker.Services;

/// <summary>
/// Orchestrates goal business rules (single active goal per type, etc.).
/// Pure persistence lives in <see cref="IGoalRepository"/>.
/// </summary>
public interface IGoalService
{
    Task LoadGoalsAsync();
    Task<Goal>  SaveGoalAsync(Goal goal);
    Task<List<Goal>> GetAllGoalsAsync();
    Task<Goal?>     GetActiveGoalByTypeAsync(string type);
    Task<bool>      DeleteGoalAsync(int goalId);
}

public sealed class GoalService : IGoalService, IDisposable
{
    private readonly IGoalRepository _repo;

    public GoalService(IGoalRepository repo)
        => _repo = repo ?? throw new ArgumentNullException(nameof(repo));

    /* ---------- Persistence passthroughs --------------------------------- */
    public Task LoadGoalsAsync()          => _repo.LoadAsync();
    public Task<List<Goal>> GetAllGoalsAsync() => _repo.GetAllGoalsAsync();
    public Task<bool> DeleteGoalAsync(int id)  => _repo.DeleteGoalAsync(id);

    /* ---------- Business logic ------------------------------------------- */
    public async Task<Goal> SaveGoalAsync(Goal goal)
    {
        if (goal == null) throw new ArgumentNullException(nameof(goal));

        // 1) De-activate existing active goals of the *same* type
        var all = await _repo.GetAllGoalsAsync();
        var modified = false;
        foreach (var g in all.Where(g => g.Type == goal.Type && g.IsActive))
        {
            g.IsActive = false;
            modified   = true;
        }
        if (modified)
            await _repo.SaveAllGoalsAsync(all);   // persist status flips

        // 2) Prepare & persist the new goal
        goal.IsActive  = true;
        goal.CreatedAt = DateTime.UtcNow;

        return await _repo.AddGoalAsync(goal);
    }

    public async Task<Goal?> GetActiveGoalByTypeAsync(string type)
    {
        if (string.IsNullOrWhiteSpace(type)) return null;
        var all = await _repo.GetAllGoalsAsync();
        return all.FirstOrDefault(g => g.Type == type && g.IsActive);
    }

    public void Dispose() => (_repo as IDisposable)?.Dispose();
}
