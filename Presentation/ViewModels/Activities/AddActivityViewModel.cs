using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using CONEX_APP.Domain.Exceptions;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Registrations;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.MainApplication.UseCases.Waitlists;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers;
using CONEX_APP.Presentation.Services;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class AddActivityViewModel : ViewModelBase
{
    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");

    private readonly CreateActivityUseCase _createActivityUseCase;
    private readonly UpdateActivityUseCase _updateActivityUseCase;
    private readonly GetActivityUseCase _getActivityUseCase;
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly RemoveUserFromActivityUseCase _removeUserFromActivityUseCase;
    private readonly EnrollUserUseCase _enrollUserUseCase;
    private readonly AddToWaitlistUseCase _addToWaitlistUseCase;
    private readonly GetWaitlistUseCase _getWaitlistUseCase;
    private readonly RemoveFromWaitlistUseCase _removeFromWaitlistUseCase;
    private readonly WaitlistPromotionService _waitlistPromotionService;

    private readonly int? _editingActivityId;

    private ActivityScheduleDto? _savedActivity;

    private List<UserDto> _allUsers = new();

    public Action? CloseAction { get; set; }

    public bool WasSaved { get; private set; }

    public bool IsEditing => _editingActivityId.HasValue;

    public string WindowTitle => IsEditing ? "Editar Actividad" : "Añadir Nueva Actividad";

    private string _name = string.Empty;
    private string _classRoom = string.Empty;
    private int _maxStudents;

    public int MaxStudents
    {
        get => _maxStudents;
        set => SetProperty(ref _maxStudents, value);
    }

    public ObservableCollection<string> DaysOfWeek { get; } = new()
    {
        "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"
    };

    private string _selectedDay = "Lunes";
    public string SelectedDay
    {
        get => _selectedDay;
        set => SetProperty(ref _selectedDay, value);
    }

    public ObservableCollection<string> Times { get; } = new();

    private string _selectedTime = "12:00";
    public string SelectedTime
    {
        get => _selectedTime;
        set => SetProperty(ref _selectedTime, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private UserDto? _selectedTutor;
    public UserDto? SelectedTutor
    {
        get => _selectedTutor;
        set => SetProperty(ref _selectedTutor, value);
    }

    public ObservableCollection<UserDto> Tutors { get; } = new();

    public string Classroom
    {
        get => _classRoom;
        set => SetProperty(ref _classRoom, value);
    }

    // --- Alumnos ---

    public ObservableCollection<EnrolledStudentDto> EnrolledStudents { get; } = new();

    private EnrolledStudentDto? _selectedEnrolledStudent;
    public EnrolledStudentDto? SelectedEnrolledStudent
    {
        get => _selectedEnrolledStudent;
        set => SetProperty(ref _selectedEnrolledStudent, value);
    }

    private string _occupancyText = string.Empty;
    public string OccupancyText
    {
        get => _occupancyText;
        private set => SetProperty(ref _occupancyText, value);
    }

    // --- Lista de espera ---

    public ObservableCollection<WaitlistEntryDto> WaitlistEntries { get; } = new();

    private WaitlistEntryDto? _selectedWaitlistEntry;
    public WaitlistEntryDto? SelectedWaitlistEntry
    {
        get => _selectedWaitlistEntry;
        set => SetProperty(ref _selectedWaitlistEntry, value);
    }

    public ObservableCollection<UserDto> WaitlistCandidates { get; } = new();

    private UserDto? _selectedWaitlistCandidate;
    public UserDto? SelectedWaitlistCandidate
    {
        get => _selectedWaitlistCandidate;
        set => SetProperty(ref _selectedWaitlistCandidate, value);
    }

    private bool _isBusy;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RemoveStudentCommand { get; }
    public ICommand RemoveFromWaitlistCommand { get; }
    public ICommand AddToWaitlistCommand { get; }

    public AddActivityViewModel(
        CreateActivityUseCase createActivityUseCase,
        UpdateActivityUseCase updateActivityUseCase,
        GetActivityUseCase getActivityUseCase,
        GetUsersUseCase getUsersUseCase,
        RemoveUserFromActivityUseCase removeUserFromActivityUseCase,
        EnrollUserUseCase enrollUserUseCase,
        AddToWaitlistUseCase addToWaitlistUseCase,
        GetWaitlistUseCase getWaitlistUseCase,
        RemoveFromWaitlistUseCase removeFromWaitlistUseCase,
        WaitlistPromotionService waitlistPromotionService,
        ActivityScheduleDto? activityToEdit = null)
    {
        _createActivityUseCase = createActivityUseCase;
        _updateActivityUseCase = updateActivityUseCase;
        _getActivityUseCase = getActivityUseCase;
        _getUsersUseCase = getUsersUseCase;
        _removeUserFromActivityUseCase = removeUserFromActivityUseCase;
        _enrollUserUseCase = enrollUserUseCase;
        _addToWaitlistUseCase = addToWaitlistUseCase;
        _getWaitlistUseCase = getWaitlistUseCase;
        _removeFromWaitlistUseCase = removeFromWaitlistUseCase;
        _waitlistPromotionService = waitlistPromotionService;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());
        RemoveStudentCommand = new RelayCommand(
            async _ => await RemoveStudentAsync(),
            _ => !_isBusy && SelectedEnrolledStudent != null && IsEditing);
        RemoveFromWaitlistCommand = new RelayCommand(
            async _ => await RemoveFromWaitlistAsync(),
            _ => !_isBusy && SelectedWaitlistEntry != null && IsEditing);
        AddToWaitlistCommand = new RelayCommand(
            async _ => await AddToWaitlistAsync(),
            _ => !_isBusy && SelectedWaitlistCandidate != null && IsEditing);

        PopulateTimes();

        if (activityToEdit != null)
        {
            _editingActivityId = activityToEdit.Id;
            _savedActivity = activityToEdit;
            _name = activityToEdit.Name;
            _classRoom = activityToEdit.Classroom;
            _maxStudents = activityToEdit.MaxStudents;
            string day = activityToEdit.Date.ToString("dddd", SpanishCulture);
            _selectedDay = char.ToUpper(day[0]) + day.Substring(1);
            _selectedTime = activityToEdit.Date.ToString("HH:mm");
            ShowStudents(activityToEdit);
        }
        else
        {
            _maxStudents = 10;
        }

        _ = LoadUsersAsync(activityToEdit?.Tutor);
    }

    private void PopulateTimes()
    {
        DateTime startTime = DateTime.Today.AddHours(12);
        DateTime endTime = DateTime.Today.AddHours(21);

        while (startTime <= endTime)
        {
            Times.Add(startTime.ToString("HH:mm"));
            startTime = startTime.AddMinutes(30);
        }
    }

    private async Task LoadUsersAsync(string? tutorToSelect = null)
    {
        try
        {
            _allUsers = (await _getUsersUseCase.ExecuteAsync()).OrderBy(u => u.FullName).ToList();

            Tutors.Clear();
            foreach (UserDto user in _allUsers.Where(u => u.IsTutor))
            {
                Tutors.Add(user);
                if (tutorToSelect != null && user.FullName == tutorToSelect)
                    SelectedTutor = user;
            }

            if (IsEditing)
                await RefreshFromDatabaseAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando usuarios: {ex.Message}");
        }
    }

    private async Task RefreshFromDatabaseAsync()
    {
        if (!_editingActivityId.HasValue) return;

        ActivityScheduleDto? activity = await _getActivityUseCase.ExecuteByIdAsync(_editingActivityId.Value);
        if (activity != null)
        {
            _savedActivity = activity;
            ShowStudents(activity);
        }

        List<WaitlistEntryDto> entries = await _getWaitlistUseCase.ExecuteAsync(_editingActivityId.Value);
        WaitlistEntries.Clear();
        foreach (WaitlistEntryDto entry in entries)
            WaitlistEntries.Add(entry);

        HashSet<int> excluded = EnrolledStudents.Select(s => s.Id)
            .Concat(WaitlistEntries.Select(w => w.UserId))
            .ToHashSet();

        WaitlistCandidates.Clear();
        foreach (UserDto user in _allUsers.Where(u => !excluded.Contains(u.Id)))
            WaitlistCandidates.Add(user);
    }

    private void ShowStudents(ActivityScheduleDto activity)
    {
        EnrolledStudents.Clear();
        foreach (EnrolledStudentDto student in activity.EnrolledStudents)
            EnrolledStudents.Add(student);

        OccupancyText = activity.IsFull
            ? $"{activity.Occupancy} · COMPLETA"
            : activity.Occupancy;
    }

    private async Task RemoveStudentAsync()
    {
        if (SelectedEnrolledStudent is not { } student || !_editingActivityId.HasValue) return;

        MessageBoxResult confirm = MessageBox.Show(
            $"¿Eliminar a \"{student.FullName}\" de esta clase?",
            "Confirmar baja",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        await RunAsync(async () =>
        {
            await _removeUserFromActivityUseCase.ExecuteAsync(student.Id, _editingActivityId.Value);
            WasSaved = true;
            await _waitlistPromotionService.OfferPromotionsAsync(_editingActivityId.Value);
        }, "Error al eliminar al alumno");
    }

    private async Task RemoveFromWaitlistAsync()
    {
        if (SelectedWaitlistEntry is not { } entry || !_editingActivityId.HasValue) return;

        MessageBoxResult confirm = MessageBox.Show(
            $"¿Quitar a \"{entry.FullName}\" de la lista de espera?",
            "Confirmar baja de lista de espera",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        await RunAsync(async () =>
        {
            await _removeFromWaitlistUseCase.ExecuteAsync(entry.UserId, _editingActivityId.Value);
            WasSaved = true;
        }, "Error al quitar de la lista de espera");
    }

    private async Task AddToWaitlistAsync()
    {
        if (SelectedWaitlistCandidate is not { } user || !_editingActivityId.HasValue) return;
        int activityId = _editingActivityId.Value;

        await RunAsync(async () =>
        {
            ActivityScheduleDto? current = await _getActivityUseCase.ExecuteByIdAsync(activityId);
            if (current == null) return;

            if (!current.IsFull)
            {
                // Con plazas libres no tiene sentido esperar: se ofrece inscribirlo directamente
                MessageBoxResult answer = MessageBox.Show(
                    $"La clase todavía tiene plazas libres ({current.Occupancy}).\n\n" +
                    $"¿Inscribir directamente a {user.FullName}?",
                    "Hay plazas libres",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (answer != MessageBoxResult.Yes) return;

                await _enrollUserUseCase.ExecuteAsync(user.Id, activityId);
            }
            else
            {
                await _addToWaitlistUseCase.ExecuteAsync(user.Id, activityId);
            }

            WasSaved = true;
            SelectedWaitlistCandidate = null;
        }, "Error al apuntar a la lista de espera");
    }

    private async Task RunAsync(Func<Task> action, string errorTitle)
    {
        _isBusy = true;
        try
        {
            await action();
        }
        catch (DomainException ex)
        {
            MessageBox.Show(ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{errorTitle}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
            try
            {
                await RefreshFromDatabaseAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar la ficha: {ex.Message}");
            }
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private bool CanSave()
    {
        return !_isBusy
            && !string.IsNullOrWhiteSpace(Name)
            && SelectedTutor != null
            && !string.IsNullOrWhiteSpace(Classroom)
            && !string.IsNullOrWhiteSpace(SelectedDay)
            && !string.IsNullOrWhiteSpace(SelectedTime)
            && MaxStudents > 0;
    }

    private async Task SaveAsync()
    {
        _isBusy = true;
        try
        {
            ActivityDateCalculator dateCalculator = new ActivityDateCalculator();
            DateTime date = dateCalculator.GetNextOccurrence(SelectedDay, SelectedTime);

            if (_editingActivityId.HasValue)
            {
                await _updateActivityUseCase.ExecuteAsync(new UpdateActivityDto
                {
                    Id = _editingActivityId.Value,
                    Name = Name.Trim(),
                    Tutor = SelectedTutor!.FullName,
                    Classroom = Classroom.Trim(),
                    MaxStudents = MaxStudents,
                    Date = date
                });
                WasSaved = true;

                // Si se ha subido el aforo, puede haber sitio para los que esperan
                await _waitlistPromotionService.OfferPromotionsAsync(_editingActivityId.Value);
            }
            else
            {
                await _createActivityUseCase.ExecuteAsync(new CreateActivityDto
                {
                    Name = Name.Trim(),
                    Tutor = SelectedTutor!.FullName,
                    Classroom = Classroom.Trim(),
                    MaxStudents = MaxStudents,
                    Date = date
                });
                WasSaved = true;
            }

            CloseAction?.Invoke();
        }
        catch (DuplicateEntityException ex)
        {
            MessageBox.Show(ex.Message, "Actividad duplicada", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (DomainException ex)
        {
            MessageBox.Show(ex.Message, "No se puede guardar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar la actividad: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isBusy = false;
        }
    }

    private void Cancel()
    {
        CloseAction?.Invoke();
    }
}
