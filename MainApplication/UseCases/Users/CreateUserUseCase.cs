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
        var user = new User
        {
            Name = dto.FullName,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
    }
}
