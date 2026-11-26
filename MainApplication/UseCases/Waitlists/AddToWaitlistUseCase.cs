using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Waitlists;

public class AddToWaitlistUseCase
{
    private readonly IWaitlistRepository _waitlistRepository;
    private readonly IActivityRepository _activityRepository;

    public AddToWaitlistUseCase(IWaitlistRepository waitlistRepository, IActivityRepository activityRepository)
    {
        _waitlistRepository = waitlistRepository;
        _activityRepository = activityRepository;
    }

    public async Task ExecuteAsync(int userId, int activityId)
    {
        Activity activity = await _activityRepository.GetByIdAsync(activityId)
            ?? throw new DomainException("La clase ya no existe. Puede que se haya borrado.");

        if (activity.Students.Any(s => s.Id == userId))
            throw new DomainException($"El usuario ya está inscrito en \"{activity.Name}\".");

        if (!activity.IsFull)
            throw new DomainException(
                $"La clase \"{activity.Name}\" tiene plazas libres: inscribe al usuario directamente en lugar de ponerlo en espera.");

        if (await _waitlistRepository.IsUserInWaitlistAsync(userId, activityId))
            throw new DomainException($"El usuario ya está en la lista de espera de \"{activity.Name}\".");

        await _waitlistRepository.AddAsync(new Waitlist
        {
            UserId = userId,
            ActivityId = activityId,
            JoinedAt = DateTime.Now
        });
    }
}
