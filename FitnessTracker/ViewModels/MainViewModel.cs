using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FitnessTracker.ViewModels;

/// <summary>
/// Main ViewModel class that interfaces with the View.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public FitnessViewModel Fitness { get;} = new FitnessViewModel();
    
    public MainViewModel()
    {}
    
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