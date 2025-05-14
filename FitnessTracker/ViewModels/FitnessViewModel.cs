using System.Windows.Input;
using FitnessTracker.Models;

namespace FitnessTracker.ViewModels;

public class FitnessViewModel
{
    private FitnessData _data = new FitnessData();
    
    public ICommand AddRunDistanceCommand { get;}

    public FitnessViewModel()
    {
        AddRunDistanceCommand = new RelayCommand(AddRunDistance);
    }
    
    public double WaterIntake
    {
        get
        {
            return _data.WaterContent.Unit switch
            {
                WaterUnit.Ounces => _data.WaterContent.Value,
                WaterUnit.Cups => _data.WaterContent.Value / 8,
                WaterUnit.Liters => _data.WaterContent.Value * 0.0295735,
                _ => _data.WaterContent.Value,
            };
        }
    }

    private void AddRunDistance()
    {
        Console.WriteLine("Adding Run Distance");
    }
}