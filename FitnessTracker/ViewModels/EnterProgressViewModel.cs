// FitnessTracker/ViewModels/EnterProgressViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FitnessTracker.Models;
using FitnessTracker.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitnessTracker.ViewModels
{
    public class EnterProgressViewModel : INotifyPropertyChanged
    {
        private readonly IProgressService _progressService;
        private readonly ILogger<EnterProgressViewModel>? _logger;

        public EnterProgressViewModel(IProgressService progressService, ILogger<EnterProgressViewModel>? logger = null)
        {
            _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
            _logger = logger;
        }

        private GoalType _selectedProgressType = GoalType.Running;
        private float _progressValue;
        private DistanceUnit _selectedDistanceUnit = DistanceUnit.Miles;
        private WaterUnit _selectedWaterUnit = WaterUnit.Ounces;
        private string? _notes;
        private ProgressEntry? _lastSavedProgress;
        private string? _validationError;
        private bool _isSaving;

        public GoalType[] ProgressTypes { get; } = Enum.GetValues<GoalType>();
        public DistanceUnit[] DistanceUnits { get; } = Enum.GetValues<DistanceUnit>();
        public WaterUnit[] WaterUnits { get; } = Enum.GetValues<WaterUnit>();

        public GoalType SelectedProgressType
        {
            get => _selectedProgressType;
            set => SetField(ref _selectedProgressType, value);
        }

        public float ProgressValue
        {
            get => _progressValue;
            set => SetField(ref _progressValue, value);
        }

        public DistanceUnit SelectedDistanceUnit
        {
            get => _selectedDistanceUnit;
            set => SetField(ref _selectedDistanceUnit, value);
        }

        public WaterUnit SelectedWaterUnit
        {
            get => _selectedWaterUnit;
            set => SetField(ref _selectedWaterUnit, value);
        }

        public string? Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        public bool IsRunningProgress => SelectedProgressType == GoalType.Running;
        public bool IsWaterProgress => SelectedProgressType == GoalType.Water;

        public ProgressEntry? LastSavedProgress
        {
            get => _lastSavedProgress;
            private set => SetField(ref _lastSavedProgress, value);
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

        public async Task<bool> SaveAsync()
        {
            if (IsSaving) return false;

            IsSaving = true;
            try
            {
                var saved = await _progressService.SaveProgressAsync(
                    SelectedProgressType, ProgressValue, SelectedDistanceUnit, SelectedWaterUnit, Notes);

                LastSavedProgress = saved;
                ValidationError = null;
                ResetForm();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save progress");
                ValidationError = ex.Message;
                return false;
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ResetForm()
        {
            ProgressValue = 0;
            Notes = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            if (name == nameof(SelectedProgressType))
            {
                OnPropertyChanged(nameof(IsRunningProgress));
                OnPropertyChanged(nameof(IsWaterProgress));
            }
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}