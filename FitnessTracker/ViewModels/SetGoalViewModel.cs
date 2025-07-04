using System.ComponentModel;
using System.Runtime.CompilerServices;
using FitnessTracker.Models;
using FitnessTracker.Services;

namespace FitnessTracker.ViewModels
{
    /// <summary>
    /// View-model for the Set-Goal dialog.
    /// Contains all validation and persistence logic, injected with <see cref="IGoalService"/>.
    /// </summary>
    public class SetGoalViewModel : INotifyPropertyChanged
    {
        private readonly IGoalService _goalService;

        public SetGoalViewModel(IGoalService goalService)
        {
            _goalService = goalService;
        }

        private string _selectedGoalType = "Running";
        private float  _goalValue;
        private DistanceUnit _selectedDistanceUnit = DistanceUnit.Miles;
        private WaterUnit    _selectedWaterUnit    = WaterUnit.Ounces;
        private Goal? _lastSavedGoal;

        public string[]       GoalTypes     { get; } = { "Running", "Water" };
        public DistanceUnit[] DistanceUnits { get; } = Enum.GetValues<DistanceUnit>();
        public WaterUnit[]    WaterUnits    { get; } = Enum.GetValues<WaterUnit>();

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
                    var previous = _selectedDistanceUnit;
                    _selectedDistanceUnit = value;

                    if (IsRunningGoal && GoalValue > 0)
                    {
                        GoalValue = new RunningDistance
                        {
                            Unit  = previous,
                            Value = GoalValue
                        }.ConvertTo(value);
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
                        GoalValue = new WaterContent
                        {
                            Unit  = previous,
                            Value = GoalValue
                        }.ConvertTo(value);
                    }

                    OnPropertyChanged();
                }
            }
        }

        public bool IsRunningGoal => SelectedGoalType == "Running";
        public bool IsWaterGoal   => SelectedGoalType == "Water";

        /// <summary>The goal that was just saved (null until first save).</summary>
        public Goal? LastSavedGoal
        {
            get => _lastSavedGoal;
            private set => SetField(ref _lastSavedGoal, value);
        }

        /// <summary>
        /// Validates input and saves the goal.  
        /// Returns <c>true</c> on success; < c>false</c> means validation failed.
        /// </summary>
        public async Task<bool> SaveAsync()
        {
            if (GoalValue <= 0)
                return false;

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
                return false;
            }

            LastSavedGoal = saved;
            return true;
        }

        public RunningDistance GetRunningGoal() => new()
        {
            Unit  = SelectedDistanceUnit,
            Value = GoalValue
        };

        public WaterContent GetWaterGoal() => new()
        {
            Unit  = SelectedWaterUnit,
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
