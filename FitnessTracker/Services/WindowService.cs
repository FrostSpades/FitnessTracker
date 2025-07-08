// FitnessTracker/Services/WindowService.cs
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using FitnessTracker.Views;


namespace FitnessTracker.Services;


public class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
    
    public WindowService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public void ShowSetGoalDialog()
    {
        var setGoalWindow = new Window
        {
            Title = "Set Goal",
            Content = _serviceProvider.GetRequiredService<SetGoal>(),
            Width = 325,
            Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow
        };


        setGoalWindow.ShowDialog();
    }
}