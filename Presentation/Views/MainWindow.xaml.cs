using System.Windows;
using CONEX_APP.Infrastructure.Data.Context;
using CONEX_APP.Infrastructure.Repositories;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.ViewModels.Users;

namespace CONEX_APP.Presentation.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Configuración manual de Inyección de Dependencias (DI)
        AppDbContext dbContext = new AppDbContext();
        UserRepository userRepository = new UserRepository(dbContext);
        GetUsersUseCase getUsersUseCase = new GetUsersUseCase(userRepository);
        CreateUserUseCase createUserUseCase = new CreateUserUseCase(userRepository);
        UserListViewModel userListViewModel = new UserListViewModel(getUsersUseCase, createUserUseCase);

        // Asignamos el ViewModel a la ventana principal para que el UserListView lo herede
        DataContext = userListViewModel;
    }
}
