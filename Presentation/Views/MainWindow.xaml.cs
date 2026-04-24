using System.Windows;
using CONEX_APP.Infrastructure.Data.Context;
using CONEX_APP.Infrastructure.Repositories;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.ViewModels;
using CONEX_APP.Presentation.ViewModels.Classes;
using CONEX_APP.Presentation.ViewModels.Users;

namespace CONEX_APP.Presentation.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private readonly AppDbContext _dbContext;
    private readonly UserRepository _userRepository;
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly CreateUserUseCase _createUserUseCase;

    public MainWindow()
    {
        InitializeComponent();

        _dbContext = new AppDbContext();
        _userRepository = new UserRepository(_dbContext);
        _getUsersUseCase = new GetUsersUseCase(_userRepository);
        _createUserUseCase = new CreateUserUseCase(_userRepository);

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
            goBack: NavigateToHome
        );
    }

    private void NavigateToClasses()
    {
        _mainViewModel.CurrentViewModel = new ClassListViewModel(
            goBack: NavigateToHome
        );
    }
}
