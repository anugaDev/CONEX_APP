using CONEX_APP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Data.Context;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Activity> Activities { get; set; }

    public DbSet<Renewal> Renewals { get; set; }

    public DbSet<RenewalClass> RenewalClasses { get; set; }

    public DbSet<AppSettings> AppSettings { get; set; }

    public DbSet<Waitlist> Waitlists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(DatabasePaths.BuildConnectionString(DatabasePaths.CurrentDatabasePath));
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

        modelBuilder.Entity<Renewal>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Renewal>()
            .HasMany(r => r.Classes)
            .WithOne()
            .HasForeignKey(c => c.RenewalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Waitlist>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Waitlist>()
            .HasOne(w => w.Activity)
            .WithMany()
            .HasForeignKey(w => w.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
