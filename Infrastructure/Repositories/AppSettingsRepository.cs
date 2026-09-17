using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Repositories;

public class AppSettingsRepository : IAppSettingsRepository
{
    private readonly AppDbContext _context;

    public AppSettingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppSettings> GetAsync()
    {
        // Siempre devolvemos el único registro con Id = 1.
        // Si por algún motivo no existiera, lo creamos aquí.
        AppSettings? settings = await _context.AppSettings.FindAsync(1);
        if (settings == null)
        {
            settings = new AppSettings { Id = 1, RenewalPeriodMonths = 12, RenewalCost = 0, ClassCost = 0 };
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task SaveAsync(AppSettings settings)
    {
        AppSettings? existing = await _context.AppSettings.FindAsync(settings.Id);
        if (existing == null)
        {
            _context.AppSettings.Add(settings);
        }
        else
        {
            existing.RenewalPeriodMonths = settings.RenewalPeriodMonths;
            existing.RenewalCost = settings.RenewalCost;
            existing.ClassCost = settings.ClassCost;
        }
        await _context.SaveChangesAsync();
    }
}
