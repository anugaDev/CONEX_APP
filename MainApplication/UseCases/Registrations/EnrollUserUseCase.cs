using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Registrations;

public class EnrollUserUseCase
{
    private readonly IActivityRepository _activityRepository;

    public EnrollUserUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task ExecuteAsync(int userId, int activityId)
    {
        Activity activity = await _activityRepository.GetByIdAsync(activityId)
            ?? throw new DomainException("La clase ya no existe. Puede que se haya borrado.");

        bool alreadyEnrolled = activity.Students.Any(s => s.Id == userId);
        if (!alreadyEnrolled && activity.IsFull)
            throw new ActivityFullException(activity.Id, activity.Name);

        await _activityRepository.EnrollAsync(userId, activityId);
    }
}
