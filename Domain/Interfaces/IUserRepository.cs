using CONEX_APP.Domain.Entities;

namespace CONEX_APP.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<int> AddAsync(User user, IEnumerable<int> activityIds);

    Task UpdateAsync(User user, IEnumerable<int> activityIds);

    Task DeleteAsync(int id);

    Task<bool> ExistsAsync(string idCard, string name, string surname, string secondSurname, int? excludeId = null);
}
