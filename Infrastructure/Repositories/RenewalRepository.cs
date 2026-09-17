using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Repositories;

public class RenewalRepository : IRenewalRepository
{
    private readonly AppDbContext _context;

    public RenewalRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Renewal?> GetLastRenewalAsync(int userId)
    {
        return await _context.Renewals
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RenewalDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Renewal>> GetAllAsync()
    {
        return await _context.Renewals
            .OrderByDescending(r => r.RenewalDate)
            .ToListAsync();
    }

    public async Task AddAsync(Renewal renewal)
    {
        _context.Renewals.Add(renewal);
        await _context.SaveChangesAsync();
    }
}
