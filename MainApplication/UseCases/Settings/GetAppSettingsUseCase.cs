using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.UseCases.Settings;

public class GetAppSettingsUseCase
{
    private readonly IAppSettingsRepository _repository;

    public GetAppSettingsUseCase(IAppSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppSettingsDto> ExecuteAsync()
    {
        AppSettings settings = await _repository.GetAsync();
        return new AppSettingsDto
        {
            RenewalPeriodMonths = settings.RenewalPeriodMonths,
            RenewalCost = settings.RenewalCost,
            ClassCost = settings.ClassCost
        };
    }
}
