using System.Threading.Tasks;
using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.UseCases.Activities;

public class UpdateActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public UpdateActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task ExecuteAsync(UpdateActivityDto dto)
    {
        Activity activity = new Activity
        {
            Id = dto.Id,
            Name = dto.Name,
            Tutor = dto.Tutor,
            Classroom = dto.Classroom,
            Date = dto.Date,
            MaxStudents = dto.MaxStudents
        };

        await _activityRepository.UpdateAsync(activity);
    }
}
