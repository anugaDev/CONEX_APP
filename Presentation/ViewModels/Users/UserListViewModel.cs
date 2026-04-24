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
        var usersFromDb = await _getUsersUseCase.ExecuteAsync();
        
        Users.Clear();
        foreach (var user in usersFromDb)
        {
            Users.Add(user);
        }
    }
}