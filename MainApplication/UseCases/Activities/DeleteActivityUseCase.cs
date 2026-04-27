using System.Threading.Tasks;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Activities;

public class DeleteActivityUseCase
{
    private readonly IActivityRepository _activityRepository;

    public DeleteActivityUseCase(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task ExecuteAsync(int id)
    {
        await _activityRepository.DeleteAsync(id);
    }
}
