namespace CONEX_APP.Infrastructure.Data.Context;

public class AppDbContextFactory
{
    public AppDbContext Create() => new AppDbContext();
}
