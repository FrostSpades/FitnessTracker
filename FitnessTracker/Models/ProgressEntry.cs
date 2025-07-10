// FitnessTracker/Models/ProgressEntry.cs
using System.Text.Json.Serialization;

namespace FitnessTracker.Models;

public class ProgressEntry
{
    public Guid     Id        { get; init; }
    public DateTime Timestamp { get; init; }

    public string  GoalType { get; init; } = string.Empty;   // stored as text for JSON
    public float   Value    { get; init; }
    public string  Unit     { get; init; } = string.Empty;
    public string? Notes    { get; init; }

    // Fully qualify the enum to avoid the name-clash with the property above
    [JsonIgnore] public bool IsRunning =>
        GoalType == FitnessTracker.Models.GoalType.Running.ToString();

    [JsonIgnore] public bool IsWater =>
        GoalType == FitnessTracker.Models.GoalType.Water.ToString();

    public static ProgressEntry FromRunningDistance(RunningDistance distance, string? notes = null) =>
        new()
        {
            Id        = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            GoalType  = FitnessTracker.Models.GoalType.Running.ToString(),
            Value     = distance.Value,
            Unit      = distance.Unit.ToString(),
            Notes     = notes
        };

    public static ProgressEntry FromWaterContent(WaterContent water, string? notes = null) =>
        new()
        {
            Id        = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            GoalType  = FitnessTracker.Models.GoalType.Water.ToString(),
            Value     = water.Value,
            Unit      = water.Unit.ToString(),
            Notes     = notes
        };

    public RunningDistance ToRunningDistance() =>
        GoalType != FitnessTracker.Models.GoalType.Running.ToString()
            ? throw new InvalidOperationException("Cannot convert non-running progress to RunningDistance")
            : new RunningDistance
              {
                  Value = Value,
                  Unit  = Enum.Parse<DistanceUnit>(Unit)
              };

    public WaterContent ToWaterContent() =>
        GoalType != FitnessTracker.Models.GoalType.Water.ToString()
            ? throw new InvalidOperationException("Cannot convert non-water progress to WaterContent")
            : new WaterContent
              {
                  Value = Value,
                  Unit  = Enum.Parse<WaterUnit>(Unit)
              };
}
