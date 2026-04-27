using CONEX_APP.Domain.Entities;

namespace CONEX_APP.Domain.Interfaces;

public interface IActivityRepository
{
    Task<IEnumerable<Activity>> GetAllAsync();
    Task AddAsync(Activity activity);
    Task UpdateAsync(Activity activity);
    Task DeleteAsync(int id);
}