// FitnessTracker/ViewModels/EnterProgressViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FitnessTracker.Models;
using FitnessTracker.Services;
using Microsoft.Extensions.Logging;


namespace FitnessTracker.ViewModels;


public class EnterProgressViewModel : INotifyPropertyChanged
{
    private readonly IProgressService _progressService;
    private readonly ILogger<EnterProgressViewModel>? _logger;


    public EnterProgressViewModel(IProgressService progressService, ILogger<EnterProgressViewModel>? logger = null)
    {
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _logger = logger;
    }


    private string _selectedProgressType = "Running";
    private float _progressValue;
    private DistanceUnit _selectedDistanceUnit = DistanceUnit.Miles;
    private WaterUnit _selectedWaterUnit = WaterUnit.Ounces;
    private string? _notes;
    private ProgressEntry? _lastSavedProgress;
    private string? _validationError;
    private bool _isSaving;


    public string[] ProgressTypes { get; } = { "Running", "Water" };
    public DistanceUnit[] DistanceUnits { get; } = Enum.GetValues<DistanceUnit>();
    public WaterUnit[] WaterUnits { get; } = Enum.GetValues<WaterUnit>();


    public string SelectedProgressType
    {
        get => _selectedProgressType;
        set => SetField(ref _selectedProgressType, value);
    }


    public float ProgressValue
    {
        get => _progressValue;
        set
        {
            if (SetField(ref _progressValue, value))
            {
                ValidateProgressValue();
            }
        }
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


    public bool IsRunningProgress => SelectedProgressType == "Running";
    public bool IsWaterProgress => SelectedProgressType == "Water";


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


    public bool IsValid => string.IsNullOrEmpty(ValidationError);


    public async Task<bool> SaveAsync()
    {
        if (IsSaving) return false;


        ValidateProgressValue();
        if (!IsValid) return false;


        IsSaving = true;
        try
        {
            ProgressEntry saved;
            if (IsRunningProgress)
            {
                saved = await _progressService.SaveProgressAsync(
                    ProgressEntry.FromRunningDistance(GetRunningProgress(), Notes));
            }
            else if (IsWaterProgress)
            {
                saved = await _progressService.SaveProgressAsync(
                    ProgressEntry.FromWaterContent(GetWaterProgress(), Notes));
            }
            else
            {
                ValidationError = "Invalid progress type selected";
                return false;
            }


            LastSavedProgress = saved;
            ValidationError = null;
            
            // Reset form after successful save
            ResetForm();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save progress of type {Type} with value {Value}", 
                SelectedProgressType, ProgressValue);
            ValidationError = "Failed to save progress. Please try again.";
            return false;
        }
        finally
        {
            IsSaving = false;
        }
    }


    private void ValidateProgressValue()
    {
        if (ProgressValue <= 0)
        {
            ValidationError = "Progress value must be greater than 0";
        }
        else if (float.IsNaN(ProgressValue) || float.IsInfinity(ProgressValue))
        {
            ValidationError = "Progress value must be a valid number";
        }
        else
        {
            ValidationError = null;
        }
    }


    private void ResetForm()
    {
        ProgressValue = 0;
        Notes = null;
    }


    public RunningDistance GetRunningProgress() => new()
    {
        Unit = SelectedDistanceUnit,
        Value = ProgressValue
    };


    public WaterContent GetWaterProgress() => new()
    {
        Unit = SelectedWaterUnit,
        Value = ProgressValue
    };


    public event PropertyChangedEventHandler? PropertyChanged;


    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        if (name == nameof(SelectedProgressType))
        {
            OnPropertyChanged(nameof(IsRunningProgress));
            OnPropertyChanged(nameof(IsWaterProgress));
            ValidateProgressValue();
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