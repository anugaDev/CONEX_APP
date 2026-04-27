using System.Collections.ObjectModel;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class AddActivityViewModel : ViewModelBase
{
    private readonly CreateActivityUseCase _createActivityUseCase;
    private readonly GetUsersUseCase _getUsersUseCase;

    public Action? CloseAction { get; set; }

    public bool WasSaved { get; private set; }

    private string _name = string.Empty;

    private string _tutor = string.Empty;

    private string _classRoom = string.Empty;

    private DateTime _date = DateTime.Now;
    
    public string FullName
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
    
    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddActivityViewModel(CreateActivityUseCase createActivityUseCase, GetUsersUseCase getUsersUseCase)
    {
        _createActivityUseCase = createActivityUseCase;
        _getUsersUseCase = getUsersUseCase;
        
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());

        _ = LoadTutorsAsync();
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
        return !string.IsNullOrWhiteSpace(FullName) && SelectedTutor != null 
                                                    && !string.IsNullOrWhiteSpace(Classroom);
    }
    

    private async Task SaveAsync()
    {
        CreateActivityDto dto = new CreateActivityDto() { Name = FullName, Tutor = SelectedTutor!.FullName, Classroom = _classRoom, Date = _date };
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
