// FitnessTracker/Repositories/ProgressRepository.cs
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using FitnessTracker.Models;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Repositories;

public class ProgressRepository : IProgressRepository
{
    private readonly string _saveDirectory;
    private readonly ILogger<ProgressRepository>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly object _fileLock = new();

    private readonly ConcurrentDictionary<string, List<ProgressEntry>> _cache = new();

    public ProgressRepository(ILogger<ProgressRepository>? logger = null)
    {
        _logger = logger;
        _saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData");
        Directory.CreateDirectory(_saveDirectory);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<List<ProgressEntry>> LoadAsync(string goalType)
    {
        if (_cache.TryGetValue(goalType, out var cached))
            return new List<ProgressEntry>(cached);

        var path = GetFilePath(goalType);
        if (!File.Exists(path))
        {
            _cache[goalType] = new(); 
            return new();
        }

        try
        {
            string json;
            lock (_fileLock)                  
            {
                json = File.ReadAllText(path);
            }

            var entries = JsonSerializer.Deserialize<List<ProgressEntry>>(json, _jsonOptions) ?? new();
            _cache[goalType] = entries;       
            return new List<ProgressEntry>(entries);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to read or deserialize progress data for {GoalType}", goalType);
            return new();
        }
    }

    public async Task SaveAsync(string goalType, List<ProgressEntry> entries)
    {
        var path     = GetFilePath(goalType);
        var tempPath = path + ".tmp";

        try
        {
            var json = JsonSerializer.Serialize(entries, _jsonOptions);

            lock (_fileLock)            
            {
                File.WriteAllText(tempPath, json);
                File.Copy(tempPath, path, overwrite: true);
                File.Delete(tempPath);
            }
            
            _cache[goalType] = entries;

            _logger?.LogInformation("Progress data saved for {GoalType}", goalType);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save progress data for {GoalType}", goalType);
            throw;
        }
    }

    private string GetFilePath(string goalType) =>
        Path.Combine(_saveDirectory, $"{goalType.ToLowerInvariant()}_progress.json");
}
