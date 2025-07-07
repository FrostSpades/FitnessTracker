// FitnessTracker/Repositories/GoalRepository.cs

using System.IO;
using System.Text.Json;
using FitnessTracker.Models;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Repositories;

/// <summary>
/// Responsible for on-disk persistence of goals.
/// </summary>
public interface IGoalRepository : IDisposable
{
    Task LoadAsync();
    Task<Goal>  AddGoalAsync(Goal goal);
    Task<bool>  DeleteGoalAsync(int goalId);
    Task<List<Goal>> GetAllGoalsAsync();
    Task SaveAllGoalsAsync(IEnumerable<Goal> goals);
}

public sealed class GoalRepository : IGoalRepository
{
    private readonly List<Goal>      _goals     = new();
    private readonly SemaphoreSlim   _sem       = new(1, 1);
    private readonly ILogger<GoalRepository>? _log;

    private readonly string _dir  = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData");
    private readonly string _file;

    private int _nextId = 1;

    public GoalRepository(ILogger<GoalRepository>? log = null)
    {
        _log  = log;
        _file = Path.Combine(_dir, "goals.json");
        Directory.CreateDirectory(_dir);
    }

    /* ---------- Public API ---------------------------------------------- */
    public async Task LoadAsync()
    {
        await _sem.WaitAsync();
        try
        {
            if (!File.Exists(_file)) return;

            await using var s = File.OpenRead(_file);
            var loaded = await JsonSerializer.DeserializeAsync<List<Goal>>(s);
            if (loaded is null) return;

            _goals.Clear();
            _goals.AddRange(loaded.Select(g => g.Clone()));
            _nextId = _goals.Any() ? _goals.Max(g => g.Id) + 1 : 1;
        }
        finally { _sem.Release(); }
    }

    public async Task<Goal> AddGoalAsync(Goal goal)
    {
        await _sem.WaitAsync();
        try
        {
            var toStore = goal.Clone();
            toStore.Id        = _nextId++;
            if (toStore.CreatedAt == default)
                toStore.CreatedAt = DateTime.UtcNow;

            _goals.Add(toStore);
            await PersistAsync();
            return toStore.Clone();
        }
        finally { _sem.Release(); }
    }

    public async Task<List<Goal>> GetAllGoalsAsync()
    {
        await _sem.WaitAsync();
        try   { return _goals.Select(g => g.Clone()).ToList(); }
        finally { _sem.Release(); }
    }

    public async Task<bool> DeleteGoalAsync(int id)
    {
        await _sem.WaitAsync();
        try
        {
            var g = _goals.FirstOrDefault(x => x.Id == id);
            if (g is null) return false;
            _goals.Remove(g);
            await PersistAsync();
            return true;
        }
        finally { _sem.Release(); }
    }

    public async Task SaveAllGoalsAsync(IEnumerable<Goal> goals)
    {
        if (goals == null) throw new ArgumentNullException(nameof(goals));

        await _sem.WaitAsync();
        try
        {
            _goals.Clear();
            _goals.AddRange(goals.Select(g => g.Clone()));
            _nextId = _goals.Any() ? _goals.Max(g => g.Id) + 1 : 1;
            await PersistAsync();
        }
        finally { _sem.Release(); }
    }

    /* ---------- Helpers -------------------------------------------------- */
    private async Task PersistAsync()
    {
        var json = JsonSerializer.Serialize(_goals, new JsonSerializerOptions { WriteIndented = true });
        var tmp  = Path.Combine(_dir, $"g_{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllTextAsync(tmp, json);
            File.Move(tmp, _file, overwrite: true);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }

    public void Dispose() => _sem.Dispose();
}
