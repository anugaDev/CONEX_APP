using CONEX_APP.Domain.Entities;

namespace CONEX_APP.Domain.Interfaces;

public interface IRenewalRepository
{
    Task<Renewal?> GetLastRenewalAsync(int userId);
    Task<IEnumerable<Renewal>> GetAllAsync();
    Task AddAsync(Renewal renewal);
}
