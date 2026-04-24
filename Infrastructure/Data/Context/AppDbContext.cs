using CONEX_APP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Data.Context;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<Activity> Activities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=conex_app.db");
    }
}
