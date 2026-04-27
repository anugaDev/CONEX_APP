using System.Collections.ObjectModel;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Views.Users;
using AddActivityWindow = CONEX_APP.Presentation.Views.Activities.AddActivityWindow;

namespace CONEX_APP.Presentation.ViewModels.Users;

public class UserListViewModel : ViewModelBase
{
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly UpdateUserUseCase _updateUserUseCase;
    private readonly DeleteUserUseCase _deleteUserUseCase;
    private readonly GetActivityUseCase _getActivityUseCase;

    public ObservableCollection<UserDto> Users { get; set; }

    private UserDto? _selectedUser;
    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    public ICommand OpenAddUserWindowCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand GoBackCommand { get; }

    public UserListViewModel(GetUsersUseCase getUsersUseCase, CreateUserUseCase createUserUseCase, UpdateUserUseCase updateUserUseCase, DeleteUserUseCase deleteUserUseCase, GetActivityUseCase getActivityUseCase, Action goBack)
    {
        _getUsersUseCase = getUsersUseCase;
        _createUserUseCase = createUserUseCase;
        _updateUserUseCase = updateUserUseCase;
        _deleteUserUseCase = deleteUserUseCase;
        _getActivityUseCase = getActivityUseCase;
        Users = new ObservableCollection<UserDto>();
        
        OpenAddUserWindowCommand = new RelayCommand(_ => OpenAddUserWindow());
        EditUserCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null);
        DeleteUserCommand = new RelayCommand(async _ => await DeleteUserAsync(), _ => SelectedUser != null);
        GoBackCommand = new RelayCommand(_ => goBack());

        _ = LoadUsersAsync();
    }

    private void OpenAddUserWindow()
    {
        var addUserViewModel = new AddUserViewModel(_createUserUseCase, _updateUserUseCase, _getActivityUseCase);
        var addUserWindow = new AddUserWindow(addUserViewModel);
        
        addUserWindow.ShowDialog();

        if (addUserViewModel.WasSaved)
        {
            _ = LoadUsersAsync();
        }
    }

    private void EditUser()
    {
        if (SelectedUser != null)
        {
            var addUserViewModel = new AddUserViewModel(_createUserUseCase, _updateUserUseCase, _getActivityUseCase, SelectedUser);
            var addUserWindow = new AddUserWindow(addUserViewModel);
            
            addUserWindow.ShowDialog();

            if (addUserViewModel.WasSaved)
            {
                _ = LoadUsersAsync();
            }
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

    private async Task DeleteUserAsync()
    {
        if (SelectedUser != null)
        {
            var result = System.Windows.MessageBox.Show(
                $"¿Seguro que quieres eliminar a {SelectedUser.Name} {SelectedUser.Surname}?", 
                "Confirmar Eliminación", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Warning);
            
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                await _deleteUserUseCase.ExecuteAsync(SelectedUser.Id);
                await LoadUsersAsync();
            }
        }
    }
}