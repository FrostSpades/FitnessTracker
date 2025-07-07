using System;
using System.Windows;
using FitnessTracker.Repositories;
using FitnessTracker.Services;
using FitnessTracker.ViewModels;
using FitnessTracker.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessTracker
{
    /// <summary>Application bootstrapper wired for Dependency Injection.</summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        // make it async and await the load
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var goalService = ServiceProvider.GetRequiredService<IGoalService>();
            await goalService.LoadGoalsAsync();     // ← no deadlock

            ServiceProvider.GetRequiredService<MainWindow>()
                .Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<IGoalRepository, GoalRepository>();
            services.AddSingleton<IGoalService,    GoalService>();

            // View-models
            services.AddTransient<SetGoalViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<FitnessViewModel>();
            services.AddTransient<MainViewModel>();

            // Views
            services.AddTransient<SetGoal>();
            services.AddTransient<Home>();
            services.AddSingleton<MainWindow>(); 
        }
    }
}