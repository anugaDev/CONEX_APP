using CONEX_APP.Domain.Entities;

namespace CONEX_APP.Domain.Interfaces;

public interface IWaitlistRepository
{
    Task<IEnumerable<Waitlist>> GetByActivityAsync(int activityId);

    Task<IEnumerable<Waitlist>> GetByUserAsync(int userId);

    Task<IEnumerable<Waitlist>> GetAllAsync();

    Task<Dictionary<int, int>> GetCountsByActivityAsync();

    Task<bool> IsUserInWaitlistAsync(int userId, int activityId);

    Task AddAsync(Waitlist entry);

    Task RemoveAsync(int userId, int activityId);
}
