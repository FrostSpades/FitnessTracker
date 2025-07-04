// FitnessTracker/ViewModels/SetGoalViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FitnessTracker.Models;

namespace FitnessTracker.ViewModels;

public class SetGoalViewModel : INotifyPropertyChanged
{
    private string _selectedGoalType = "Running";
    private float _goalValue;
    private DistanceUnit _selectedDistanceUnit = DistanceUnit.Miles;
    private WaterUnit _selectedWaterUnit = WaterUnit.Ounces;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string[] GoalTypes { get; } = { "Running", "Water" };
    public DistanceUnit[] DistanceUnits { get; } = Enum.GetValues<DistanceUnit>();
    public WaterUnit[] WaterUnits { get; } = Enum.GetValues<WaterUnit>();

    public string SelectedGoalType
    {
        get => _selectedGoalType;
        set => SetField(ref _selectedGoalType, value);
    }

    public float GoalValue
    {
        get => _goalValue;
        set => SetField(ref _goalValue, value);
    }

    public DistanceUnit SelectedDistanceUnit
    {
        get => _selectedDistanceUnit;
        set
        {
            if (_selectedDistanceUnit != value)
            {
                var previousUnit = _selectedDistanceUnit;
                _selectedDistanceUnit = value;

                // Convert value if goal is running
                if (IsRunningGoal && GoalValue > 0)
                {
                    var temp = new RunningDistance
                    {
                        Unit = previousUnit,
                        Value = GoalValue
                    };
                    GoalValue = temp.ConvertTo(value);
                }

                OnPropertyChanged();
            }
        }
    }

    public WaterUnit SelectedWaterUnit
    {
        get => _selectedWaterUnit;
        set
        {
            if (_selectedWaterUnit != value)
            {
                var previousUnit = _selectedWaterUnit;
                _selectedWaterUnit = value;

                // Convert value if goal is water
                if (IsWaterGoal && GoalValue > 0)
                {
                    var temp = new WaterContent
                    {
                        Unit = previousUnit,
                        Value = GoalValue
                    };
                    GoalValue = temp.ConvertTo(value);
                }

                OnPropertyChanged();
            }
        }
    }

    public bool IsRunningGoal => SelectedGoalType == "Running";
    public bool IsWaterGoal => SelectedGoalType == "Water";

    public RunningDistance GetRunningGoal()
    {
        return new RunningDistance
        {
            Unit = SelectedDistanceUnit,
            Value = GoalValue
        };
    }

    public WaterContent GetWaterGoal()
    {
        return new WaterContent
        {
            Unit = SelectedWaterUnit,
            Value = GoalValue
        };
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (propertyName == nameof(SelectedGoalType))
        {
            OnPropertyChanged(nameof(IsRunningGoal));
            OnPropertyChanged(nameof(IsWaterGoal));
        }
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
