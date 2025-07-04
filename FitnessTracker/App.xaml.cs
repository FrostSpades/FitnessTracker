using System;
using System.Windows;
using FitnessTracker.Services;
using FitnessTracker.ViewModels;
using FitnessTracker.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure DI
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Launch your starting window
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<GoalService>();

            // Register viewmodels
            services.AddTransient<SetGoalViewModel>();
            services.AddTransient<FitnessViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<HomeViewModel>();

            // Register views
            services.AddTransient<MainWindow>();
            services.AddTransient<SetGoal>();
            services.AddTransient<Home>();
        }
    }
}