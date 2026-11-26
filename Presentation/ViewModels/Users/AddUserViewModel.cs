using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Windows;
using System.Windows.Input;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.MainApplication.UseCases.Waitlists;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Services;

namespace CONEX_APP.Presentation.ViewModels.Users;

public class UserWaitlistItem
{
    public ActivityScheduleDto Activity { get; init; } = new();

    public int? Position { get; init; }

    public string ActivityName => Activity.Name;

    public string StatusLabel => Position.HasValue
        ? $"#{Position} en la lista"
        : "nueva: se apuntará al guardar";
}

public class AddUserViewModel : ViewModelBase
{
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly UpdateUserUseCase _updateUserUseCase;
    private readonly GetActivityUseCase _getActivityUseCase;
    private readonly GetWaitlistUseCase _getWaitlistUseCase;
    private readonly AddToWaitlistUseCase _addToWaitlistUseCase;
    private readonly RemoveFromWaitlistUseCase _removeFromWaitlistUseCase;
    private readonly WaitlistPromotionService _waitlistPromotionService;

    private readonly int? _editingUserId;

    private readonly HashSet<int> _originalEnrolledIds = new();

    private readonly HashSet<int> _originalWaitlistIds = new();

    public Action? CloseAction { get; set; }
    public bool WasSaved { get; private set; }

    public string WindowTitle => _editingUserId.HasValue ? "Editar Usuario" : "Añadir Nuevo Usuario";

    private string _name = string.Empty;
    private string _surname = string.Empty;
    private string _secondSurname = string.Empty;
    private string _idCard = string.Empty;
    private string _phone = string.Empty;
    private string _address = string.Empty;
    private string _location = string.Empty;
    private string _email = string.Empty;
    private bool _isPartner;
    private bool _isTutor;

    // --- Errores de validación por campo ---
    private string _nameError = string.Empty;
    private string _surnameError = string.Empty;
    private string _idCardError = string.Empty;
    private string _emailError = string.Empty;

    public string NameError
    {
        get => _nameError;
        set { SetProperty(ref _nameError, value); OnPropertyChanged(nameof(HasNameError)); }
    }

    public string SurnameError
    {
        get => _surnameError;
        set { SetProperty(ref _surnameError, value); OnPropertyChanged(nameof(HasSurnameError)); }
    }

    public string IdCardError
    {
        get => _idCardError;
        set { SetProperty(ref _idCardError, value); OnPropertyChanged(nameof(HasIdCardError)); }
    }

    public string EmailError
    {
        get => _emailError;
        set { SetProperty(ref _emailError, value); OnPropertyChanged(nameof(HasEmailError)); }
    }

