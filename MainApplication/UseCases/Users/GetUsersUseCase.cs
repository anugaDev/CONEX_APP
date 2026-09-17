using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Enums;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class GetUsersUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IRenewalRepository _renewalRepository;
    private readonly IAppSettingsRepository _appSettingsRepository;

    public GetUsersUseCase(IUserRepository userRepository, IRenewalRepository renewalRepository, IAppSettingsRepository appSettingsRepository)
    {
        _userRepository = userRepository;
        _renewalRepository = renewalRepository;
        _appSettingsRepository = appSettingsRepository;
    }

    public async Task<IEnumerable<UserDto>> ExecuteAsync()
    {
        IEnumerable<User> users = await _userRepository.GetAllAsync();
        AppSettings settings = await _appSettingsRepository.GetAsync();
        IEnumerable<Renewal> allRenewals = await _renewalRepository.GetAllAsync();

        // Indexamos la última renovación por usuario para evitar N+1 queries
        Dictionary<int, Renewal?> lastRenewalByUser = users.ToDictionary(
            u => u.Id,
            u => allRenewals.Where(r => r.UserId == u.Id).MaxBy(r => r.RenewalDate));

        DateTime today = DateTime.Today;

        return users.Select(u =>
        {
            Renewal? lastRenewal = lastRenewalByUser[u.Id];

            // Si nunca ha renovado, usamos CreatedAt como base.
            // Protección: si CreatedAt es MinValue (dato antiguo sin registrar), usamos hoy.
            DateTime baseDate = lastRenewal?.RenewalDate
                ?? (u.CreatedAt > DateTime.MinValue ? u.CreatedAt.Date : DateTime.Today);
            DateTime nextRenewal = baseDate.AddMonths(settings.RenewalPeriodMonths);

            RenewalStatus status = today > nextRenewal
                ? RenewalStatus.Expired
                : today >= nextRenewal.AddDays(-30)
                    ? RenewalStatus.ExpiringSoon
                    : RenewalStatus.Active;

            return new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Surname = u.Surname,
                SecondSurname = u.SecondSurname,
                IdCard = u.IdCard,
                Address = u.Address,
                Location = u.Location,
                Phone = u.Phone,
                Email = u.Email,
                IsPartner = u.IsPartner,
                IsTutor = u.IsTutor,
                EnrolledActivityIds = u.Activities.Select(a => a.Id).ToList(),
                CreatedAt = u.CreatedAt,
                DischargedAt = u.DischargedAt,
                LastRenewalDate = lastRenewal?.RenewalDate,
                NextRenewalDate = nextRenewal,
                RenewalStatus = status
            };
        });
    }
}
