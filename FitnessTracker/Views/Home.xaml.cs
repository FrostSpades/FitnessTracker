// FitnessTracker/Views/Home.xaml.cs
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessTracker.Views;

public partial class Home : UserControl
{
    public Home()
    {
        InitializeComponent();
    }

    private void SetGoalButton_Click(object sender, RoutedEventArgs e)
    {
        var setGoalWindow = new Window
        {
            Title = "Set Goal",
            Content = App.ServiceProvider.GetRequiredService<SetGoal>(),
            Width = 325,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow
        };

        setGoalWindow.ShowDialog();
    }
}