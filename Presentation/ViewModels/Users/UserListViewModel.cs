using System.Collections.ObjectModel;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
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
    public ICommand GoToActivitiesCommand { get; }
    public ICommand PrintBadgeCommand { get; }
    public ICommand PrintRegistrationFormCommand { get; }
    public ICommand PrintRenewalReceiptCommand { get; }

    public UserListViewModel(GetUsersUseCase getUsersUseCase, CreateUserUseCase createUserUseCase, UpdateUserUseCase updateUserUseCase, DeleteUserUseCase deleteUserUseCase, GetActivityUseCase getActivityUseCase, Action goBack, Action goToActivities)
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
        GoToActivitiesCommand = new RelayCommand(_ => goToActivities());
        PrintBadgeCommand = new RelayCommand(_ => PrintBadge(), _ => SelectedUser != null);
        PrintRegistrationFormCommand = new RelayCommand(_ => PrintRegistrationForm(), _ => SelectedUser != null);
        PrintRenewalReceiptCommand = new RelayCommand(_ => PrintRenewalReceipt(), _ => SelectedUser != null);

        _ = LoadUsersAsync();
    }

    private void OpenAddUserWindow()
    {
        AddUserViewModel addUserViewModel = new AddUserViewModel(_createUserUseCase, _updateUserUseCase, _getActivityUseCase);
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
            AddUserViewModel addUserViewModel = new AddUserViewModel(_createUserUseCase, _updateUserUseCase, _getActivityUseCase, SelectedUser);
            AddUserWindow addUserWindow = new AddUserWindow(addUserViewModel);
            
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