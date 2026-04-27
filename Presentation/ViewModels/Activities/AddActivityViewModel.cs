using System.Collections.ObjectModel;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Application.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class AddActivityViewModel : ViewModelBase
{
    private readonly CreateActivityUseCase _createActivityUseCase;
    private readonly UpdateActivityUseCase _updateActivityUseCase;
    private readonly GetUsersUseCase _getUsersUseCase;

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

    private string _email = string.Empty;

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

    public ObservableCollection<string> EnrolledStudents { get; } = new();

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddActivityViewModel(CreateActivityUseCase createActivityUseCase, UpdateActivityUseCase updateActivityUseCase, GetUsersUseCase getUsersUseCase, ActivityScheduleDto? activityToEdit = null)
    {
        _createActivityUseCase = createActivityUseCase;
        _updateActivityUseCase = updateActivityUseCase;
        _getUsersUseCase = getUsersUseCase;
        
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());

        PopulateTimes();

        if (activityToEdit != null)
        {
            _editingActivityId = activityToEdit.Id;
            _name = activityToEdit.Name;
            _classRoom = activityToEdit.Classroom;
            _maxStudents = activityToEdit.MaxStudents;
            _selectedDay = activityToEdit.Date.ToString("dddd"); // Capitalize might be needed depending on culture, but combobox ignores it usually, actually culture might return "lunes". We use "Lunes".
            _selectedDay = char.ToUpper(_selectedDay[0]) + _selectedDay.Substring(1); 
            _selectedTime = activityToEdit.Date.ToString("HH:mm");

            foreach (var student in activityToEdit.EnrolledStudentNames)
            {
                EnrolledStudents.Add(student);
            }
        }

        _ = LoadTutorsAsync(activityToEdit?.Tutor);
    }

    private void PopulateTimes()
    {
        DateTime startTime = DateTime.Today.AddHours(12); // 12:00 PM
        DateTime endTime = DateTime.Today.AddHours(21);   // 9:00 PM
        
        while(startTime <= endTime) 
        {
            Times.Add(startTime.ToString("HH:mm"));
            startTime = startTime.AddMinutes(30);
        }
    }

    private async Task LoadTutorsAsync(string? tutorToSelect = null)
    {
        try
        {
            var users = await _getUsersUseCase.ExecuteAsync();
            Tutors.Clear();
            foreach (var user in users.Where(u => u.IsTutor))
            {
                Tutors.Add(user);
                if (tutorToSelect != null && user.FullName == tutorToSelect)
                {
                    SelectedTutor = user;
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error cargando tutores: {ex.Message}");
        }
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name) && SelectedTutor != null 
                                                    && !string.IsNullOrWhiteSpace(Classroom)
                                                    && !string.IsNullOrWhiteSpace(SelectedDay)
                                                    && !string.IsNullOrWhiteSpace(SelectedTime)
                                                    && MaxStudents > 0;
    }
    
    private async Task SaveAsync()
    {
        var dateCalculator = new ActivityDateCalculator();

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

    private void Cancel()
    {
        WasSaved = false;
        CloseAction?.Invoke();
    }
}