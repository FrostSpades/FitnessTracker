using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FitnessTracker.ViewModels
{
    /// <summary>
    /// Shell-level view-model that coordinates page navigation.
    /// All child view-models are supplied by DI.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Commands
        public ICommand ShowSetGoal { get; }
        public ICommand ShowHome    { get; }

        // The currently displayed view-model
        private INotifyPropertyChanged _currentViewModel;
        public  INotifyPropertyChanged  CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetField(ref _currentViewModel, value);
        }

        // Keep references so they aren’t GC’d away
        private readonly SetGoalViewModel _setGoalVM;
        private readonly HomeViewModel    _homeVM;

        /// <summary>
        /// All dependencies arrive via constructor injection.
        /// </summary>
        public MainViewModel(HomeViewModel    homeVM,
                             SetGoalViewModel setGoalVM)
        {
            _homeVM     = homeVM;
            _setGoalVM  = setGoalVM;

            CurrentViewModel = _homeVM;

            ShowSetGoal = new RelayCommand(() => SwitchTo(_setGoalVM));
            ShowHome    = new RelayCommand(() => SwitchTo(_homeVM));
        }

        private void SwitchTo(INotifyPropertyChanged vm)
        {
            CurrentViewModel = vm;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
