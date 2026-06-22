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

    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Surname
    {
        get => _surname;
        set => SetProperty(ref _surname, value);
    }

    public string SecondSurname
    {
        get => _secondSurname;
        set => SetProperty(ref _secondSurname, value);
    }

    public string IdCard
    {
        get => _idCard;
        set => SetProperty(ref _idCard, value);
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
        set => SetProperty(ref _email, value);
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
            ((RelayCommand)RemoveActivityCommand).RaiseCanExecuteChanged();
        }
    }

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
        
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());
        RemoveActivityCommand = new RelayCommand(async _ => await RemoveActivityAsync(), _ => SelectedActivityToRemove != null && _editingUserId.HasValue);

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

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email);
    }

    private async Task RemoveActivityAsync()
    {
        if (SelectedActivityToRemove == null || !_editingUserId.HasValue) return;

        ActivityScheduleDto actToRemove = SelectedActivityToRemove;

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
            MessageBox.Show(ex.Message, "Usuario duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
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