    // Verdadero cuando el campo tiene error (para trigger en XAML)
    public bool HasNameError    => !string.IsNullOrEmpty(NameError);
    public bool HasSurnameError => !string.IsNullOrEmpty(SurnameError);
    public bool HasIdCardError  => !string.IsNullOrEmpty(IdCardError);
    public bool HasEmailError   => !string.IsNullOrEmpty(EmailError);

    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            if (!string.IsNullOrWhiteSpace(value))
                NameError = string.Empty;
        }
    }

    public string Surname
    {
        get => _surname;
        set
        {
            SetProperty(ref _surname, value);
            if (!string.IsNullOrWhiteSpace(value))
                SurnameError = string.Empty;
        }
    }

    public string SecondSurname
    {
        get => _secondSurname;
        set => SetProperty(ref _secondSurname, value);
    }

    public string IdCard
    {
        get => _idCard;
        set
        {
            SetProperty(ref _idCard, value);
            if (!string.IsNullOrWhiteSpace(value))
                IdCardError = string.Empty;
        }
    }

    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            EmailError = string.Empty;
        }
    }

    public bool IsPartner
    {
        get => _isPartner;
        set => SetProperty(ref _isPartner, value);
    }

    public bool IsTutor
    {
        get => _isTutor;
        set => SetProperty(ref _isTutor, value);
    }

    // --- Clases ---

    public ObservableCollection<ActivityScheduleDto> AvailableActivities { get; } = new();

    public ObservableCollection<ActivityScheduleDto> SelectedActivities { get; } = new();

    public ObservableCollection<UserWaitlistItem> WaitlistItems { get; } = new();

    private ActivityScheduleDto? _selectedActivityToAdd;
    public ActivityScheduleDto? SelectedActivityToAdd
    {
        get => _selectedActivityToAdd;
        set
        {
            SetProperty(ref _selectedActivityToAdd, value);
            if (value == null) return;

            // Se procesa después de que el desplegable termine de actualizarse: puede abrir un diálogo
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _selectedActivityToAdd = null;
                OnPropertyChanged(nameof(SelectedActivityToAdd));
                AddActivity(value);
            });
        }
    }

    private ActivityScheduleDto? _selectedActivityToRemove;
    public ActivityScheduleDto? SelectedActivityToRemove
    {
        get => _selectedActivityToRemove;
        set
        {
            SetProperty(ref _selectedActivityToRemove, value);
            OnPropertyChanged(nameof(CanRemoveActivity));
        }
    }

    public bool CanRemoveActivity => _selectedActivityToRemove != null;

    private UserWaitlistItem? _selectedWaitlistItem;
    public UserWaitlistItem? SelectedWaitlistItem
    {
        get => _selectedWaitlistItem;
        set => SetProperty(ref _selectedWaitlistItem, value);
    }

    private bool _isBusy;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RemoveActivityCommand { get; }
    public ICommand RemoveFromWaitlistCommand { get; }

    public AddUserViewModel(
        CreateUserUseCase createUserUseCase,
        UpdateUserUseCase updateUserUseCase,
        GetActivityUseCase getActivityUseCase,
        GetWaitlistUseCase getWaitlistUseCase,
        AddToWaitlistUseCase addToWaitlistUseCase,
        RemoveFromWaitlistUseCase removeFromWaitlistUseCase,
        WaitlistPromotionService waitlistPromotionService,
        UserDto? userToEdit = null)
    {
        _createUserUseCase = createUserUseCase;
        _updateUserUseCase = updateUserUseCase;
        _getActivityUseCase = getActivityUseCase;
        _getWaitlistUseCase = getWaitlistUseCase;
        _addToWaitlistUseCase = addToWaitlistUseCase;
        _removeFromWaitlistUseCase = removeFromWaitlistUseCase;
        _waitlistPromotionService = waitlistPromotionService;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => !_isBusy);
        CancelCommand = new RelayCommand(_ => Cancel());
        RemoveActivityCommand = new RelayCommand(_ => RemoveActivity(), _ => SelectedActivityToRemove != null);
        RemoveFromWaitlistCommand = new RelayCommand(_ => RemoveFromWaitlist(), _ => SelectedWaitlistItem != null);

        if (userToEdit != null)
        {
            _editingUserId = userToEdit.Id;
            _name = userToEdit.Name;
            _surname = userToEdit.Surname;
            _secondSurname = userToEdit.SecondSurname;
            _idCard = userToEdit.IdCard;
            _phone = userToEdit.Phone;
            _email = userToEdit.Email;
            _address = userToEdit.Address;
            _location = userToEdit.Location;
            _isPartner = userToEdit.IsPartner;
            _isTutor = userToEdit.IsTutor;
            _originalEnrolledIds.UnionWith(userToEdit.EnrolledActivityIds);
        }

        _ = LoadActivitiesAsync();
    }

    private async Task LoadActivitiesAsync()
    {
        try
        {
            List<ActivityScheduleDto> activities = (await _getActivityUseCase.ExecuteAsync())
                .OrderBy(a => a.Name)
                .ToList();

            List<UserWaitlistEntryDto> waitlists = _editingUserId.HasValue
                ? await _getWaitlistUseCase.ExecuteForUserAsync(_editingUserId.Value)
                : new List<UserWaitlistEntryDto>();
            Dictionary<int, int> waitlistPositions = waitlists.ToDictionary(w => w.ActivityId, w => w.Position);
            _originalWaitlistIds.UnionWith(waitlistPositions.Keys);

            AvailableActivities.Clear();
            SelectedActivities.Clear();
            WaitlistItems.Clear();

            foreach (ActivityScheduleDto activity in activities)
            {
                if (_originalEnrolledIds.Contains(activity.Id))
                    SelectedActivities.Add(activity);
                else if (waitlistPositions.TryGetValue(activity.Id, out int position))
                    WaitlistItems.Add(new UserWaitlistItem { Activity = activity, Position = position });
                else
                    AvailableActivities.Add(activity);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando clases: {ex.Message}");
        }
    }

    private void AddActivity(ActivityScheduleDto activity)
    {
        if (!AvailableActivities.Contains(activity))
            return;

        // Si ya estaba inscrito al abrir la ficha, su plaza sigue siendo suya aunque la clase figure completa
        bool keepsOwnSeat = _originalEnrolledIds.Contains(activity.Id);

        if (activity.IsFull && !keepsOwnSeat)
        {
            string waiting = activity.WaitlistCount == 1 ? "1 persona" : $"{activity.WaitlistCount} personas";
            MessageBoxResult answer = MessageBox.Show(
                $"La clase \"{activity.Name}\" está completa ({activity.Occupancy}).\n" +
                $"Ahora mismo hay {waiting} en la lista de espera.\n\n" +
                "¿Añadir al usuario a la lista de espera?",
                "Clase completa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer != MessageBoxResult.Yes)
                return;

            AvailableActivities.Remove(activity);
            WaitlistItems.Add(new UserWaitlistItem { Activity = activity });
            return;
        }

        AvailableActivities.Remove(activity);
        SelectedActivities.Add(activity);
    }

    private void RemoveActivity()
    {
        if (SelectedActivityToRemove is not { } activity) return;

        SelectedActivities.Remove(activity);
        InsertSorted(AvailableActivities, activity);
        SelectedActivityToRemove = null;
    }

    private void RemoveFromWaitlist()
    {
        if (SelectedWaitlistItem is not { } item) return;

        WaitlistItems.Remove(item);
        InsertSorted(AvailableActivities, item.Activity);
        SelectedWaitlistItem = null;
    }

    private static void InsertSorted(ObservableCollection<ActivityScheduleDto> list, ActivityScheduleDto activity)
    {
        int index = 0;
        while (index < list.Count && string.Compare(list[index].Name, activity.Name, StringComparison.CurrentCultureIgnoreCase) < 0)
            index++;
        list.Insert(index, activity);
    }

    /// <summary>
    /// Valida los campos obligatorios. Devuelve true si todo es correcto.
    /// </summary>
    private bool Validate()
    {
        bool valid = true;

        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "El nombre es obligatorio.";
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(Surname))
        {
            SurnameError = "El primer apellido es obligatorio.";
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(IdCard))
        {
            IdCardError = "El DNI/NIF es obligatorio.";
            valid = false;
        }

        // El correo es opcional (muchos socios importados no lo tienen), pero si se pone tiene que ser válido
        if (!string.IsNullOrWhiteSpace(Email) && !IsValidEmail(Email.Trim()))
        {
            EmailError = "El correo electrónico no tiene un formato válido.";
            valid = false;
        }

        return valid;
    }

    private static bool IsValidEmail(string email) =>
        MailAddress.TryCreate(email, out MailAddress? address)
        && address.Address == email
        && address.Host.Contains('.');

    private async Task SaveAsync()
    {
        if (!Validate()) return;

        _isBusy = true;
        try
        {
            List<int> selectedIds = SelectedActivities.Select(a => a.Id).ToList();
            int userId;

            if (_editingUserId.HasValue)
            {
                userId = _editingUserId.Value;
                await _updateUserUseCase.ExecuteAsync(new UpdateUserDto
                {
                    Id = userId,
                    Name = Name.Trim(),
                    Surname = Surname.Trim(),
                    SecondSurname = SecondSurname.Trim(),
                    Email = Email.Trim(),
                    Phone = Phone.Trim(),
                    Address = Address.Trim(),
                    Location = Location.Trim(),
                    IdCard = IdCard.Trim(),
                    IsPartner = IsPartner,
                    IsTutor = IsTutor,
                    SelectedActivityIds = selectedIds
                });
            }
            else
            {
                userId = await _createUserUseCase.ExecuteAsync(new CreateUserDto
                {
                    Name = Name.Trim(),
                    Surname = Surname.Trim(),
                    SecondSurname = SecondSurname.Trim(),
                    Email = Email.Trim(),
                    Phone = Phone.Trim(),
                    Address = Address.Trim(),
                    Location = Location.Trim(),
                    IdCard = IdCard.Trim(),
                    IsPartner = IsPartner,
                    IsTutor = IsTutor,
                    SelectedActivityIds = selectedIds
                });
            }

            // A partir de aquí el usuario ya está guardado: la ventana se cerrará pase lo que pase
            WasSaved = true;

            List<string> warnings = await SaveWaitlistChangesAsync(userId);

            // Las plazas que deja libres pueden ser para el primero de la lista de espera
            IEnumerable<int> freedActivityIds = _originalEnrolledIds.Where(id => !selectedIds.Contains(id));
            await _waitlistPromotionService.OfferPromotionsAsync(freedActivityIds);

            if (warnings.Count > 0)
            {
                MessageBox.Show(
                    "El usuario se ha guardado, pero algunas listas de espera no se han podido actualizar:\n\n" +
                    string.Join("\n", warnings.Select(w => $"• {w}")),
                    "Listas de espera",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            CloseAction?.Invoke();
        }
        catch (DuplicateEntityException ex)
        {
            // Mostrar el error en el campo DNI/NIF ya que suele ser el duplicado
            IdCardError = ex.Message;
        }
        catch (ActivityFullException ex)
        {
            // Otra persona ha ocupado la última plaza desde que se abrió la ficha: se devuelve la clase
            // a la lista con los datos actualizados y se ofrece la lista de espera
            MessageBox.Show(ex.Message, "Clase completa", MessageBoxButton.OK, MessageBoxImage.Warning);
            await ReofferFullActivityAsync(ex.ActivityId);
        }
        catch (DomainException ex)
        {
            MessageBox.Show(ex.Message, "No se puede guardar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task ReofferFullActivityAsync(int activityId)
    {
        ActivityScheduleDto? stale = SelectedActivities.FirstOrDefault(a => a.Id == activityId);
        if (stale == null) return;

        SelectedActivities.Remove(stale);

        ActivityScheduleDto? refreshed = await _getActivityUseCase.ExecuteByIdAsync(activityId);
        if (refreshed == null) return;

        InsertSorted(AvailableActivities, refreshed);
        AddActivity(refreshed);
    }

    private async Task<List<string>> SaveWaitlistChangesAsync(int userId)
    {
        List<string> warnings = new();
        HashSet<int> currentIds = WaitlistItems.Select(w => w.Activity.Id).ToHashSet();

        foreach (int activityId in _originalWaitlistIds.Where(id => !currentIds.Contains(id)))
            await _removeFromWaitlistUseCase.ExecuteAsync(userId, activityId);

        foreach (UserWaitlistItem item in WaitlistItems.Where(w => !_originalWaitlistIds.Contains(w.Activity.Id)))
        {
            try
            {
                await _addToWaitlistUseCase.ExecuteAsync(userId, item.Activity.Id);
            }
            catch (DomainException ex)
            {
                warnings.Add(ex.Message);
            }
        }

        return warnings;
    }

    private void Cancel()
    {
        WasSaved = false;
        CloseAction?.Invoke();
    }
}
