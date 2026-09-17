using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;

namespace CONEX_APP.MainApplication.UseCases.Settings;

public class SaveAppSettingsUseCase
{
    private readonly IAppSettingsRepository _repository;

    public SaveAppSettingsUseCase(IAppSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(AppSettingsDto dto)
    {
        AppSettings settings = new AppSettings
        {
            Id = 1,
            RenewalPeriodMonths = dto.RenewalPeriodMonths,
            RenewalCost = dto.RenewalCost,
            ClassCost = dto.ClassCost
        };

        await _repository.SaveAsync(settings);
    }
}
