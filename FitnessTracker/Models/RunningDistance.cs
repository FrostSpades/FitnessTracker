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

    public float ConvertTo(DistanceUnit targetUnit)
    {
        if (Unit == targetUnit) return Value;

        float meters = Unit switch
        {
            DistanceUnit.Miles => Value * 1609.34f,
            DistanceUnit.Kilometers => Value * 1000f,
            DistanceUnit.Feet => Value * 0.3048f,
            DistanceUnit.Meters => Value,
            _ => Value
        };

        return targetUnit switch
        {
            DistanceUnit.Miles => meters / 1609.34f,
            DistanceUnit.Kilometers => meters / 1000f,
            DistanceUnit.Feet => meters / 0.3048f,
            DistanceUnit.Meters => meters,
            _ => meters
        };
    }
}