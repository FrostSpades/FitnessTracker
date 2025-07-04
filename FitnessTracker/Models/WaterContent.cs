namespace FitnessTracker.Models;

public enum WaterUnit
{
    Ounces,
    Cups,
    Liters
}

/// <summary>
/// Model containing water information
/// </summary>
public class WaterContent 
{
    public WaterUnit Unit { get; set; }
    public float Value { get; set; }

    public float ConvertTo(WaterUnit targetUnit)
    {
        if (Unit == targetUnit) return Value;

        float ounces = Unit switch
        {
            WaterUnit.Cups => Value * 8f,
            WaterUnit.Liters => Value * 33.814f,
            WaterUnit.Ounces => Value,
            _ => Value
        };

        return targetUnit switch
        {
            WaterUnit.Cups => ounces / 8f,
            WaterUnit.Liters => ounces / 33.814f,
            WaterUnit.Ounces => ounces,
            _ => ounces
        };
    }
}