namespace FitnessTracker.Models;

public enum DistanceUnit
{
    Miles,
    Meters,
    Kilometers,
    Feet
}

public class RunningDistance
{
    public DistanceUnit Unit { get; set; }
    public float Value { get; set; }
}