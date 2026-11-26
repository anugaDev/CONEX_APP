using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Registrations;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IActivityRepository _activityRepository;

    public CreateUserUseCase(IUserRepository userRepository, IActivityRepository activityRepository)
    {
        _userRepository = userRepository;
        _activityRepository = activityRepository;
    }

    public async Task<int> ExecuteAsync(CreateUserDto dto)
    {
        bool exists = await _userRepository.ExistsAsync(dto.IdCard, dto.Name, dto.Surname, dto.SecondSurname);
        if (exists)
            throw new DuplicateEntityException(
                $"Ya existe un usuario con el mismo DNI o nombre completo: {dto.Name} {dto.Surname} {dto.SecondSurname}".Trim());

        await EnrollmentRules.EnsureCapacityAsync(_activityRepository, dto.SelectedActivityIds);

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
            IsTutor = dto.IsTutor,
            CreatedAt = DateTime.UtcNow,
            DischargedAt = DateTime.MinValue
        };

        return await _userRepository.AddAsync(user, dto.SelectedActivityIds);
    }
}
