using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers.Reports;
using CONEX_APP.Presentation.Services;
using CONEX_APP.Presentation.Views.Renewals;
using CONEX_APP.Presentation.Views.Settings;
using CONEX_APP.Presentation.Views.Users;

namespace CONEX_APP.Presentation.ViewModels.Users;

public class UserListViewModel : ViewModelBase
{
    private readonly AppServices _services;

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

    public UserListViewModel(AppServices services, Action goBack, Action goToActivities)
    {
        _services = services;

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
        PrintRenewalReceiptCommand = new RelayCommand(async _ => await PrintRenewalReceiptAsync(), _ => HasSelection());
        ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);
        RenewUserCommand = new RelayCommand(async _ => await RenewUserAsync(), _ => HasSelection());
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

        _ = LoadUsersAsync();
    }

    private void OpenAddUserWindow()
    {
        AddUserViewModel addUserViewModel = _services.CreateUserForm();
        AddUserWindow addUserWindow = new AddUserWindow(addUserViewModel) { Owner = System.Windows.Application.Current.MainWindow };

        addUserWindow.ShowDialog();

        if (addUserViewModel.WasSaved)
            _ = LoadUsersAsync();
    }

    private void EditUser()
    {
        if (SelectedUser == null) return;

        AddUserViewModel addUserViewModel = _services.CreateUserForm(SelectedUser);
        AddUserWindow addUserWindow = new AddUserWindow(addUserViewModel) { Owner = System.Windows.Application.Current.MainWindow };

        addUserWindow.ShowDialog();

        if (addUserViewModel.WasSaved)
            _ = LoadUsersAsync();
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
            IEnumerable<UserDto> usersFromDb = await _services.GetUsers.ExecuteAsync();
            Users.Clear();
            foreach (UserDto user in usersFromDb)
                Users.Add(user);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando usuarios: {ex.Message}");
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

        MessageBoxResult result = MessageBox.Show(
            message,
            "Confirmar Eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        List<int> freedActivityIds = new();
        try
        {
            foreach (UserDto user in targets)
            {
                await _services.DeleteUser.ExecuteAsync(user.Id);
                freedActivityIds.AddRange(user.EnrolledActivityIds);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar usuarios: {ex.Message}");
        }

        // Las plazas que dejan libres pueden ser para el primero de cada lista de espera
        await _services.WaitlistPromotion.OfferPromotionsAsync(freedActivityIds);
        await LoadUsersAsync();
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
            List<ActivityScheduleDto> allActivities = (await _services.GetActivities.ExecuteAsync()).ToList();

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

                List<int> selectedClassIds = selectionWindow.ViewModel.SelectedActivities.Select(a => a.Id).ToList();

                // 3. Registrar la renovación (con importes y clases) en la base de datos
                RenewalReceiptDto receipt = await _services.RegisterRenewal.ExecuteAsync(userSnapshot.Id, selectedClassIds);
                anyRenewed = true;

                // 4. Generar e imprimir el recibo con los datos guardados
                UserRenewalReceiptGenerator generator = new UserRenewalReceiptGenerator();
                OpenPdf(generator.GenerateReceipt(userSnapshot, receipt));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al procesar la renovación: {ex.Message}");
        }
        finally
        {
            if (anyRenewed)
                await LoadUsersAsync();
        }
    }

    private async Task PrintRenewalReceiptAsync()
    {
        List<UserDto> targets = GetTargetUsers();
        List<string> withoutRenewals = new();
        List<string> errors = new();
        UserRenewalReceiptGenerator generator = new UserRenewalReceiptGenerator();

        foreach (UserDto user in targets)
        {
            try
            {
                RenewalReceiptDto? receipt = await _services.GetLastRenewalReceipt.ExecuteAsync(user.Id);
                if (receipt == null)
                {
                    withoutRenewals.Add($"• {user.FullName}");
                    continue;
                }

                OpenPdf(generator.GenerateReceipt(user, receipt));
            }
            catch (Exception ex)
            {
                errors.Add($"• {user.FullName}: {ex.Message}");
            }
        }

        if (withoutRenewals.Count > 0)
        {
            string intro = withoutRenewals.Count == 1
                ? "Este usuario no tiene renovaciones registradas:"
                : "Estos usuarios no tienen renovaciones registradas:";
            MessageBox.Show(
                $"{intro}\n\n{string.Join("\n", withoutRenewals)}\n\nUsa «🔄 Renovar» para registrar una renovación y generar su recibo.",
                "Sin renovaciones",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        if (errors.Count > 0)
            MessageBox.Show($"Error al generar el recibo de renovación:\n\n{string.Join("\n", errors)}");
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
        SettingsWindow settingsWindow = SettingsWindow.Create(_services.GetAppSettings, _services.SaveAppSettings);
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
            MessageBox.Show($"Error al generar {documentName}:\n\n{string.Join("\n", errors)}");
    }
}
