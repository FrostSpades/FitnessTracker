// FitnessTracker/Services/IProgressService.cs
using FitnessTracker.Models;


namespace FitnessTracker.Services;


public interface IProgressService
{
    Task<ProgressEntry> SaveProgressAsync(ProgressEntry progress);
    Task<IEnumerable<ProgressEntry>> GetProgressHistoryAsync(string goalType);
    Task<ProgressEntry?> GetLatestProgressAsync(string goalType);
}