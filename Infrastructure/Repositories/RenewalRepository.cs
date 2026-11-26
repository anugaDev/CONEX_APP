using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Repositories;

public class RenewalRepository : IRenewalRepository
{
    private readonly AppDbContextFactory _contextFactory;

    public RenewalRepository(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Renewal?> GetLastRenewalAsync(int userId)
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Renewals.AsNoTracking()
            .Include(r => r.Classes)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RenewalDate).ThenByDescending(r => r.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Renewal>> GetAllAsync()
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Renewals.AsNoTracking()
            .OrderByDescending(r => r.RenewalDate)
            .ToListAsync();
    }

    public async Task AddAsync(Renewal renewal)
    {
        await using AppDbContext context = _contextFactory.Create();
        context.Renewals.Add(renewal);
        await context.SaveChangesAsync();
    }
}
