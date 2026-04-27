using System.Windows;
using CONEX_APP.Infrastructure.Data.Context;
using CONEX_APP.Infrastructure.Repositories;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.ViewModels;
using CONEX_APP.Presentation.ViewModels.Classes;
using CONEX_APP.Presentation.ViewModels.Users;
using CONEX_APP.Presentation.ViewModels.Activities;
using CONEX_APP.MainApplication.UseCases.Activities;

namespace CONEX_APP.Presentation.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private readonly AppDbContext _dbContext;
    private readonly UserRepository _userRepository;
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly GetActivityUseCase _getActivityUseCase;
    private readonly CreateActivityUseCase _createActivityUseCase;

    public MainWindow()
    {
        InitializeComponent();

        _dbContext = new AppDbContext();

        // Parche de seguridad para crear la tabla si las migraciones fallan por entorno
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

        _userRepository = new UserRepository(_dbContext);
        _getUsersUseCase = new GetUsersUseCase(_userRepository);
        _createUserUseCase = new CreateUserUseCase(_userRepository);

        var activityRepository = new ActivityRepository(_dbContext);
        _getActivityUseCase = new GetActivityUseCase(activityRepository);
        _createActivityUseCase = new CreateActivityUseCase(activityRepository);

        _mainViewModel = new MainViewModel();
        DataContext = _mainViewModel;

        NavigateToHome();
    }

    private void NavigateToHome()
    {
        _mainViewModel.CurrentViewModel = new HomeViewModel(
            navigateToUsers: NavigateToUsers,
            navigateToClasses: NavigateToClasses,
            exitAction: () => this.Close()
        );
    }

    private void NavigateToUsers()
    {
        _mainViewModel.CurrentViewModel = new UserListViewModel(
            _getUsersUseCase, 
            _createUserUseCase, 
            _getActivityUseCase,
            goBack: NavigateToHome
        );
    }

    private void NavigateToClasses()
    {
        _mainViewModel.CurrentViewModel = new ActivityScheduleViewModel(
            _getActivityUseCase,
            _createActivityUseCase,
            _getUsersUseCase,
            goBack: NavigateToHome
        );
    }
}
