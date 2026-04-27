using CONEX_APP.Domain.Entities;

namespace CONEX_APP.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user, IEnumerable<int>? activityIds = null);
    Task UpdateAsync(User user, IEnumerable<int>? activityIds = null);
    Task DeleteAsync(int id);
}