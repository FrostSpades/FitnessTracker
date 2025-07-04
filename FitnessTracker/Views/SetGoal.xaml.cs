using System.Windows;
using System.Windows.Controls;
using FitnessTracker.ViewModels;

namespace FitnessTracker.Views
{
    /// <summary>Code-behind is **UI-only**: shows dialogs and closes the window.</summary>
    public partial class SetGoal : UserControl
    {
        private SetGoalViewModel ViewModel => (SetGoalViewModel)DataContext;

        // ViewModel is supplied by DI; no “new” here.
        public SetGoal(SetGoalViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await ViewModel.SaveAsync())
            {
                MessageBox.Show("Please enter a valid goal value.",
                    "Invalid Input",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var g = ViewModel.LastSavedGoal!;
            MessageBox.Show($"{g.Type} goal set: {g.Value} {g.Unit}",
                "Goal Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            CloseDialog();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => CloseDialog();

        private void CloseDialog() => Window.GetWindow(this)?.Close();
    }
}