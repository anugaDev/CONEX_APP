using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Waitlists;

public class RemoveFromWaitlistUseCase
{
    private readonly IWaitlistRepository _waitlistRepository;

    public RemoveFromWaitlistUseCase(IWaitlistRepository waitlistRepository)
    {
        _waitlistRepository = waitlistRepository;
    }

    public async Task ExecuteAsync(int userId, int activityId)
    {
        await _waitlistRepository.RemoveAsync(userId, activityId);
    }
}
