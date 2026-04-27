using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;

    public CreateUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ExecuteAsync(CreateUserDto dto)
    {
        User user = new User
        {
            Name = dto.Name,
            Surname = dto.Surname,
            SecondSurname = dto.SecondSurname,
            IdCard = dto.IdCard,
            Address = dto.Address,
            Location = dto.Location,
            Phone = dto.Phone,
            Email = dto.Email,
            IsPartner = dto.IsPartner,
            CreatedAt = DateTime.UtcNow,
            DischargedAt = DateTime.MinValue
        };

        await _userRepository.AddAsync(user);
    }
}
