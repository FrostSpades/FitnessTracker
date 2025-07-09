// FitnessTracker/Models/ProgressEntry.cs
namespace FitnessTracker.Models;


public class ProgressEntry
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string GoalType { get; set; } = string.Empty;
    public float Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Notes { get; set; }


    public static ProgressEntry FromRunningDistance(RunningDistance distance, string? notes = null)
    {
        return new ProgressEntry
        {
            GoalType = "Running",
            Value = distance.Value,
            Unit = distance.Unit.ToString(),
            Notes = notes
        };
    }


    public static ProgressEntry FromWaterContent(WaterContent water, string? notes = null)
    {
        return new ProgressEntry
        {
            GoalType = "Water",
            Value = water.Value,
            Unit = water.Unit.ToString(),
            Notes = notes
        };
    }


    public RunningDistance ToRunningDistance()
    {
        if (GoalType != "Running")
            throw new InvalidOperationException("Cannot convert non-running progress to RunningDistance");


        return new RunningDistance
        {
            Value = Value,
            Unit = Enum.Parse<DistanceUnit>(Unit)
        };
    }


    public WaterContent ToWaterContent()
    {
        if (GoalType != "Water")
            throw new InvalidOperationException("Cannot convert non-water progress to WaterContent");


        return new WaterContent
        {
            Value = Value,
            Unit = Enum.Parse<WaterUnit>(Unit)
        };
    }
}