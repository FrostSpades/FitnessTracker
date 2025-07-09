// FitnessTracker/Views/EnterProgress.xaml.cs
using System.Windows;
using System.Windows.Controls;
using FitnessTracker.ViewModels;


namespace FitnessTracker.Views;


public partial class EnterProgress : UserControl
{
    private readonly EnterProgressViewModel _viewModel;

    public EnterProgress(EnterProgressViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }


    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is EnterProgressViewModel viewModel)
        {
            var success = await viewModel.SaveAsync();
            if (success)
            {
                // Close the dialog window if this control is hosted in one
                var window = Window.GetWindow(this);
                if (window != null && window != Application.Current.MainWindow)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
        }
    }
}