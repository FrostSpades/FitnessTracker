using System.IO;
using System.Text.Json;
using FitnessTracker.Models;

namespace FitnessTracker.Services;

public interface IGoalService
{
    Task<Goal> SaveGoalAsync(Goal goal);
    Task<List<Goal>> GetAllGoalsAsync();
    Task<Goal?> GetActiveGoalByTypeAsync(string type);
    Task<bool> DeleteGoalAsync(int goalId);
    Task LoadGoalsAsync();
}

public class GoalService : IGoalService
{
    private readonly List<Goal> _goals = new();
    private readonly string _saveDirectory;
    private readonly string _saveFilePath;
    private int _nextId = 1;

    public GoalService()
    {
        _saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FitnessTracker", "SaveData");
        _saveFilePath = Path.Combine(_saveDirectory, "goals.json");
        
        // Ensure directory exists
        Directory.CreateDirectory(_saveDirectory);
    }

    public async Task<Goal> SaveGoalAsync(Goal goal)
    {
        // Deactivate existing goals of the same type
        var existingGoals = _goals.Where(g => g.Type == goal.Type).ToList();
        foreach (var existingGoal in existingGoals)
        {
            existingGoal.IsActive = false;
        }

        // Set ID and add new goal
        goal.Id = _nextId++;
        goal.IsActive = true;
        goal.CreatedAt = DateTime.Now;
        
        _goals.Add(goal);

        // Save to JSON
        await SaveToJsonAsync();

        return goal;
    }

    public async Task<List<Goal>> GetAllGoalsAsync()
    {
        return await Task.FromResult(_goals.ToList());
    }

    public async Task<Goal?> GetActiveGoalByTypeAsync(string type)
    {
        return await Task.FromResult(_goals.FirstOrDefault(g => g.Type == type && g.IsActive));
    }

    public async Task<bool> DeleteGoalAsync(int goalId)
    {
        var goal = _goals.FirstOrDefault(g => g.Id == goalId);
        if (goal == null) return false;

        _goals.Remove(goal);
        await SaveToJsonAsync();
        return true;
    }

    public async Task LoadGoalsAsync()
    {
        try
        {
            if (!File.Exists(_saveFilePath))
                return;

            var json = await File.ReadAllTextAsync(_saveFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return;

            var goals = JsonSerializer.Deserialize<List<Goal>>(json);
            if (goals != null)
            {
                _goals.Clear();
                _goals.AddRange(goals);
                
                // Update next ID
                _nextId = _goals.Any() ? _goals.Max(g => g.Id) + 1 : 1;
            }
        }
        catch (Exception ex)
        {
            // Log error or handle as appropriate for your app
            System.Diagnostics.Debug.WriteLine($"Error loading goals: {ex.Message}");
        }
    }

    private async Task SaveToJsonAsync()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            var json = JsonSerializer.Serialize(_goals, options);
            await File.WriteAllTextAsync(_saveFilePath, json);
        }
        catch (Exception ex)
        {
            // Log error or handle as appropriate for your app
            System.Diagnostics.Debug.WriteLine($"Error saving goals: {ex.Message}");
        }
    }
}