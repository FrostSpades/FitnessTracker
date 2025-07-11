using System;
using System.IO;
using System.Windows;
using FitnessTracker.Data;
using FitnessTracker.Repositories;
using FitnessTracker.Services;
using FitnessTracker.ViewModels;
using FitnessTracker.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessTracker
{
    /// <summary>Application bootstrapper wired for Dependency Injection.</summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            ServiceProvider.GetRequiredService<MainWindow>().Show();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Setup SQLite and EF Core
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FitnessTracker.db");
            services.AddDbContext<FitnessTrackerDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Services & Repositories
            services.AddScoped<IGoalRepository, GoalRepository>();
            services.AddScoped<IProgressRepository, ProgressRepository>();
            services.AddScoped<IGoalService, GoalService>();
            services.AddScoped<IProgressService, ProgressService>();
            services.AddScoped<IWindowService, WindowService>();

            // ViewModels
            services.AddTransient<SetGoalViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<EnterProgressViewModel>();

            // Views
            services.AddTransient<SetGoal>();
            services.AddTransient<Home>();
            services.AddSingleton<MainWindow>();
            services.AddTransient<EnterProgress>();
        }
    }
}
