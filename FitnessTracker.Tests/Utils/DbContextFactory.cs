using FitnessTracker.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Tests.Utils;

public static class DbContextFactory
{
    public static FitnessTrackerDbContext CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open(); // Must remain open for in-memory DB to persist during test

        var options = new DbContextOptionsBuilder<FitnessTrackerDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new FitnessTrackerDbContext(options);
        context.Database.EnsureCreated(); // Apply schema

        return context;
    }
}