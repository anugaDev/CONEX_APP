using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Registrations;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers.Reports;
using CONEX_APP.Presentation.Views.Users;

namespace CONEX_APP.Presentation.ViewModels.Users;

public class UserListViewModel : ViewModelBase
{
    private readonly GetUsersUseCase _getUsersUseCase;

    private readonly CreateUserUseCase _createUserUseCase;

    private readonly UpdateUserUseCase _updateUserUseCase;

    private readonly DeleteUserUseCase _deleteUserUseCase;

    private readonly GetActivityUseCase _getActivityUseCase;

    private readonly RemoveUserFromActivityUseCase _removeUserFromActivityUseCase;

    public ObservableCollection<UserDto> Users { get; set; }

    public ICollectionView UsersView { get; }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                UsersView.Refresh();
        }
    }

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
    public ICommand GoToActivitiesCommand { get; }
    public ICommand PrintBadgeCommand { get; }
    public ICommand PrintRegistrationFormCommand { get; }
    public ICommand PrintRenewalReceiptCommand { get; }
    public ICommand ClearSearchCommand { get; }

    public UserListViewModel(GetUsersUseCase getUsersUseCase, CreateUserUseCase createUserUseCase, UpdateUserUseCase updateUserUseCase, DeleteUserUseCase deleteUserUseCase, GetActivityUseCase getActivityUseCase, RemoveUserFromActivityUseCase removeUserFromActivityUseCase, Action goBack, Action goToActivities)
    {
        _getUsersUseCase = getUsersUseCase;
        _createUserUseCase = createUserUseCase;
        _updateUserUseCase = updateUserUseCase;
        _deleteUserUseCase = deleteUserUseCase;
        _getActivityUseCase = getActivityUseCase;
        _removeUserFromActivityUseCase = removeUserFromActivityUseCase;
        Users = new ObservableCollection<UserDto>();
        UsersView = CollectionViewSource.GetDefaultView(Users);
        UsersView.Filter = FilterUser;
        
        OpenAddUserWindowCommand = new RelayCommand(_ => OpenAddUserWindow());
        EditUserCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null);
        DeleteUserCommand = new RelayCommand(async _ => await DeleteUserAsync(), _ => SelectedUser != null);
        GoBackCommand = new RelayCommand(_ => goBack());
        GoToActivitiesCommand = new RelayCommand(_ => goToActivities());
        PrintBadgeCommand = new RelayCommand(_ => PrintBadge(), _ => SelectedUser != null);
        PrintRegistrationFormCommand = new RelayCommand(_ => PrintRegistrationForm(), _ => SelectedUser != null);
        PrintRenewalReceiptCommand = new RelayCommand(_ => PrintRenewalReceipt(), _ => SelectedUser != null);
        ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);

        _ = LoadUsersAsync();
    }

    private void OpenAddUserWindow()
    {
        AddUserViewModel addUserViewModel = new AddUserViewModel(_createUserUseCase, _updateUserUseCase, _getActivityUseCase, _removeUserFromActivityUseCase);
        AddUserWindow addUserWindow = new AddUserWindow(addUserViewModel);
        
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
            AddUserViewModel addUserViewModel = new AddUserViewModel(_createUserUseCase, _updateUserUseCase, _getActivityUseCase, _removeUserFromActivityUseCase, SelectedUser);
            AddUserWindow addUserWindow = new AddUserWindow(addUserViewModel);
            
            addUserWindow.ShowDialog();

            if (addUserViewModel.WasSaved)
            {
                _ = LoadUsersAsync();
            }
        }
    }

    private bool FilterUser(object obj)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
            return true;

        if (obj is not UserDto user)
            return false;

        string search = _searchText.Trim().ToLowerInvariant();
        return user.Name.ToLowerInvariant().Contains(search)
            || user.Surname.ToLowerInvariant().Contains(search)
            || user.SecondSurname.ToLowerInvariant().Contains(search)
            || user.IdCard.ToLowerInvariant().Contains(search)
            || user.Phone.ToLowerInvariant().Contains(search)
            || user.Email.ToLowerInvariant().Contains(search)
            || user.Address.ToLowerInvariant().Contains(search)
            || user.Location.ToLowerInvariant().Contains(search);
    }

    public async Task LoadUsersAsync()
    {
        try
        {
            IEnumerable<UserDto> usersFromDb = await _getUsersUseCase.ExecuteAsync();
            Users.Clear();
            foreach (UserDto user in usersFromDb)
            {
                Users.Add(user);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error cargando usuarios: {ex.Message}");
        }
    }

    private async Task DeleteUserAsync()
    {
        if (SelectedUser != null)
        {
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
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

    private void PrintBadge()
    {
        if (SelectedUser == null) return;

        try
        {
            UserBadgeGenerator generator = new UserBadgeGenerator();
            string pdfPath = generator.GenerateBadge(SelectedUser);

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar el carnet: {ex.Message}");
        }
    }

    private void PrintRegistrationForm()
    {
        if (SelectedUser == null) return;

        try
        {
            UserRegistrationFormGenerator generator = new UserRegistrationFormGenerator();
            string pdfPath = generator.GenerateRegistrationForm(SelectedUser);

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar la hoja de inscripción: {ex.Message}");
        }
    }

    private void PrintRenewalReceipt()
    {
        if (SelectedUser == null) return;

        try
        {
            UserRenewalReceiptGenerator generator = new UserRenewalReceiptGenerator();
            string pdfPath = generator.GenerateReceipt(SelectedUser);

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar el recibo de renovación: {ex.Message}");
        }
    }
}