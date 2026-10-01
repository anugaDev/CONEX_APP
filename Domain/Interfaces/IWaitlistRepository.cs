using CONEX_APP.Domain.Entities;

namespace CONEX_APP.Domain.Interfaces;

public interface IWaitlistRepository
{
    Task<IEnumerable<Waitlist>> GetByActivityAsync(int activityId);
    Task<bool> IsUserInWaitlistAsync(int userId, int activityId);
    Task AddAsync(Waitlist entry);
    Task RemoveAsync(int userId, int activityId);
}
