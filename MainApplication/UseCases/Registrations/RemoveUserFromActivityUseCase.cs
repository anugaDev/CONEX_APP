using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Registrations;

public class RemoveUserFromActivityUseCase
{
    private readonly IUserRepository _userRepository;

    public RemoveUserFromActivityUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ExecuteAsync(int userId, int activityId)
    {
        IEnumerable<User> users = await _userRepository.GetAllAsync();
        User? user = users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
            throw new InvalidOperationException($"Usuario con Id {userId} no encontrado.");

        List<int> remainingActivityIds = user.Activities
            .Where(a => a.Id != activityId)
            .Select(a => a.Id)
            .ToList();

        await _userRepository.UpdateAsync(user, remainingActivityIds);
    }
}
