namespace FitnessTracker.Models;

public enum WaterUnit
{
    Ounces,
    Cups,
    Liters
}

public class WaterContent 
{
    public WaterUnit Unit { get; set; }
    public float Value { get; set; }
}