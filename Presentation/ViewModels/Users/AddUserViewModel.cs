using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Application.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Registrations;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;

namespace CONEX_APP.Presentation.ViewModels.Users;

public class AddUserViewModel : ViewModelBase
{
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly UpdateUserUseCase _updateUserUseCase;
    private readonly GetActivityUseCase _getActivityUseCase;
    private readonly RemoveUserFromActivityUseCase _removeUserFromActivityUseCase;

    private readonly int? _editingUserId;

    public Action? CloseAction { get; set; }
    public bool WasSaved { get; private set; }

    private string _name = string.Empty;
    
    private string _surname = string.Empty;

    private string _secondSurname = string.Empty;
    
    private string _idCard = string.Empty;

    private string _phone = string.Empty;
    
    private string _address = string.Empty;
    
    private string _location = string.Empty;
    
    private string _email = string.Empty;
    
    private bool _isPartner = false;
    
    private bool _isTutor = false;

    // --- Errores de validación por campo ---
    private string _nameError = string.Empty;
    private string _surnameError = string.Empty;
    private string _idCardError = string.Empty;
    private string _emailError = string.Empty;

    public string NameError
    {
        get => _nameError;
        set => SetProperty(ref _nameError, value);
    }

    public string SurnameError
    {
        get => _surnameError;
        set => SetProperty(ref _surnameError, value);
    }

    public string IdCardError
    {
        get => _idCardError;
        set => SetProperty(ref _idCardError, value);
    }

