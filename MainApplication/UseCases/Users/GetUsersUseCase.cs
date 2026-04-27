using CONEX_APP.Domain.Entities;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class GetUsersUseCase
{
    private readonly IUserRepository _userRepository;

    public GetUsersUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> ExecuteAsync()
    {
        IEnumerable<User> users = await _userRepository.GetAllAsync();

        return users.Select(u => new UserDto 
        {
            Id = u.Id,
            Name = u.Name,
            Surname = u.Surname,
            SecondSurname = u.SecondSurname,
            IdCard = u.IdCard,
            Address = u.Address,
            Location = u.Location,
            Phone = u.Phone,
            Email = u.Email,
            IsPartner = u.IsPartner,
            IsTutor = u.IsTutor
        });
    }
}
