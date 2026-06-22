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
using CONEX_APP.Presentation.Helpers;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class AddActivityViewModel : ViewModelBase
{
    private readonly CreateActivityUseCase _createActivityUseCase;
    private readonly UpdateActivityUseCase _updateActivityUseCase;
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly RemoveUserFromActivityUseCase _removeUserFromActivityUseCase;

    private readonly int? _editingActivityId;

    public Action? CloseAction { get; set; }

    public bool WasSaved { get; private set; }

    private string _name = string.Empty;
    private string _classRoom = string.Empty;
    private int _maxStudents = 10;

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

    public ObservableCollection<EnrolledStudentDto> EnrolledStudents { get; } = new();

    private EnrolledStudentDto? _selectedEnrolledStudent;
    public EnrolledStudentDto? SelectedEnrolledStudent
    {
        get => _selectedEnrolledStudent;
        set => SetProperty(ref _selectedEnrolledStudent, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RemoveStudentCommand { get; }

    public AddActivityViewModel(
        CreateActivityUseCase createActivityUseCase,
        UpdateActivityUseCase updateActivityUseCase,
        GetUsersUseCase getUsersUseCase,
        RemoveUserFromActivityUseCase removeUserFromActivityUseCase,
        ActivityScheduleDto? activityToEdit = null)
    {
        _createActivityUseCase = createActivityUseCase;
        _updateActivityUseCase = updateActivityUseCase;
        _getUsersUseCase = getUsersUseCase;
        _removeUserFromActivityUseCase = removeUserFromActivityUseCase;

        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());
        RemoveStudentCommand = new RelayCommand(
            async _ => await RemoveStudentAsync(),
            _ => SelectedEnrolledStudent != null && _editingActivityId.HasValue);

        PopulateTimes();

        if (activityToEdit != null)
        {
            _editingActivityId = activityToEdit.Id;
            _name = activityToEdit.Name;
            _classRoom = activityToEdit.Classroom;
            _maxStudents = activityToEdit.MaxStudents;
            _selectedDay = activityToEdit.Date.ToString("dddd");
            _selectedDay = char.ToUpper(_selectedDay[0]) + _selectedDay.Substring(1);
            _selectedTime = activityToEdit.Date.ToString("HH:mm");

            foreach (EnrolledStudentDto student in activityToEdit.EnrolledStudents)
            {
                EnrolledStudents.Add(student);
            }
        }

        _ = LoadTutorsAsync(activityToEdit?.Tutor);
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

    private async Task LoadTutorsAsync(string? tutorToSelect = null)
    {
        try
        {
            IEnumerable<UserDto> users = await _getUsersUseCase.ExecuteAsync();
            Tutors.Clear();
            foreach (UserDto user in users.Where(u => u.IsTutor))
            {
                Tutors.Add(user);
                if (tutorToSelect != null && user.FullName == tutorToSelect)
                    SelectedTutor = user;
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error cargando tutores: {ex.Message}");
        }
    }

    private async Task RemoveStudentAsync()
    {
        if (SelectedEnrolledStudent == null || !_editingActivityId.HasValue) return;

        EnrolledStudentDto student = SelectedEnrolledStudent;

        MessageBoxResult confirm = MessageBox.Show(
            $"¿Eliminar a \"{student.FullName}\" de esta clase?",
            "Confirmar baja",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            await _removeUserFromActivityUseCase.ExecuteAsync(student.Id, _editingActivityId.Value);
            EnrolledStudents.Remove(student);
            SelectedEnrolledStudent = null;
            WasSaved = true;
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error al eliminar al alumno: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && SelectedTutor != null
            && !string.IsNullOrWhiteSpace(Classroom)
            && !string.IsNullOrWhiteSpace(SelectedDay)
            && !string.IsNullOrWhiteSpace(SelectedTime)
            && MaxStudents > 0;
    }

    private async Task SaveAsync()
    {
        try
        {
            ActivityDateCalculator dateCalculator = new ActivityDateCalculator();

            if (_editingActivityId.HasValue)
            {
                UpdateActivityDto dto = new UpdateActivityDto()
                {
                    Id = _editingActivityId.Value,
                    Name = Name,
                    Tutor = SelectedTutor!.FullName,
                    Classroom = _classRoom,
                    MaxStudents = _maxStudents,
                    Date = dateCalculator.GetNextOccurrence(SelectedDay, SelectedTime)
                };
                await _updateActivityUseCase.ExecuteAsync(dto);
            }
            else
            {
                CreateActivityDto dto = new CreateActivityDto()
                {
                    Name = Name,
                    Tutor = SelectedTutor!.FullName,
                    Classroom = _classRoom,
                    MaxStudents = _maxStudents,
                    Date = dateCalculator.GetNextOccurrence(SelectedDay, SelectedTime)
                };
                await _createActivityUseCase.ExecuteAsync(dto);
            }

            WasSaved = true;
            CloseAction?.Invoke();
        }
        catch (DuplicateEntityException ex)
        {
            MessageBox.Show(ex.Message, "Actividad duplicada", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Cancel()
    {
        WasSaved = false;
        CloseAction?.Invoke();
    }
}