using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Waitlists;

public class GetWaitlistUseCase
{
    private readonly IWaitlistRepository _waitlistRepository;

    public GetWaitlistUseCase(IWaitlistRepository waitlistRepository)
    {
        _waitlistRepository = waitlistRepository;
    }

    public async Task<IEnumerable<Waitlist>> ExecuteAsync(int activityId)
    {
        return await _waitlistRepository.GetByActivityAsync(activityId);
    }
}
