namespace FitnessTracker.Models;

public enum DistanceUnit
{
    Miles,
    Meters,
    Kilometers,
    Feet
}

/// <summary>
/// Model containing distance information
/// </summary>
public class RunningDistance
{
    public DistanceUnit Unit { get; set; }
    public float Value { get; set; }
}