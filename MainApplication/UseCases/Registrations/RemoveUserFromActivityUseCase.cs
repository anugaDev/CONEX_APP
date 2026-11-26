using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Registrations;

public class RemoveUserFromActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public RemoveUserFromActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task ExecuteAsync(int userId, int activityId)
    {
        await _activityRepository.UnenrollAsync(userId, activityId);
    }
}
