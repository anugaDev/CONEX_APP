using System.Windows;
using CONEX_APP.Infrastructure.Data.Context;
using CONEX_APP.Infrastructure.Repositories;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.MainApplication.UseCases.Registrations;
using CONEX_APP.MainApplication.UseCases.Renewals;
using CONEX_APP.MainApplication.UseCases.Settings;
using CONEX_APP.Presentation.ViewModels;
using CONEX_APP.Presentation.ViewModels.Classes;
using CONEX_APP.Presentation.ViewModels.Reports;
using CONEX_APP.Presentation.ViewModels.Users;
using CONEX_APP.Presentation.ViewModels.Activities;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.Presentation.Views.Settings;

namespace CONEX_APP.Presentation.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private readonly AppDbContext _dbContext;
    private readonly UserRepository _userRepository;
    private readonly RenewalRepository _renewalRepository;
    private readonly AppSettingsRepository _appSettingsRepository;
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly UpdateUserUseCase _updateUserUseCase;
    private readonly DeleteUserUseCase _deleteUserUseCase;
    private readonly GetActivityUseCase _getActivityUseCase;
    private readonly CreateActivityUseCase _createActivityUseCase;
    private readonly UpdateActivityUseCase _updateActivityUseCase;
    private readonly DeleteActivityUseCase _deleteActivityUseCase;
    private readonly RemoveUserFromActivityUseCase _removeUserFromActivityUseCase;
    private readonly RegisterRenewalUseCase _registerRenewalUseCase;
    private readonly GetAppSettingsUseCase _getAppSettingsUseCase;
    private readonly SaveAppSettingsUseCase _saveAppSettingsUseCase;

    public MainWindow()
    {
        InitializeComponent();

        _dbContext = new AppDbContext();

        // Crear las tablas si no existen (compatible con BD existentes, sin necesidad de dotnet ef migrate)
        Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(_dbContext.Database, @"
            CREATE TABLE IF NOT EXISTS Activities (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Tutor TEXT NOT NULL,
                Classroom TEXT NOT NULL,
                Date TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
        ");

        Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(_dbContext.Database, @"
            CREATE TABLE IF NOT EXISTS Renewals (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId   INTEGER NOT NULL,
                RenewalDate TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
            );
        ");

        Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(_dbContext.Database, @"
            CREATE TABLE IF NOT EXISTS AppSettings (
                Id                  INTEGER PRIMARY KEY,
                RenewalPeriodMonths INTEGER NOT NULL DEFAULT 12,
                RenewalCost         TEXT    NOT NULL DEFAULT '0',
                ClassCost           TEXT    NOT NULL DEFAULT '0'
            );
        ");

        // Insertar configuración por defecto si no existe ningún registro
        Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(_dbContext.Database, @"
            INSERT OR IGNORE INTO AppSettings (Id, RenewalPeriodMonths, RenewalCost, ClassCost)
            VALUES (1, 12, '0', '0');
        ");

        _userRepository = new UserRepository(_dbContext);
        _renewalRepository = new RenewalRepository(_dbContext);
        _appSettingsRepository = new AppSettingsRepository(_dbContext);

        _registerRenewalUseCase = new RegisterRenewalUseCase(_renewalRepository);
        _getAppSettingsUseCase = new GetAppSettingsUseCase(_appSettingsRepository);
        _saveAppSettingsUseCase = new SaveAppSettingsUseCase(_appSettingsRepository);

        _getUsersUseCase = new GetUsersUseCase(_userRepository, _renewalRepository, _appSettingsRepository);
        _createUserUseCase = new CreateUserUseCase(_userRepository);
        _updateUserUseCase = new UpdateUserUseCase(_userRepository);
        _deleteUserUseCase = new DeleteUserUseCase(_userRepository);
        _removeUserFromActivityUseCase = new RemoveUserFromActivityUseCase(_userRepository);

        ActivityRepository activityRepository = new ActivityRepository(_dbContext);
        _getActivityUseCase = new GetActivityUseCase(activityRepository);
        _createActivityUseCase = new CreateActivityUseCase(activityRepository);
        _updateActivityUseCase = new UpdateActivityUseCase(activityRepository);
        _deleteActivityUseCase = new DeleteActivityUseCase(activityRepository);

        _mainViewModel = new MainViewModel();
        DataContext = _mainViewModel;

        NavigateToHome();
    }

    private void NavigateToHome()
    {
        _mainViewModel.CurrentViewModel = new HomeViewModel(
            navigateToUsers: NavigateToUsers,
            navigateToClasses: NavigateToClasses,
            navigateToReports: NavigateToReports,
            exitAction: () => this.Close(),
            openSettings: OpenSettingsFromHome
        );
    }

    private void OpenSettingsFromHome()
    {
        SettingsWindow settingsWindow = SettingsWindow.Create(_getAppSettingsUseCase, _saveAppSettingsUseCase);
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void NavigateToReports()
    {
        _mainViewModel.CurrentViewModel = new ReportsViewModel(
            _getUsersUseCase,
            _getActivityUseCase,
            goBack: NavigateToHome
        );
    }

    private void NavigateToUsers()
    {
        _mainViewModel.CurrentViewModel = new UserListViewModel(
            _getUsersUseCase, 
            _createUserUseCase, 
            _updateUserUseCase,
            _deleteUserUseCase,
            _getActivityUseCase,
            _removeUserFromActivityUseCase,
            _registerRenewalUseCase,
            _getAppSettingsUseCase,
            _saveAppSettingsUseCase,
            goBack: NavigateToHome,
            goToActivities: NavigateToClasses
        );
    }

    private void NavigateToClasses()
    {
        _mainViewModel.CurrentViewModel = new ActivityScheduleViewModel(
            _getActivityUseCase,
            _createActivityUseCase,
            _updateActivityUseCase,
            _deleteActivityUseCase,
            _getUsersUseCase,
            _removeUserFromActivityUseCase,
            goBack: NavigateToHome,
            goToUsers: NavigateToUsers
        );
    }
}
