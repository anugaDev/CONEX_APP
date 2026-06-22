using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Exceptions;
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
        bool exists = await _activityRepository.ExistsAsync(dto.Name);
        if (exists)
            throw new DuplicateEntityException(
                $"Ya existe una actividad con el nombre \"{dto.Name}\".");

        Activity activity = new Activity()
        {
            Name = dto.Name,
            Tutor = dto.Tutor,
            Classroom = dto.Classroom,
            Date = dto.Date,
            MaxStudents = dto.MaxStudents,
            CreatedAt = DateTime.UtcNow,
        };

        await _activityRepository.AddAsync(activity);
    }
}
