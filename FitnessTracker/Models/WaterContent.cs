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
}