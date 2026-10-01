using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Repositories;

public class WaitlistRepository : IWaitlistRepository
{
    private readonly AppDbContext _context;

    public WaitlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Waitlist>> GetByActivityAsync(int activityId)
    {
        return await _context.Waitlists
            .Include(w => w.User)
            .Where(w => w.ActivityId == activityId)
            .OrderBy(w => w.JoinedAt)
            .ToListAsync();
    }

    public async Task<bool> IsUserInWaitlistAsync(int userId, int activityId)
    {
        return await _context.Waitlists
            .AnyAsync(w => w.UserId == userId && w.ActivityId == activityId);
    }

    public async Task AddAsync(Waitlist entry)
    {
        _context.Waitlists.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int userId, int activityId)
    {
        Waitlist? entry = await _context.Waitlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ActivityId == activityId);

        if (entry != null)
        {
            _context.Waitlists.Remove(entry);
            await _context.SaveChangesAsync();
        }
    }
}
