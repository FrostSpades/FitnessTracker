// FitnessTracker/Repositories/IProgressRepository.cs
using FitnessTracker.Models;

namespace FitnessTracker.Repositories;

public interface IProgressRepository
{
    Task<List<ProgressEntry>> LoadAsync(string goalType);
    Task SaveAsync(string goalType, List<ProgressEntry> entries);
}