using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContextFactory _contextFactory;

    public UserRepository(AppDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Users.AsNoTracking().Include(u => u.Activities).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using AppDbContext context = _contextFactory.Create();
        return await context.Users.AsNoTracking().Include(u => u.Activities).FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsAsync(string idCard, string name, string surname, string secondSurname, int? excludeId = null)
    {
        await using AppDbContext context = _contextFactory.Create();
        IQueryable<User> query = context.Users.AsQueryable();

        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);

        if (!string.IsNullOrWhiteSpace(idCard))
        {
            string normalizedIdCard = idCard.Trim().ToLower();
            bool existsByIdCard = await query.AnyAsync(u => u.IdCard.Trim().ToLower() == normalizedIdCard);
            if (existsByIdCard) return true;
        }

        return await query.AnyAsync(u =>
            u.Name.ToLower() == name.ToLower() &&
            u.Surname.ToLower() == surname.ToLower() &&
            u.SecondSurname.ToLower() == secondSurname.ToLower());
    }

    public async Task<int> AddAsync(User user, IEnumerable<int> activityIds)
    {
        await using AppDbContext context = _contextFactory.Create();

        List<int> ids = activityIds.Distinct().ToList();
        user.Activities = await context.Activities.Where(a => ids.Contains(a.Id)).ToListAsync();

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    public async Task UpdateAsync(User user, IEnumerable<int> activityIds)
    {
        await using AppDbContext context = _contextFactory.Create();

        User trackedUser = await context.Users.Include(u => u.Activities).FirstOrDefaultAsync(u => u.Id == user.Id)
            ?? throw new InvalidOperationException($"Usuario con Id {user.Id} no encontrado.");

        trackedUser.Name = user.Name;
        trackedUser.Surname = user.Surname;
        trackedUser.SecondSurname = user.SecondSurname;
        trackedUser.IdCard = user.IdCard;
        trackedUser.Address = user.Address;
        trackedUser.Location = user.Location;
        trackedUser.Email = user.Email;
        trackedUser.Phone = user.Phone;
        trackedUser.IsPartner = user.IsPartner;
        trackedUser.IsTutor = user.IsTutor;

        HashSet<int> wantedIds = activityIds.ToHashSet();
        HashSet<int> currentIds = trackedUser.Activities.Select(a => a.Id).ToHashSet();

        foreach (Activity removed in trackedUser.Activities.Where(a => !wantedIds.Contains(a.Id)).ToList())
            trackedUser.Activities.Remove(removed);

        List<int> addedIds = wantedIds.Where(id => !currentIds.Contains(id)).ToList();
        if (addedIds.Count > 0)
        {
            List<Activity> added = await context.Activities.Where(a => addedIds.Contains(a.Id)).ToListAsync();
            foreach (Activity activity in added)
                trackedUser.Activities.Add(activity);

            // Quien entra en una clase deja de estar en su lista de espera
            List<Waitlist> satisfied = await context.Waitlists
                .Where(w => w.UserId == user.Id && addedIds.Contains(w.ActivityId))
                .ToListAsync();
            context.Waitlists.RemoveRange(satisfied);
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using AppDbContext context = _contextFactory.Create();
        User? user = await context.Users.FindAsync(id);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }
}
