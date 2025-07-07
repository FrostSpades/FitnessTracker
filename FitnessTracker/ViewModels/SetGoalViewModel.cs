// FitnessTracker/ViewModels/SetGoalViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FitnessTracker.Models;
using FitnessTracker.Services;
using Microsoft.Extensions.Logging;


namespace FitnessTracker.ViewModels
{
    /// <summary>
    /// View-model for the Set-Goal dialog.
    /// Contains all validation and persistence logic, injected with <see cref="IGoalService"/>.
    /// </summary>
    public class SetGoalViewModel : INotifyPropertyChanged
    {
        private readonly IGoalService _goalService;
        private readonly ILogger<SetGoalViewModel>? _logger;


        public SetGoalViewModel(IGoalService goalService, ILogger<SetGoalViewModel>? logger = null)
        {
            _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));
            _logger = logger;
        }


        private string _selectedGoalType = "Running";
        private float _goalValue;
        private DistanceUnit _selectedDistanceUnit = DistanceUnit.Miles;
        private WaterUnit _selectedWaterUnit = WaterUnit.Ounces;
        private Goal? _lastSavedGoal;
        private string? _validationError;
        private bool _isSaving;


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
            set
            {
                if (SetField(ref _goalValue, value))
                {
                    ValidateGoalValue();
                }
            }
        }


        public DistanceUnit SelectedDistanceUnit
        {
            get => _selectedDistanceUnit;
            set
            {
                if (_selectedDistanceUnit != value)
                {
                    var previous = _selectedDistanceUnit;
                    _selectedDistanceUnit = value;


                    if (IsRunningGoal && GoalValue > 0)
                    {
                        try
                        {
                            GoalValue = new RunningDistance
                            {
                                Unit = previous,
                                Value = GoalValue
                            }.ConvertTo(value);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Failed to convert distance units from {Previous} to {New}", previous, value);
                        }
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
                    var previous = _selectedWaterUnit;
                    _selectedWaterUnit = value;


                    if (IsWaterGoal && GoalValue > 0)
                    {
                        try
                        {
                            GoalValue = new WaterContent
                            {
                                Unit = previous,
                                Value = GoalValue
                            }.ConvertTo(value);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Failed to convert water units from {Previous} to {New}", previous, value);
                        }
                    }


                    OnPropertyChanged();
                }
            }
        }


        public bool IsRunningGoal => SelectedGoalType == "Running";
        public bool IsWaterGoal => SelectedGoalType == "Water";


        /// <summary>The goal that was just saved (null until first save).</summary>
        public Goal? LastSavedGoal
        {
            get => _lastSavedGoal;
            private set => SetField(ref _lastSavedGoal, value);
        }


        /// <summary>Current validation error message, if any.</summary>
        public string? ValidationError
        {
            get => _validationError;
            private set => SetField(ref _validationError, value);
        }


        /// <summary>Indicates if a save operation is in progress.</summary>
        public bool IsSaving
        {
            get => _isSaving;
            private set => SetField(ref _isSaving, value);
        }


        /// <summary>Indicates if the current input is valid.</summary>
        public bool IsValid => string.IsNullOrEmpty(ValidationError);


        /// <summary>
        /// Validates input and saves the goal.  
        /// Returns <c>true</c> on success; <c>false</c> means validation failed.
        /// </summary>
        public async Task<bool> SaveAsync()
        {
            if (IsSaving) return false;


            ValidateGoalValue();
            if (!IsValid) return false;


            IsSaving = true;
            try
            {
                Goal saved;
                if (IsRunningGoal)
                {
                    saved = await _goalService.SaveGoalAsync(
                                Goal.FromRunningDistance(GetRunningGoal()));
                }
                else if (IsWaterGoal)
                {
                    saved = await _goalService.SaveGoalAsync(
                                Goal.FromWaterContent(GetWaterGoal()));
                }
                else
                {
                    ValidationError = "Invalid goal type selected";
                    return false;
                }


                LastSavedGoal = saved;
                ValidationError = null;
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save goal of type {Type} with value {Value}", SelectedGoalType, GoalValue);
                ValidationError = "Failed to save goal. Please try again.";
                return false;
            }
            finally
            {
                IsSaving = false;
            }
        }


        private void ValidateGoalValue()
        {
            if (GoalValue <= 0)
            {
                ValidationError = "Goal value must be greater than 0";
            }
            else if (float.IsNaN(GoalValue) || float.IsInfinity(GoalValue))
            {
                ValidationError = "Goal value must be a valid number";
            }
            else
            {
                ValidationError = null;
            }
        }


        public RunningDistance GetRunningGoal() => new()
        {
            Unit = SelectedDistanceUnit,
            Value = GoalValue
        };


        public WaterContent GetWaterGoal() => new()
        {
            Unit = SelectedWaterUnit,
            Value = GoalValue
        };


        public event PropertyChangedEventHandler? PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


            if (name == nameof(SelectedGoalType))
            {
                OnPropertyChanged(nameof(IsRunningGoal));
                OnPropertyChanged(nameof(IsWaterGoal));
                ValidateGoalValue(); // Re-validate when goal type changes
            }
        }


        protected bool SetField<T>(ref T field, T value,
                                   [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}