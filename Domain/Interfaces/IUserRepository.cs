using CONEX_APP.Domain.Entities;

namespace CONEX_APP.Domain.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
}