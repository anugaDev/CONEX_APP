using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.UseCases.Activities;

public class CreateActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public CreateActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task ExecuteAsync(CreateActivityDto dto)
    {
        Activity user = new Activity()
        {
            Name = dto.Name,
            Tutor = dto.Tutor,
            Classroom = dto.Classroom,
            Date = dto.Date,
            CreatedAt = DateTime.UtcNow
        };

        await _activityRepository.AddAsync(user);
    }
}
