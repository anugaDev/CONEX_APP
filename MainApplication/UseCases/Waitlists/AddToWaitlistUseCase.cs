using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Waitlists;

public class AddToWaitlistUseCase
{
    private readonly IWaitlistRepository _waitlistRepository;

    public AddToWaitlistUseCase(IWaitlistRepository waitlistRepository)
    {
        _waitlistRepository = waitlistRepository;
    }

    public async Task<bool> ExecuteAsync(int userId, int activityId)
    {
        bool alreadyInWaitlist = await _waitlistRepository.IsUserInWaitlistAsync(userId, activityId);
        if (alreadyInWaitlist)
            return false;

        Waitlist entry = new Waitlist
        {
            UserId = userId,
            ActivityId = activityId,
            JoinedAt = DateTime.Now
        };

        await _waitlistRepository.AddAsync(entry);
        return true;
    }
}
