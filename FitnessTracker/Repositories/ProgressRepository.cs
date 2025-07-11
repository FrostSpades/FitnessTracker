using FitnessTracker.Data;
using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Repositories;

/// <summary>
/// Interface for loading and saving progress entries tied to goals.
/// </summary>
public interface IProgressRepository
{
    Task<List<ProgressEntry>> LoadAsync(string goalType);
    Task SaveAsync(string goalType, List<ProgressEntry> entries);
}

/// <summary>
/// Repository for reading/writing progress data associated with specific goal types.
/// </summary>
public class ProgressRepository : IProgressRepository
{
    private readonly FitnessTrackerDbContext _context;
    private readonly ILogger<ProgressRepository>? _logger;

    public ProgressRepository(FitnessTrackerDbContext context, ILogger<ProgressRepository>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    // Loads all progress entries for a specific goal type, sorted by timestamp (descending).
    public async Task<List<ProgressEntry>> LoadAsync(string goalType)
    {
        return await _context.ProgressEntries
            .Where(p => p.GoalType == goalType)
            .OrderByDescending(p => p.Timestamp)
            .AsNoTracking()
            .ToListAsync();
    }

    // Saves a new list of progress entries, replacing the old ones for the same goal type.
    public async Task SaveAsync(string goalType, List<ProgressEntry> entries)
    {
        var oldEntries = _context.ProgressEntries.Where(p => p.GoalType == goalType);
        _context.ProgressEntries.RemoveRange(oldEntries);
        await _context.SaveChangesAsync();

        await _context.ProgressEntries.AddRangeAsync(entries);
        await _context.SaveChangesAsync();

        _logger?.LogInformation("Progress data saved for {GoalType}, {Count} entries", goalType, entries.Count);
    }
}