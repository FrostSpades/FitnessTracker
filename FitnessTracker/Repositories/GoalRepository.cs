using FitnessTracker.Data;
using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Repositories;

/// <summary>
/// Interface for managing Goal data in the database.
/// </summary>
public interface IGoalRepository
{
    Task<Goal?> GetGoalAsync(int id);
    Task<List<Goal>> GetGoalsByTypeAsync(GoalType type);
    Task<List<Goal>> GetAllGoalsAsync();
    Task<Goal> AddGoalAsync(Goal goal);
    Task<bool> DeleteGoalAsync(int id);
    Task SaveAllGoalsAsync(List<Goal> goals);
}

/// <summary>
/// Repository implementation for handling Goal data operations.
/// </summary>
public class GoalRepository : IGoalRepository
{
    private readonly FitnessTrackerDbContext _context;

    public GoalRepository(FitnessTrackerDbContext context)
    {
        _context = context;
    }

    // Retrieves a goal by its ID.
    public async Task<Goal?> GetGoalAsync(int id)
    {
        return await _context.Goals
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    // Retrieves all goals of a specific type.
    public async Task<List<Goal>> GetGoalsByTypeAsync(GoalType type)
    {
        return await _context.Goals
            .Where(g => g.Type == type)
            .AsNoTracking()
            .ToListAsync();
    }

    // Retrieves all goals.
    public async Task<List<Goal>> GetAllGoalsAsync()
    {
        return await _context.Goals
            .AsNoTracking()
            .ToListAsync();
    }

    // Adds a new goal and saves changes.
    public async Task<Goal> AddGoalAsync(Goal goal)
    {
        await _context.Goals.AddAsync(goal);
        await _context.SaveChangesAsync();
        return goal;
    }

    // Deletes a goal by ID.
    public async Task<bool> DeleteGoalAsync(int id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal is null)
            return false;

        _context.Goals.Remove(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    // Replaces all goals of specific types with new entries.
    public async Task SaveAllGoalsAsync(List<Goal> goals)
    {
        var types = goals.Select(g => g.Type).Distinct().ToList();
        var existing = _context.Goals.Where(g => types.Contains(g.Type));
        _context.Goals.RemoveRange(existing);
        await _context.SaveChangesAsync();

        await _context.Goals.AddRangeAsync(goals);
        await _context.SaveChangesAsync();
    }
}
