using System.Text.Json.Serialization;

namespace FitnessTracker.Models;

public class Goal
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public float Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public bool IsRunningGoal => Type == "Running";
    
    [JsonIgnore]
    public bool IsWaterGoal => Type == "Water";

    public RunningDistance ToRunningDistance()
    {
        if (!IsRunningGoal) throw new InvalidOperationException("Goal is not a running goal");
        
        return new RunningDistance
        {
            Value = Value,
            Unit = Enum.Parse<DistanceUnit>(Unit)
        };
    }

    public WaterContent ToWaterContent()
    {
        if (!IsWaterGoal) throw new InvalidOperationException("Goal is not a water goal");
        
        return new WaterContent
        {
            Value = Value,
            Unit = Enum.Parse<WaterUnit>(Unit)
        };
    }

    public static Goal FromRunningDistance(RunningDistance running)
    {
        return new Goal
        {
            Type = "Running",
            Value = running.Value,
            Unit = running.Unit.ToString()
        };
    }

    public static Goal FromWaterContent(WaterContent water)
    {
        return new Goal
        {
            Type = "Water",
            Value = water.Value,
            Unit = water.Unit.ToString()
        };
    }
}