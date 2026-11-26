using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.UseCases.Waitlists;

public class GetPromotionCandidateUseCase
{
    private readonly IActivityRepository _activityRepository;
    private readonly IWaitlistRepository _waitlistRepository;

    public GetPromotionCandidateUseCase(IActivityRepository activityRepository, IWaitlistRepository waitlistRepository)
    {
        _activityRepository = activityRepository;
        _waitlistRepository = waitlistRepository;
    }

    public async Task<PromotionCandidateDto?> ExecuteAsync(int activityId)
    {
        Activity? activity = await _activityRepository.GetByIdAsync(activityId);
        if (activity == null || activity.IsFull)
            return null;

        List<WaitlistEntryDto> waitlist = GetWaitlistUseCase.ToDtos(await _waitlistRepository.GetByActivityAsync(activityId));
        WaitlistEntryDto? first = waitlist.FirstOrDefault();
        if (first == null)
            return null;

        return new PromotionCandidateDto
        {
            ActivityId = activity.Id,
            ActivityName = activity.Name,
            UserId = first.UserId,
            UserFullName = first.FullName,
            RemainingInWaitlist = waitlist.Count - 1
        };
    }
}

public class PromotionCandidateDto
{
    public int ActivityId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;

    public int RemainingInWaitlist { get; set; }
}
