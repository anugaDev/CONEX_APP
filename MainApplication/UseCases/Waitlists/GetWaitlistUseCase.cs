using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.Mapping;

namespace CONEX_APP.MainApplication.UseCases.Waitlists;

public class GetWaitlistUseCase
{
    private readonly IWaitlistRepository _waitlistRepository;

    public GetWaitlistUseCase(IWaitlistRepository waitlistRepository)
    {
        _waitlistRepository = waitlistRepository;
    }

    public async Task<List<WaitlistEntryDto>> ExecuteAsync(int activityId)
    {
        IEnumerable<Waitlist> entries = await _waitlistRepository.GetByActivityAsync(activityId);
        return ToDtos(entries);
    }

    public async Task<List<UserWaitlistEntryDto>> ExecuteForUserAsync(int userId)
    {
        IEnumerable<Waitlist> all = await _waitlistRepository.GetAllAsync();

        return all
            .GroupBy(w => w.ActivityId)
            .SelectMany(g => g.Select((entry, index) => (entry, position: index + 1)))
            .Where(x => x.entry.UserId == userId)
            .Select(x => new UserWaitlistEntryDto
            {
                ActivityId = x.entry.ActivityId,
                ActivityName = x.entry.Activity.Name,
                JoinedAt = x.entry.JoinedAt,
                Position = x.position
            })
            .OrderBy(e => e.ActivityName)
            .ToList();
    }

    internal static List<WaitlistEntryDto> ToDtos(IEnumerable<Waitlist> orderedEntries) =>
        orderedEntries
            .Select((entry, index) => new WaitlistEntryDto
            {
                UserId = entry.UserId,
                FullName = ActivityMapper.FullName(entry.User),
                JoinedAt = entry.JoinedAt,
                Position = index + 1
            })
            .ToList();
}
