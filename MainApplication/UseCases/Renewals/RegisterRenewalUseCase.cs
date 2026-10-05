using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Renewals;

public class RegisterRenewalUseCase
{
    private readonly IRenewalRepository _renewalRepository;
    private readonly IAppSettingsRepository _appSettingsRepository;

    public RegisterRenewalUseCase(IRenewalRepository renewalRepository, IAppSettingsRepository appSettingsRepository)
    {
        _renewalRepository = renewalRepository;
        _appSettingsRepository = appSettingsRepository;
    }

    public async Task ExecuteAsync(int userId)
    {
        AppSettings settings = await _appSettingsRepository.GetAsync();
        DateTime today = DateTime.Today;

        Renewal renewal = new Renewal
        {
            UserId = userId,
            RenewalDate = today,
            NextRenewalDate = today.AddMonths(settings.RenewalPeriodMonths)
        };

        await _renewalRepository.AddAsync(renewal);
    }
}
