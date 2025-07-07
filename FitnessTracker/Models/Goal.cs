// FitnessTracker/Models/Goal.cs
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FitnessTracker.Models;

public class Goal
{
    public int Id { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Range(0.01, float.MaxValue, ErrorMessage = "Value must be greater than 0")]
    public float Value { get; set; }

    [Required]
    public string Unit { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    [JsonIgnore] public bool IsRunningGoal => Type == "Running";
    [JsonIgnore] public bool IsWaterGoal   => Type == "Water";

    public RunningDistance ToRunningDistance()
    {
        if (!IsRunningGoal) throw new InvalidOperationException("Goal is not a running goal");
        if (!Enum.TryParse<DistanceUnit>(Unit, out var unit))
            throw new InvalidOperationException($"Invalid distance unit: {Unit}");

        return new RunningDistance { Value = Value, Unit = unit };
    }

    public WaterContent ToWaterContent()
    {
        if (!IsWaterGoal) throw new InvalidOperationException("Goal is not a water goal");
        if (!Enum.TryParse<WaterUnit>(Unit, out var unit))
            throw new InvalidOperationException($"Invalid water unit: {Unit}");

        return new WaterContent { Value = Value, Unit = unit };
    }

    public static Goal FromRunningDistance(RunningDistance running)
    {
        if (running == null) throw new ArgumentNullException(nameof(running));
        if (running.Value <= 0) throw new ArgumentException("Running distance value must be greater than 0", nameof(running));

        return new Goal
        {
            Type = "Running",
            Value = running.Value,
            Unit  = running.Unit.ToString()
        };
    }

    public static Goal FromWaterContent(WaterContent water)
    {
        if (water == null) throw new ArgumentNullException(nameof(water));
        if (water.Value <= 0) throw new ArgumentException("Water content value must be greater than 0", nameof(water));

        return new Goal
        {
            Type = "Water",
            Value = water.Value,
            Unit  = water.Unit.ToString()
        };
    }

    /* --- Deep-copy helper ------------------------------------------- */
    public Goal Clone() => new()
    {
        Id        = Id,
        Type      = Type,
        Value     = Value,
        Unit      = Unit,
        CreatedAt = CreatedAt,
        IsActive  = IsActive
    };
}