    public string EmailError
    {
        get => _emailError;
        set => SetProperty(ref _emailError, value);
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
            {
                NameError = string.Empty;
                OnPropertyChanged(nameof(HasNameError));
            }
        }
    }

    public string Surname
    {
        get => _surname;
        set
        {
            SetProperty(ref _surname, value);
            if (!string.IsNullOrWhiteSpace(value))
            {
                SurnameError = string.Empty;
                OnPropertyChanged(nameof(HasSurnameError));
            }
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
            {
                IdCardError = string.Empty;
                OnPropertyChanged(nameof(HasIdCardError));
            }
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
            if (!string.IsNullOrWhiteSpace(value))
            {
                EmailError = string.Empty;
                OnPropertyChanged(nameof(HasEmailError));
            }
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

    private ActivityScheduleDto? _selectedActivityToRemove;
    public ActivityScheduleDto? SelectedActivityToRemove
    {
        get => _selectedActivityToRemove;
        set
        {
            SetProperty(ref _selectedActivityToRemove, value);
            OnPropertyChanged(nameof(CanRemoveActivity));
            ((RelayCommand)RemoveActivityCommand).RaiseCanExecuteChanged();
        }
    }

    public bool CanRemoveActivity => _selectedActivityToRemove != null;

    public ObservableCollection<ActivityScheduleDto> AvailableActivities { get; } = new();
    public ObservableCollection<ActivityScheduleDto> SelectedActivities { get; } = new();

    private ActivityScheduleDto? _selectedActivityToAdd;
    public ActivityScheduleDto? SelectedActivityToAdd
    {
        get => _selectedActivityToAdd;
        set 
        {
            SetProperty(ref _selectedActivityToAdd, value);
            if (value != null)
            {
                SelectedActivities.Add(value);
                AvailableActivities.Remove(value);
                SetProperty(ref _selectedActivityToAdd, null);
            }
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RemoveActivityCommand { get; }

    public AddUserViewModel(CreateUserUseCase createUserUseCase, UpdateUserUseCase updateUserUseCase, GetActivityUseCase getActivityUseCase, RemoveUserFromActivityUseCase removeUserFromActivityUseCase, UserDto? userToEdit = null)
    {
        _createUserUseCase = createUserUseCase;
        _updateUserUseCase = updateUserUseCase;
        _getActivityUseCase = getActivityUseCase;
        _removeUserFromActivityUseCase = removeUserFromActivityUseCase;
        
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        CancelCommand = new RelayCommand(_ => Cancel());
        RemoveActivityCommand = new RelayCommand(async _ => await RemoveActivityAsync(), _ => SelectedActivityToRemove != null);

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
        }

        _ = LoadActivitiesAsync(userToEdit?.EnrolledActivityIds);
    }

    private async Task LoadActivitiesAsync(List<int>? enrolledActivityIds = null)
    {
        try
        {
            IEnumerable<ActivityScheduleDto> activities = await _getActivityUseCase.ExecuteAsync();
            AvailableActivities.Clear();
            SelectedActivities.Clear();

            foreach (ActivityScheduleDto act in activities)
            {
                if (enrolledActivityIds != null && enrolledActivityIds.Contains(act.Id))
                {
                    SelectedActivities.Add(act);
                }
                else if (act.MaxStudents == 0 || act.EnrolledStudentsCount < act.MaxStudents)
                {
                    AvailableActivities.Add(act);
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error cargando clases: {ex.Message}");
        }
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
            OnPropertyChanged(nameof(HasNameError));
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(Surname))
        {
            SurnameError = "El primer apellido es obligatorio.";
            OnPropertyChanged(nameof(HasSurnameError));
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(IdCard))
        {
            IdCardError = "El DNI/NIF es obligatorio.";
            OnPropertyChanged(nameof(HasIdCardError));
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "El correo electrónico es obligatorio.";
            OnPropertyChanged(nameof(HasEmailError));
            valid = false;
        }

        return valid;
    }

    private async Task RemoveActivityAsync()
    {
        if (SelectedActivityToRemove == null) return;

        ActivityScheduleDto actToRemove = SelectedActivityToRemove;

        // New user: just remove from the list in memory, no DB call needed
        if (!_editingUserId.HasValue)
        {
            SelectedActivities.Remove(actToRemove);
            if (actToRemove.MaxStudents == 0 || actToRemove.EnrolledStudentsCount < actToRemove.MaxStudents)
                AvailableActivities.Add(actToRemove);
            SelectedActivityToRemove = null;
            return;
        }

        // Existing user: confirm and call the use case
        MessageBoxResult confirm = MessageBox.Show(
            $"¿Eliminar al usuario de la clase \"{actToRemove.Name}\"?",
            "Confirmar baja de clase",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            await _removeUserFromActivityUseCase.ExecuteAsync(_editingUserId.Value, actToRemove.Id);
            SelectedActivities.Remove(actToRemove);
            if (actToRemove.MaxStudents == 0 || actToRemove.EnrolledStudentsCount - 1 < actToRemove.MaxStudents)
                AvailableActivities.Add(actToRemove);
            SelectedActivityToRemove = null;
            WasSaved = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar de la clase: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SaveAsync()
    {
        if (!Validate()) return;

        try
        {
            if (_editingUserId.HasValue)
            {
                UpdateUserDto dto = new UpdateUserDto
                {
                    Id = _editingUserId.Value,
                    Name = Name, 
                    Surname = Surname,
                    SecondSurname = SecondSurname,
                    Email = Email,
                    Phone = Phone,
                    Address = Address,
                    Location = Location,
                    IdCard = IdCard,
                    IsPartner = IsPartner,
                    IsTutor = IsTutor,
                    SelectedActivityIds = SelectedActivities.Select(a => a.Id).ToList()
                };
                await _updateUserUseCase.ExecuteAsync(dto);
            }
            else
            {
                CreateUserDto dto = GetNewUSerDto();
                await _createUserUseCase.ExecuteAsync(dto);
            }
            WasSaved = true;
            CloseAction?.Invoke();
        }
        catch (DuplicateEntityException ex)
        {
            // Mostrar el error en el campo DNI/NIF ya que suele ser el duplicado
            IdCardError = ex.Message;
            OnPropertyChanged(nameof(HasIdCardError));
        }
    }

    private CreateUserDto GetNewUSerDto()
    {
        return new CreateUserDto {
            Name = Name, 
            Surname = Surname,
            SecondSurname = SecondSurname,
            Email = Email ,
            Phone = Phone,
            Address = Address,
            Location = Location,
            IdCard = IdCard,
            IsPartner = IsPartner,
            IsTutor = IsTutor,
            SelectedActivityIds = SelectedActivities.Select(a => a.Id).ToList()
        };
    }

    private void Cancel()
    {
        WasSaved = false;
        CloseAction?.Invoke();
    }
}
