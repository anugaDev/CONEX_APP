using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;

namespace CONEX_APP.Infrastructure.Repositories;

public class AppSettingsRepository : IAppSettingsRepository
{
    private const int SettingsId = 1;

    private readonly AppDbContextFactory _contextFactory;

    public AppSettingsRepository(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<AppSettings> GetAsync()
    {
        // Siempre hay un único registro con Id = 1 (lo crea schema.sql). Si faltara, lo creamos aquí.
        await using AppDbContext context = _contextFactory.Create();
        AppSettings? settings = await context.AppSettings.FindAsync(SettingsId);
        if (settings == null)
        {
            settings = new AppSettings { Id = SettingsId, RenewalPeriodMonths = 12, RenewalCost = 0, ClassCost = 0 };
            context.AppSettings.Add(settings);
            await context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task SaveAsync(AppSettings settings)
    {
        await using AppDbContext context = _contextFactory.Create();
        AppSettings? existing = await context.AppSettings.FindAsync(settings.Id);
        if (existing == null)
        {
            context.AppSettings.Add(settings);
        }
        else
        {
            existing.RenewalPeriodMonths = settings.RenewalPeriodMonths;
            existing.RenewalCost = settings.RenewalCost;
            existing.ClassCost = settings.ClassCost;
        }
        await context.SaveChangesAsync();
    }
}
