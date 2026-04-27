using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CONEX_APP.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.Include(u => u.Activities).ToListAsync();
    }

    public async Task AddAsync(User user, IEnumerable<int>? activityIds = null)
    {
        if (activityIds != null && activityIds.Any())
        {
            var activities = await _context.Activities.Where(a => activityIds.Contains(a.Id)).ToListAsync();
            user.Activities = activities;
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user, IEnumerable<int>? activityIds = null)
    {
        // Limpiamos las actividades actuales y metemos las nuevas si hay
        if (activityIds != null)
        {
            var trackedUser = await _context.Users.Include(u => u.Activities).FirstOrDefaultAsync(u => u.Id == user.Id);
            if (trackedUser != null)
            {
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

                var activities = await _context.Activities.Where(a => activityIds.Contains(a.Id)).ToListAsync();
                trackedUser.Activities = activities;
                
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}