using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Data;

/// <summary>
/// Represents the Entity Framework Core database context for the application.
/// </summary>
public class FitnessTrackerDbContext : DbContext
{
    public FitnessTrackerDbContext(DbContextOptions<FitnessTrackerDbContext> options)
        : base(options) { }

    // Table for tracking user-defined goals.
    public DbSet<Goal> Goals { get; set; } = null!;

    // Table for tracking user progress entries.
    public DbSet<ProgressEntry> ProgressEntries { get; set; } = null!;
}