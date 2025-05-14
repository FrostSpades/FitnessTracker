namespace FitnessTracker.Models;

public class FitnessData
{
    public RunningDistance Distance { get; set; } = new RunningDistance();
    public WaterContent WaterContent { get; set; } = new WaterContent();
}