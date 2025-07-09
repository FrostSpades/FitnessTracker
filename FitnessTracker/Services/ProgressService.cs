// FitnessTracker/Services/ProgressService.cs
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Services;

public class ProgressService : IProgressService
{
    private readonly IProgressRepository _repository;
    private readonly ILogger<ProgressService>? _logger;

    public ProgressService(IProgressRepository repository, ILogger<ProgressService>? logger = null)
    {
        _repository = repository;
        _logger     = logger;
    }

    public async Task<ProgressEntry> SaveProgressAsync(ProgressEntry progress)
    {
        if (progress is null) throw new ArgumentNullException(nameof(progress));

        progress.Id        = Guid.NewGuid();
        progress.Timestamp = DateTime.UtcNow;

        // repository now returns a cached list, so this is cheap
        var entries = await _repository.LoadAsync(progress.GoalType);
        entries.Add(progress);
        await _repository.SaveAsync(progress.GoalType, entries);

        _logger?.LogInformation("Saved progress entry: {GoalType} - {Value}", progress.GoalType, progress.Value);
        return progress;
    }

    public async Task<IEnumerable<ProgressEntry>> GetProgressHistoryAsync(string goalType)
    {
        var entries = await _repository.LoadAsync(goalType);
        return entries.OrderByDescending(e => e.Timestamp);
    }

    public async Task<ProgressEntry?> GetLatestProgressAsync(string goalType) =>
        (await GetProgressHistoryAsync(goalType)).FirstOrDefault();
}