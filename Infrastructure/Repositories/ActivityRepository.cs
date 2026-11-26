using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly AppDbContextFactory _contextFactory;

    public ActivityRepository(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Activity>> GetAllAsync()
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Activities.AsNoTracking().Include(a => a.Students).ToListAsync();
    }

    public async Task<Activity?> GetByIdAsync(int id)
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Activities.AsNoTracking().Include(a => a.Students).FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Activity>> GetByIdsAsync(IEnumerable<int> ids)
    {
        List<int> idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        await using AppDbContext context = _contextFactory.Create();
        return await context.Activities.AsNoTracking()
            .Include(a => a.Students)
            .Where(a => idList.Contains(a.Id))
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string name, int? excludeId = null)
    {
        await using AppDbContext context = _contextFactory.Create();
        IQueryable<Activity> query = context.Activities.AsQueryable();

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync(a => a.Name.ToLower() == name.ToLower());
    }

    public async Task AddAsync(Activity activity)
    {
        await using AppDbContext context = _contextFactory.Create();
        context.Activities.Add(activity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Activity activity)
    {
        await using AppDbContext context = _contextFactory.Create();
        Activity existing = await context.Activities.FindAsync(activity.Id)
            ?? throw new InvalidOperationException($"Clase con Id {activity.Id} no encontrada.");

        existing.Name = activity.Name;
        existing.Tutor = activity.Tutor;
        existing.Classroom = activity.Classroom;
        existing.Date = activity.Date;
        existing.MaxStudents = activity.MaxStudents;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using AppDbContext context = _contextFactory.Create();
        Activity? activity = await context.Activities.FindAsync(id);
        if (activity != null)
        {
            context.Activities.Remove(activity);
            await context.SaveChangesAsync();
        }
    }

    public async Task EnrollAsync(int userId, int activityId)
    {
        await using AppDbContext context = _contextFactory.Create();

        Activity activity = await context.Activities.Include(a => a.Students).FirstOrDefaultAsync(a => a.Id == activityId)
            ?? throw new InvalidOperationException($"Clase con Id {activityId} no encontrada.");

        if (activity.Students.All(s => s.Id != userId))
        {
            User user = await context.Users.FindAsync(userId)
                ?? throw new InvalidOperationException($"Usuario con Id {userId} no encontrado.");
            activity.Students.Add(user);
        }

        List<Waitlist> entries = await context.Waitlists
            .Where(w => w.UserId == userId && w.ActivityId == activityId)
            .ToListAsync();
        context.Waitlists.RemoveRange(entries);

        // Un único SaveChanges es una única transacción: o se hacen las dos cosas o ninguna
        await context.SaveChangesAsync();
    }

    public async Task UnenrollAsync(int userId, int activityId)
    {
        await using AppDbContext context = _contextFactory.Create();

        Activity? activity = await context.Activities.Include(a => a.Students).FirstOrDefaultAsync(a => a.Id == activityId);
        User? student = activity?.Students.FirstOrDefault(s => s.Id == userId);
        if (activity == null || student == null)
            return;

        activity.Students.Remove(student);
        await context.SaveChangesAsync();
    }
}
