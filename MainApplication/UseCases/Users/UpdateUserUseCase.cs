using System.Threading.Tasks;
using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class UpdateUserUseCase
{
    private readonly IUserRepository _userRepository;

    public UpdateUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ExecuteAsync(UpdateUserDto dto)
    {
        User user = new User
        {
            Id = dto.Id,
            Name = dto.Name,
            Surname = dto.Surname,
            SecondSurname = dto.SecondSurname,
            IdCard = dto.IdCard,
            Address = dto.Address,
            Location = dto.Location,
            Phone = dto.Phone,
            Email = dto.Email,
            IsPartner = dto.IsPartner,
            IsTutor = dto.IsTutor
        };

        await _userRepository.UpdateAsync(user, dto.SelectedActivityIds);
    }
}
