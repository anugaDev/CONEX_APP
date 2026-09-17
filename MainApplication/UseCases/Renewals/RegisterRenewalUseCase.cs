using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Renewals;

public class RegisterRenewalUseCase
{
    private readonly IRenewalRepository _renewalRepository;

    public RegisterRenewalUseCase(IRenewalRepository renewalRepository)
    {
        _renewalRepository = renewalRepository;
    }

    public async Task ExecuteAsync(int userId)
    {
        Renewal renewal = new Renewal
        {
            UserId = userId,
            RenewalDate = DateTime.Today
        };

        await _renewalRepository.AddAsync(renewal);
    }
}
