using CONEX_APP.Infrastructure.Data.Context;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Infrastructure.Repositories;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Registrations;
using CONEX_APP.MainApplication.UseCases.Renewals;
using CONEX_APP.MainApplication.UseCases.Settings;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.MainApplication.UseCases.Waitlists;
using CONEX_APP.Presentation.ViewModels.Activities;
using CONEX_APP.Presentation.ViewModels.Users;

namespace CONEX_APP.Presentation.Services;

public class AppServices
{
    // Usuarios
    public GetUsersUseCase GetUsers { get; }
    public CreateUserUseCase CreateUser { get; }
    public UpdateUserUseCase UpdateUser { get; }
    public DeleteUserUseCase DeleteUser { get; }

    // Clases e inscripciones
    public GetActivityUseCase GetActivities { get; }
    public CreateActivityUseCase CreateActivity { get; }
    public UpdateActivityUseCase UpdateActivity { get; }
    public DeleteActivityUseCase DeleteActivity { get; }
    public EnrollUserUseCase EnrollUser { get; }
    public RemoveUserFromActivityUseCase RemoveUserFromActivity { get; }

    // Listas de espera
    public GetWaitlistUseCase GetWaitlist { get; }
    public AddToWaitlistUseCase AddToWaitlist { get; }
    public RemoveFromWaitlistUseCase RemoveFromWaitlist { get; }
    public GetWaitlistReportUseCase GetWaitlistReport { get; }
    public WaitlistPromotionService WaitlistPromotion { get; }

    // Renovaciones y ajustes
    public RegisterRenewalUseCase RegisterRenewal { get; }
    public GetLastRenewalReceiptUseCase GetLastRenewalReceipt { get; }
    public GetAppSettingsUseCase GetAppSettings { get; }
    public SaveAppSettingsUseCase SaveAppSettings { get; }

    public AppServices(AppDbContextFactory contextFactory)
    {
        UserRepository userRepository = new UserRepository(contextFactory);
        ActivityRepository activityRepository = new ActivityRepository(contextFactory);
        WaitlistRepository waitlistRepository = new WaitlistRepository(contextFactory);
        RenewalRepository renewalRepository = new RenewalRepository(contextFactory);
        AppSettingsRepository appSettingsRepository = new AppSettingsRepository(contextFactory);

        GetUsers = new GetUsersUseCase(userRepository, renewalRepository, appSettingsRepository);
        CreateUser = new CreateUserUseCase(userRepository, activityRepository);
        UpdateUser = new UpdateUserUseCase(userRepository, activityRepository);
        DeleteUser = new DeleteUserUseCase(userRepository);

        GetActivities = new GetActivityUseCase(activityRepository, waitlistRepository);
        CreateActivity = new CreateActivityUseCase(activityRepository);
        UpdateActivity = new UpdateActivityUseCase(activityRepository);
        DeleteActivity = new DeleteActivityUseCase(activityRepository);
        EnrollUser = new EnrollUserUseCase(activityRepository);
        RemoveUserFromActivity = new RemoveUserFromActivityUseCase(activityRepository);

        GetWaitlist = new GetWaitlistUseCase(waitlistRepository);
        AddToWaitlist = new AddToWaitlistUseCase(waitlistRepository, activityRepository);
        RemoveFromWaitlist = new RemoveFromWaitlistUseCase(waitlistRepository);
        GetWaitlistReport = new GetWaitlistReportUseCase(activityRepository, waitlistRepository);
        WaitlistPromotion = new WaitlistPromotionService(
            new GetPromotionCandidateUseCase(activityRepository, waitlistRepository),
            EnrollUser);

        RegisterRenewal = new RegisterRenewalUseCase(renewalRepository, appSettingsRepository, activityRepository);
        GetLastRenewalReceipt = new GetLastRenewalReceiptUseCase(renewalRepository, appSettingsRepository);
        GetAppSettings = new GetAppSettingsUseCase(appSettingsRepository);
        SaveAppSettings = new SaveAppSettingsUseCase(appSettingsRepository);
    }

    public AddUserViewModel CreateUserForm(UserDto? userToEdit = null) =>
        new(CreateUser, UpdateUser, GetActivities, GetWaitlist, AddToWaitlist, RemoveFromWaitlist, WaitlistPromotion, userToEdit);

    public AddActivityViewModel CreateActivityForm(ActivityScheduleDto? activityToEdit = null) =>
        new(CreateActivity, UpdateActivity, GetActivities, GetUsers, RemoveUserFromActivity, EnrollUser,
            AddToWaitlist, GetWaitlist, RemoveFromWaitlist, WaitlistPromotion, activityToEdit);
}
