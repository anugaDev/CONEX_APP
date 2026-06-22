using CONEX_APP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Data.Context;

public enum DatabaseMode
{
    Test,
    Production
}

public class AppDbContext : DbContext
{
    public static DatabaseMode Mode { get; set; } = DatabaseMode.Production;

    private static string DatabaseFileName => Mode switch
    {
        DatabaseMode.Test => "conex_app_test.db",
        DatabaseMode.Production => "conex_app.db",
        _ => "conex_app.db"
    };

    public DbSet<User> Users { get; set; }
    
    public DbSet<Activity> Activities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DatabaseFileName}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Activities)
            .WithMany(a => a.Students)
            .UsingEntity<Dictionary<string, object>>(
                "ActivityUser",
                j => j.HasOne<Activity>().WithMany().HasForeignKey("ActivitiesId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("StudentsId"));
    }
}
