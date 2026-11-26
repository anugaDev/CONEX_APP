using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.Mapping;

namespace CONEX_APP.MainApplication.UseCases.Waitlists;

public class GetWaitlistReportUseCase
{
    private readonly IActivityRepository _activityRepository;
    private readonly IWaitlistRepository _waitlistRepository;

    public GetWaitlistReportUseCase(IActivityRepository activityRepository, IWaitlistRepository waitlistRepository)
    {
        _activityRepository = activityRepository;
        _waitlistRepository = waitlistRepository;
    }

    public async Task<List<ActivityWaitlistDto>> ExecuteAsync()
    {
        IEnumerable<Activity> activities = await _activityRepository.GetAllAsync();
        Dictionary<int, List<Waitlist>> waitlistByActivity = (await _waitlistRepository.GetAllAsync())
            .GroupBy(w => w.ActivityId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return activities
            .Where(a => waitlistByActivity.ContainsKey(a.Id))
            .OrderBy(a => a.Name)
            .Select(a =>
            {
                List<Waitlist> entries = waitlistByActivity[a.Id];
                return new ActivityWaitlistDto
                {
                    Activity = ActivityMapper.ToDto(a, entries.Count),
                    Entries = GetWaitlistUseCase.ToDtos(entries)
                };
            })
            .ToList();
    }
}
