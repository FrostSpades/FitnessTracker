namespace FitnessTracker.Models;

/// <summary>
/// Model containing fitness information.
/// </summary>
public class FitnessData
{
    public RunningDistance Distance { get; set; } = new RunningDistance();
    public WaterContent WaterContent { get; set; } = new WaterContent();
}