using CONEX_APP.Application.DTOs;
using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class GetActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public GetActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivityScheduleDto>> ExecuteAsync()
    {
        IEnumerable<Activity> activities = await _activityRepository.GetAllAsync();
        
        return activities.Select(activity => new ActivityScheduleDto 
        {
            Id = activity.Id,
            Name = activity.Name,
            Tutor = activity.Tutor,
            Date = activity.Date,
            Classroom = activity.Classroom
        });
    }
}
