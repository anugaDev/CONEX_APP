using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using CONEX_APP.Application.DTOs;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Registrations;
using CONEX_APP.MainApplication.UseCases.Renewals;
using CONEX_APP.MainApplication.UseCases.Settings;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers.Reports;
using CONEX_APP.Presentation.ViewModels.Settings;
using CONEX_APP.Presentation.Views.Renewals;
using CONEX_APP.Presentation.Views.Settings;
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
    private readonly RegisterRenewalUseCase _registerRenewalUseCase;
    private readonly GetAppSettingsUseCase _getAppSettingsUseCase;
    private readonly SaveAppSettingsUseCase _saveAppSettingsUseCase;

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

    private IReadOnlyList<UserDto> _selectedUsers = [];
    /// <summary>Usuarios seleccionados en la tabla (admite Shift/Ctrl + clic).</summary>
    public IReadOnlyList<UserDto> SelectedUsers
    {
        get => _selectedUsers;
        set => SetProperty(ref _selectedUsers, value ?? []);
    }

    /// <summary>Devuelve los usuarios sobre los que actuar: la selección múltiple o, en su defecto, el seleccionado.</summary>
    private List<UserDto> GetTargetUsers()
    {
        if (_selectedUsers.Count > 0)
            return _selectedUsers.ToList();

        return SelectedUser != null ? [SelectedUser] : [];
    }

    private bool HasSelection() => _selectedUsers.Count > 0 || SelectedUser != null;

    public ICommand OpenAddUserWindowCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand GoBackCommand { get; }
    public ICommand GoToActivitiesCommand { get; }
    public ICommand PrintBadgeCommand { get; }
    public ICommand PrintRegistrationFormCommand { get; }
    public ICommand PrintRenewalReceiptCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand RenewUserCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    public UserListViewModel(
        GetUsersUseCase getUsersUseCase,
        CreateUserUseCase createUserUseCase,
        UpdateUserUseCase updateUserUseCase,
        DeleteUserUseCase deleteUserUseCase,
        GetActivityUseCase getActivityUseCase,
        RemoveUserFromActivityUseCase removeUserFromActivityUseCase,
        RegisterRenewalUseCase registerRenewalUseCase,
        GetAppSettingsUseCase getAppSettingsUseCase,
        SaveAppSettingsUseCase saveAppSettingsUseCase,
        Action goBack,
        Action goToActivities)
    {
        _getUsersUseCase = getUsersUseCase;
        _createUserUseCase = createUserUseCase;
        _updateUserUseCase = updateUserUseCase;
        _deleteUserUseCase = deleteUserUseCase;
        _getActivityUseCase = getActivityUseCase;
        _removeUserFromActivityUseCase = removeUserFromActivityUseCase;
        _registerRenewalUseCase = registerRenewalUseCase;
        _getAppSettingsUseCase = getAppSettingsUseCase;
        _saveAppSettingsUseCase = saveAppSettingsUseCase;

        Users = new ObservableCollection<UserDto>();
        UsersView = CollectionViewSource.GetDefaultView(Users);
        UsersView.Filter = FilterUser;
        
        OpenAddUserWindowCommand = new RelayCommand(_ => OpenAddUserWindow());
        EditUserCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null);
        DeleteUserCommand = new RelayCommand(async _ => await DeleteUserAsync(), _ => HasSelection());
        GoBackCommand = new RelayCommand(_ => goBack());
        GoToActivitiesCommand = new RelayCommand(_ => goToActivities());
        PrintBadgeCommand = new RelayCommand(_ => PrintBadge(), _ => HasSelection());
        PrintRegistrationFormCommand = new RelayCommand(_ => PrintRegistrationForm(), _ => HasSelection());
        PrintRenewalReceiptCommand = new RelayCommand(_ => PrintRenewalReceipt(), _ => HasSelection());
        ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);
        RenewUserCommand = new RelayCommand(async _ => await RenewUserAsync(), _ => HasSelection());
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

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
        List<UserDto> targets = GetTargetUsers();
        if (targets.Count == 0) return;

        string message = targets.Count == 1
            ? $"¿Seguro que quieres eliminar a {targets[0].Name} {targets[0].Surname}?"
            : $"¿Seguro que quieres eliminar a los {targets.Count} usuarios seleccionados?\n\n" +
              string.Join("\n", targets.Select(u => $"• {u.Name} {u.Surname}"));

        System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
            message,
            "Confirmar Eliminación",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            foreach (UserDto user in targets)
                await _deleteUserUseCase.ExecuteAsync(user.Id);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al eliminar usuarios: {ex.Message}");
        }
        finally
        {
            await LoadUsersAsync();
        }
    }

    private async Task RenewUserAsync()
    {
        // Capturamos la selección antes de cualquier recarga que pueda limpiarla
        List<UserDto> targets = GetTargetUsers();
        if (targets.Count == 0) return;

        bool anyRenewed = false;

        try
        {
            // 1. Obtener todas las clases una sola vez
            List<ActivityScheduleDto> allActivities = (await _getActivityUseCase.ExecuteAsync()).ToList();

            for (int i = 0; i < targets.Count; i++)
            {
                UserDto userSnapshot = targets[i];

                List<ActivityScheduleDto> enrolledActivities = allActivities
                    .Where(a => userSnapshot.EnrolledActivityIds.Contains(a.Id))
                    .ToList();

                // 2. Abrir ventana de selección de clases para este usuario
                RenewalClassSelectionWindow selectionWindow = new RenewalClassSelectionWindow(enrolledActivities);
                string counter = targets.Count > 1 ? $" ({i + 1}/{targets.Count})" : string.Empty;
                selectionWindow.Title = $"🔄 Renovación: {userSnapshot.Name} {userSnapshot.Surname}{counter}";
                selectionWindow.ShowDialog();

                // Si se cancela, se omite este usuario y se pasa al siguiente
                if (!selectionWindow.ViewModel.Confirmed) continue;

                IReadOnlyList<ActivityScheduleDto> selectedClasses = selectionWindow.ViewModel.SelectedActivities;

                // 3. Registrar la renovación en la base de datos
                await _registerRenewalUseCase.ExecuteAsync(userSnapshot.Id);
                anyRenewed = true;

                // 4. Generar e imprimir el recibo con las clases seleccionadas
                UserRenewalReceiptGenerator generator = new UserRenewalReceiptGenerator();
                string pdfPath = generator.GenerateReceipt(userSnapshot, selectedClasses);
                OpenPdf(pdfPath);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al procesar la renovación: {ex.Message}");
        }
        finally
        {
            if (anyRenewed)
                await LoadUsersAsync();
        }
    }

    private static void OpenPdf(string pdfPath)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = pdfPath,
            UseShellExecute = true
        });
    }

    private void OpenSettings()
    {
        SettingsWindow settingsWindow = SettingsWindow.Create(_getAppSettingsUseCase, _saveAppSettingsUseCase);
        settingsWindow.ShowDialog();

        // Recargamos los usuarios para que el estado de renovación refleje el nuevo período
        _ = LoadUsersAsync();
    }

    private void PrintBadge()
    {
        UserBadgeGenerator generator = new UserBadgeGenerator();
        PrintForSelectedUsers(user => generator.GenerateBadge(user), "el carnet");
    }

    private void PrintRegistrationForm()
    {
        UserRegistrationFormGenerator generator = new UserRegistrationFormGenerator();
        PrintForSelectedUsers(user => generator.GenerateRegistrationForm(user), "la hoja de inscripción");
    }

    private void PrintRenewalReceipt()
    {
        UserRenewalReceiptGenerator generator = new UserRenewalReceiptGenerator();
        PrintForSelectedUsers(user => generator.GenerateReceipt(user), "el recibo de renovación");
    }

    /// <summary>Genera y abre un PDF por cada usuario seleccionado.</summary>
    private void PrintForSelectedUsers(Func<UserDto, string> generatePdf, string documentName)
    {
        List<UserDto> targets = GetTargetUsers();
        List<string> errors = new List<string>();

        foreach (UserDto user in targets)
        {
            try
            {
                OpenPdf(generatePdf(user));
            }
            catch (Exception ex)
            {
                errors.Add($"• {user.Name} {user.Surname}: {ex.Message}");
            }
        }

        if (errors.Count > 0)
            System.Windows.MessageBox.Show($"Error al generar {documentName}:\n\n{string.Join("\n", errors)}");
    }
}