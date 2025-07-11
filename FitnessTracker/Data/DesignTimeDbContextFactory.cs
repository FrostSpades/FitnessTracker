using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace FitnessTracker.Data;

/// <summary>
/// Factory class used to create the DbContext at design time (e.g., for migrations).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FitnessTrackerDbContext>
{
    public FitnessTrackerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FitnessTrackerDbContext>();

        // Define the path for the local SQLite database file.
        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FitnessTracker.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        // Return the configured DbContext.
        return new FitnessTrackerDbContext(optionsBuilder.Options);
    }
}