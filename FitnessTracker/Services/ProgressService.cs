// FitnessTracker/Services/ProgressService.cs
using FitnessTracker.Models;
using FitnessTracker.Repositories;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Services
{
    public interface IProgressService
    {
        Task<ProgressEntry> SaveProgressAsync(GoalType type, float value, DistanceUnit distanceUnit, WaterUnit waterUnit, string? notes);
        Task<IEnumerable<ProgressEntry>> GetProgressHistoryAsync(string goalType);
        Task<ProgressEntry?> GetLatestProgressAsync(string goalType);
    }

    public class ProgressService : IProgressService
    {
        private readonly IProgressRepository _repository;
        private readonly ILogger<ProgressService>? _logger;

        public ProgressService(IProgressRepository repository, ILogger<ProgressService>? logger = null)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProgressEntry> SaveProgressAsync(GoalType type, float value, DistanceUnit distanceUnit, WaterUnit waterUnit, string? notes)
        {
            if (value <= 0 || float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentException(nameof(value));

            ProgressEntry progress = type switch
            {
                GoalType.Running => ProgressEntry.FromRunningDistance(new RunningDistance { Unit = distanceUnit, Value = value }, notes),
                GoalType.Water => ProgressEntry.FromWaterContent(new WaterContent { Unit = waterUnit, Value = value }, notes),
                _ => throw new ArgumentException(nameof(type))
            };

            var entries = await _repository.LoadAsync(type.ToString());
            entries.Add(progress);
            await _repository.SaveAsync(type.ToString(), entries);

            _logger?.LogInformation("Saved progress entry: {GoalType} - {Value}", type, value);
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
}