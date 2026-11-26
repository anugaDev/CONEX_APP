using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.Mapping;

namespace CONEX_APP.MainApplication.UseCases.Renewals;

public class RegisterRenewalUseCase
{
    private readonly IRenewalRepository _renewalRepository;
    private readonly IAppSettingsRepository _appSettingsRepository;
    private readonly IActivityRepository _activityRepository;

    public RegisterRenewalUseCase(
        IRenewalRepository renewalRepository,
        IAppSettingsRepository appSettingsRepository,
        IActivityRepository activityRepository)
    {
        _renewalRepository = renewalRepository;
        _appSettingsRepository = appSettingsRepository;
        _activityRepository = activityRepository;
    }

    public async Task<RenewalReceiptDto> ExecuteAsync(int userId, IEnumerable<int> activityIds)
    {
        AppSettings settings = await _appSettingsRepository.GetAsync();
        IEnumerable<Activity> activities = await _activityRepository.GetByIdsAsync(activityIds);
        DateTime today = DateTime.Today;

        Renewal renewal = new Renewal
        {
            UserId = userId,
            RenewalDate = today,
            NextRenewalDate = today.AddMonths(settings.RenewalPeriodMonths),
            RenewalAmount = settings.RenewalCost,
            Classes = activities
                .OrderBy(a => a.Name)
                .Select(a => new RenewalClass
                {
                    ActivityName = a.Name,
                    Tutor = a.Tutor,
                    Schedule = ActivityMapper.ToDto(a, 0).Schedule,
                    Price = settings.ClassCost
                })
                .ToList()
        };

        await _renewalRepository.AddAsync(renewal);

        return RenewalReceiptMapper.ToDto(renewal, settings);
    }
}
