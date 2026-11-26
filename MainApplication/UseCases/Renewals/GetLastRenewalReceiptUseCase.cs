using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.Mapping;

namespace CONEX_APP.MainApplication.UseCases.Renewals;

public class GetLastRenewalReceiptUseCase
{
    private readonly IRenewalRepository _renewalRepository;
    private readonly IAppSettingsRepository _appSettingsRepository;

    public GetLastRenewalReceiptUseCase(IRenewalRepository renewalRepository, IAppSettingsRepository appSettingsRepository)
    {
        _renewalRepository = renewalRepository;
        _appSettingsRepository = appSettingsRepository;
    }

    public async Task<RenewalReceiptDto?> ExecuteAsync(int userId)
    {
        Renewal? renewal = await _renewalRepository.GetLastRenewalAsync(userId);
        if (renewal == null)
            return null;

        AppSettings settings = await _appSettingsRepository.GetAsync();
        return RenewalReceiptMapper.ToDto(renewal, settings);
    }
}
