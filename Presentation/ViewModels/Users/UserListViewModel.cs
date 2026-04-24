using System.Collections.ObjectModel;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Views.Users;

namespace CONEX_APP.Presentation.ViewModels.Users;

public class UserListViewModel : ViewModelBase
{
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly CreateUserUseCase _createUserUseCase;

    public ObservableCollection<UserDto> Users { get; set; }
    public ICommand OpenAddUserWindowCommand { get; }

    public UserListViewModel(GetUsersUseCase getUsersUseCase, CreateUserUseCase createUserUseCase)
    {
        _getUsersUseCase = getUsersUseCase;
        _createUserUseCase = createUserUseCase;
        Users = new ObservableCollection<UserDto>();
        
        OpenAddUserWindowCommand = new RelayCommand(_ => OpenAddUserWindow());

        _ = LoadUsersAsync();
    }

    private void OpenAddUserWindow()
    {
        var addUserViewModel = new AddUserViewModel(_createUserUseCase);
        var addUserWindow = new AddUserWindow(addUserViewModel);
        
        // ShowDialog bloquea la ventana principal hasta que se cierre esta
        addUserWindow.ShowDialog();

        if (addUserViewModel.WasSaved)
        {
            _ = LoadUsersAsync();
        }
    }

    public async Task LoadUsersAsync()
    {
        try
        {
            var usersFromDb = await _getUsersUseCase.ExecuteAsync();
            Users.Clear();
            foreach (var user in usersFromDb)
            {
                Users.Add(user);
            }
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error cargando usuarios: {ex.Message}");
        }
    }
}