using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FitnessTracker.Models;

namespace FitnessTracker.ViewModels;

/// <summary>
/// ViewModel for the fitness related information
/// </summary>
public class FitnessViewModel : INotifyPropertyChanged
{
    private FitnessData _data = new FitnessData();
    public event PropertyChangedEventHandler? PropertyChanged;
    public ICommand AddRunDistanceCommand { get;}
    public ICommand AddWaterCommand { get;}
    
    public ObservableCollection<DistanceUnit> DistanceUnits { get; }
        = new ObservableCollection<DistanceUnit>((DistanceUnit[])Enum.GetValues(typeof(DistanceUnit)));

    public ObservableCollection<WaterUnit> WaterUnits { get; }
        = new ObservableCollection<WaterUnit>((WaterUnit[])Enum.GetValues(typeof(WaterUnit)));
    
    /// <summary>
    /// The current run distance selected
    /// </summary>
    public DistanceUnit SelectedDistanceUnit
    {
        get => _data.Distance.Unit;
        set
        {
            if (_data.Distance.Unit != value)
            {
                _data.Distance.Unit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayedDistance));
            }
                
        }
    }
    
    /// <summary>
    /// The distance displayed
    /// </summary>
    public double DisplayedDistance
    {
        get
        {
            return SelectedDistanceUnit switch
            {
                DistanceUnit.Miles => _data.Distance.Value,
                DistanceUnit.Kilometers => _data.Distance.Value * 1.60934,
                _ => _data.Distance.Value
            };
        }
    }
    
    /// <summary>
    /// The total water intake.
    /// </summary>
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
    
    /// <summary>
    /// Default Constructor
    /// </summary>
    public FitnessViewModel()
    {
        AddRunDistanceCommand = new RelayCommand(AddRunDistance);
        AddWaterCommand = new RelayCommand(AddWater);
    }
    
    
    private void AddRunDistance()
    {
        _data.Distance.Value += 1;
        OnPropertyChanged(nameof(DisplayedDistance));
    }

    private void AddWater()
    {
        Console.WriteLine("Adding Water");
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}