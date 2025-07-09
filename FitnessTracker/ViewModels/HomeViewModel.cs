// FitnessTracker/ViewModels/HomeViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FitnessTracker.Services;


namespace FitnessTracker.ViewModels;


public class HomeViewModel : INotifyPropertyChanged
{
    private readonly IWindowService _windowService;
    
    public ICommand SetGoalCommand { get; }
    public ICommand EnterProgressCommand { get; }
    
    public HomeViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        SetGoalCommand = new RelayCommand(ExecuteSetGoal);
        EnterProgressCommand = new RelayCommand(ExecuteEnterProgress);
    }
    
    private void ExecuteEnterProgress()
    {
        _windowService.ShowEnterProgressDialog();
    }
    
    private void ExecuteSetGoal()
    {
        _windowService.ShowSetGoalDialog();
    }


    public event PropertyChangedEventHandler? PropertyChanged;


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