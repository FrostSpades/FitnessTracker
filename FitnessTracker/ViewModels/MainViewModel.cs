using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FitnessTracker.ViewModels;

namespace FitnessTracker.ViewModels;

/// <summary>
/// Main ViewModel class that interfaces with the View.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    // Relay Commands
    public ICommand ShowSetGoal { get; }
    public ICommand ShowHome { get; }

    // The current view model
    public INotifyPropertyChanged CurrentViewModel { get; private set; }

    private readonly SetGoalViewModel _setGoalVM = new SetGoalViewModel();
    private readonly HomeViewModel _homeVM = new HomeViewModel();
    
    public FitnessViewModel Fitness { get;} = new FitnessViewModel();


    /// <summary>
    /// Default Constructor
    /// </summary>
    public MainViewModel()
    {
        CurrentViewModel = _homeVM;
        ShowSetGoal = new RelayCommand(() => SwitchTo(_setGoalVM));
        ShowHome = new RelayCommand(() => SwitchTo(_homeVM));
    }

    private void SwitchTo(INotifyPropertyChanged viewModel)
    {
        CurrentViewModel = viewModel;
        OnPropertyChanged(nameof(CurrentViewModel));
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