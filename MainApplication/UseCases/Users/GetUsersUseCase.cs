using CONEX_APP.Domain.Entities;
using CONEX_APP.Domain.Enums;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Domain.Interfaces;

namespace CONEX_APP.MainApplication.UseCases.Users;

public class GetUsersUseCase
{
    private const int ExpiringSoonDays = 30;

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

        // Última renovación por usuario, calculada en una sola pasada
        Dictionary<int, Renewal> lastRenewalByUser = allRenewals
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.MaxBy(r => r.RenewalDate)!);

        DateTime today = DateTime.Today;

        return users.Select(u =>
        {
            Renewal? lastRenewal = lastRenewalByUser.GetValueOrDefault(u.Id);

            // Si nunca ha renovado, usamos CreatedAt como base.
            // Protección: si CreatedAt es MinValue (dato antiguo sin registrar), usamos hoy.
            DateTime baseDate = lastRenewal?.RenewalDate
                ?? (u.CreatedAt > DateTime.MinValue ? u.CreatedAt.Date : DateTime.Today);
            // Si la renovación tiene fecha de vencimiento guardada (p.ej. "Venciment" importado de Conex), se respeta.
            DateTime nextRenewal = lastRenewal?.NextRenewalDate?.Date
                ?? baseDate.AddMonths(settings.RenewalPeriodMonths);

            RenewalStatus status = today > nextRenewal
                ? RenewalStatus.Expired
                : today >= nextRenewal.AddDays(-ExpiringSoonDays)
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
        }).ToList();
    }
}
