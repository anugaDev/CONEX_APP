using System.Collections.ObjectModel;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Users;

namespace CONEX_APP.Presentation.ViewModels.Users;

public class UserListViewModel
{
    private readonly GetUsersUseCase _getUsersUseCase;

    public ObservableCollection<UserDto> Users { get; set; }

    public UserListViewModel(GetUsersUseCase getUsersUseCase)
    {
        _getUsersUseCase = getUsersUseCase;
        Users = new ObservableCollection<UserDto>();
        _ = LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var usersFromDb = await _getUsersUseCase.ExecuteAsync();
            
            // Debugging line
            System.Windows.MessageBox.Show($"Usuarios cargados desde la base de datos: {usersFromDb.Count()}");

            Users.Clear();
            foreach (var user in usersFromDb)
            {
                Users.Add(user);
            }
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error cargando usuarios: {ex.Message}\n{ex.InnerException?.Message}");
        }
    }
}