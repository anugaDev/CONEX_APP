using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.Mapping;

namespace CONEX_APP.MainApplication.UseCases.Activities;

public class GetActivityUseCase
{
    private readonly IActivityRepository _activityRepository;
    private readonly IWaitlistRepository _waitlistRepository;

    public GetActivityUseCase(IActivityRepository activityRepository, IWaitlistRepository waitlistRepository)
    {
        _activityRepository = activityRepository;
        _waitlistRepository = waitlistRepository;
    }

    public async Task<IEnumerable<ActivityScheduleDto>> ExecuteAsync()
    {
        IEnumerable<Activity> activities = await _activityRepository.GetAllAsync();
        Dictionary<int, int> waitlistCounts = await _waitlistRepository.GetCountsByActivityAsync();

        return activities
            .Select(a => ActivityMapper.ToDto(a, waitlistCounts.GetValueOrDefault(a.Id)))
            .ToList();
    }

    public async Task<ActivityScheduleDto?> ExecuteByIdAsync(int activityId)
    {
        Activity? activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null)
            return null;

        Dictionary<int, int> waitlistCounts = await _waitlistRepository.GetCountsByActivityAsync();
        return ActivityMapper.ToDto(activity, waitlistCounts.GetValueOrDefault(activityId));
    }
}
