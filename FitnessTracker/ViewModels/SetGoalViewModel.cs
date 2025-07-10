using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessTracker.Models;
using FitnessTracker.Services;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.ViewModels;

public class SetGoalViewModel : INotifyPropertyChanged
{
    private readonly IGoalService _goalService;
    private readonly ILogger<SetGoalViewModel>? _logger;

    public SetGoalViewModel(IGoalService goalService,
                            ILogger<SetGoalViewModel>? logger = null)
    {
        _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));
        _logger      = logger;
    }

    private GoalType     _selectedGoalType   = GoalType.Running;
    private float        _goalValue;
    private DistanceUnit _selectedDistanceUnit = DistanceUnit.Miles;
    private WaterUnit    _selectedWaterUnit    = WaterUnit.Ounces;
    private Goal?        _lastSavedGoal;
    private string?      _validationError;
    private bool         _isSaving;

    public GoalType[]     GoalTypes     { get; } = Enum.GetValues<GoalType>();
    public DistanceUnit[] DistanceUnits { get; } = Enum.GetValues<DistanceUnit>();
    public WaterUnit[]    WaterUnits    { get; } = Enum.GetValues<WaterUnit>();

    public GoalType SelectedGoalType
    {
        get => _selectedGoalType;
        set => SetField(ref _selectedGoalType, value);
    }

    public float GoalValue
    {
        get => _goalValue;
        set => SetField(ref _goalValue, value);
    }

    /* --------- unit pickers with live conversion ----------------------- */

    public DistanceUnit SelectedDistanceUnit
    {
        get => _selectedDistanceUnit;
        set
        {
            if (_selectedDistanceUnit != value)
            {
                var previous            = _selectedDistanceUnit;
                _selectedDistanceUnit   = value;

                try
                {
                    GoalValue = _goalService.AdjustDistanceValue(
                        SelectedGoalType, GoalValue, previous, value);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex,
                        "Failed distance conversion {Prev}->{New}", previous, value);
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
                var previous          = _selectedWaterUnit;
                _selectedWaterUnit    = value;

                try
                {
                    GoalValue = _goalService.AdjustWaterValue(
                        SelectedGoalType, GoalValue, previous, value);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex,
                        "Failed water conversion {Prev}->{New}", previous, value);
                }
                OnPropertyChanged();
            }
        }
    }

    public bool IsRunningGoal => SelectedGoalType == GoalType.Running;
    public bool IsWaterGoal   => SelectedGoalType == GoalType.Water;

    public Goal? LastSavedGoal
    {
        get => _lastSavedGoal;
        private set => SetField(ref _lastSavedGoal, value);
    }

    public string? ValidationError
    {
        get => _validationError;
        private set => SetField(ref _validationError, value);
    }

    public bool IsSaving
    {
        get => _isSaving;
        private set => SetField(ref _isSaving, value);
    }

    /* ---------------- save command ------------------------------------- */

    public async Task<bool> SaveAsync()
    {
        if (IsSaving) return false;

        IsSaving = true;
        try
        {
            LastSavedGoal = await _goalService.SaveGoalAsync(
                SelectedGoalType, GoalValue, SelectedDistanceUnit, SelectedWaterUnit);

            ValidationError = null;
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save goal");
            ValidationError = ex.Message;
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }

    /* ---------------- INotifyPropertyChanged boilerplate --------------- */

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
