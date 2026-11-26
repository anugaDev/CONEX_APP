using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Registrations;

internal static class EnrollmentRules
{
    public static async Task EnsureCapacityAsync(IActivityRepository activityRepository, IEnumerable<int> newActivityIds)
    {
        IEnumerable<Activity> activities = await activityRepository.GetByIdsAsync(newActivityIds);

        Activity? full = activities.FirstOrDefault(a => a.IsFull);
        if (full != null)
            throw new ActivityFullException(full.Id, full.Name);
    }
}
