using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Repositories;

public class WaitlistRepository : IWaitlistRepository
{
    private readonly AppDbContextFactory _contextFactory;

    public WaitlistRepository(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Waitlist>> GetByActivityAsync(int activityId)
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Waitlists.AsNoTracking()
            .Include(w => w.User)
            .Where(w => w.ActivityId == activityId)
            .OrderBy(w => w.JoinedAt).ThenBy(w => w.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Waitlist>> GetByUserAsync(int userId)
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Waitlists.AsNoTracking()
            .Include(w => w.Activity)
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.JoinedAt).ThenBy(w => w.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Waitlist>> GetAllAsync()
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Waitlists.AsNoTracking()
            .Include(w => w.User)
            .Include(w => w.Activity)
            .OrderBy(w => w.JoinedAt).ThenBy(w => w.Id)
            .ToListAsync();
    }

    public async Task<Dictionary<int, int>> GetCountsByActivityAsync()
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Waitlists
            .GroupBy(w => w.ActivityId)
            .Select(g => new { ActivityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ActivityId, x => x.Count);
    }

    public async Task<bool> IsUserInWaitlistAsync(int userId, int activityId)
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Waitlists.AnyAsync(w => w.UserId == userId && w.ActivityId == activityId);
    }

    public async Task AddAsync(Waitlist entry)
    {
        await using AppDbContext context = _contextFactory.Create();
        context.Waitlists.Add(entry);
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int userId, int activityId)
    {
        await using AppDbContext context = _contextFactory.Create();
        List<Waitlist> entries = await context.Waitlists
            .Where(w => w.UserId == userId && w.ActivityId == activityId)
            .ToListAsync();

        if (entries.Count > 0)
        {
            context.Waitlists.RemoveRange(entries);
            await context.SaveChangesAsync();
        }
    }
}
