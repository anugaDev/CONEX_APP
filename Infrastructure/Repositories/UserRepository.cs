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

    public async Task<bool> ExistsAsync(string idCard, string name, string surname, string secondSurname, int? excludeId = null)
    {
        IQueryable<User> query = _context.Users.AsQueryable();

        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);

        // Si el DNI no está vacío, comprobar duplicado por DNI
        if (!string.IsNullOrWhiteSpace(idCard))
        {
            bool existsByIdCard = await query.AnyAsync(u =>
                !string.IsNullOrWhiteSpace(u.IdCard) &&
                u.IdCard.ToLower() == idCard.ToLower());
            if (existsByIdCard) return true;
        }

        // Comprobar duplicado por nombre completo (insensible a mayúsculas)
        return await query.AnyAsync(u =>
            u.Name.ToLower() == name.ToLower() &&
            u.Surname.ToLower() == surname.ToLower() &&
            u.SecondSurname.ToLower() == secondSurname.ToLower());
    }

    public async Task AddAsync(User user, IEnumerable<int>? activityIds = null)
    {
        if (activityIds != null && activityIds.Any())
        {
            List<Activity> activities = await _context.Activities.Where(a => activityIds.Contains(a.Id)).ToListAsync();
            user.Activities = activities;
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user, IEnumerable<int>? activityIds = null)
    {
        if (activityIds != null)
        {
            User? trackedUser = await _context.Users.Include(u => u.Activities).FirstOrDefaultAsync(u => u.Id == user.Id);
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

                List<Activity> activities = await _context.Activities.Where(a => activityIds.Contains(a.Id)).ToListAsync();
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
        User? user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}