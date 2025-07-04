// FitnessTracker/Views/SetGoal.xaml.cs
using System.Windows;
using System.Windows.Controls;
using FitnessTracker.ViewModels;
using FitnessTracker.Models;

namespace FitnessTracker.Views;

public partial class SetGoal : UserControl
{
    private SetGoalViewModel ViewModel => (SetGoalViewModel)DataContext;

    public SetGoal()
    {
        InitializeComponent();
        DataContext = new SetGoalViewModel();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.GoalValue <= 0)
        {
            MessageBox.Show("Please enter a valid goal value.", "Invalid Input", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (ViewModel.IsRunningGoal)
        {
            var runningGoal = ViewModel.GetRunningGoal();
            // TODO: Save running goal to your data store
            MessageBox.Show($"Running goal set: {runningGoal.Value} {runningGoal.Unit}",
                "Goal Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else if (ViewModel.IsWaterGoal)
        {
            var waterGoal = ViewModel.GetWaterGoal();
            // TODO: Save water goal to your data store
            MessageBox.Show($"Water goal set: {waterGoal.Value} {waterGoal.Unit}",
                "Goal Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        CloseDialog();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        CloseDialog();
    }

    private void CloseDialog()
    {
        var window = Window.GetWindow(this);
        window?.Close();
    }
}