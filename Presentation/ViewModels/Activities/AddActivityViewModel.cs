using System.Collections.ObjectModel;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class AddActivityViewModel : ViewModelBase
{
    private readonly CreateActivityUseCase _createActivityUseCase;
    private readonly GetUsersUseCase _getUsersUseCase;

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

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddActivityViewModel(CreateActivityUseCase createActivityUseCase, GetUsersUseCase getUsersUseCase)
    {
        _createActivityUseCase = createActivityUseCase;
        _getUsersUseCase = getUsersUseCase;
        
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());

        PopulateTimes();
        _ = LoadTutorsAsync();
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

    private async Task LoadTutorsAsync()
    {
        try
        {
            var users = await _getUsersUseCase.ExecuteAsync();
            Tutors.Clear();
            foreach (var user in users.Where(u => u.IsTutor))
            {
                Tutors.Add(user);
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

        CreateActivityDto dto = new CreateActivityDto() 
        { 
            Name = Name, 
            Tutor = SelectedTutor!.FullName, 
            Classroom = _classRoom, 
            MaxStudents = _maxStudents,
            Date = dateCalculator.GetNextOccurrence(SelectedDay, SelectedTime) 
        };
        await _createActivityUseCase.ExecuteAsync(dto);
        
        WasSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        WasSaved = false;
        CloseAction?.Invoke();
    }
}