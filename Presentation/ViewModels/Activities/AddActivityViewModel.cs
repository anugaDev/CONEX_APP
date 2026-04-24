using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class AddActivityViewModel : ViewModelBase
{
    private readonly CreateActivityUseCase _createActivityUseCase;

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

    public string Tutor
    {
        get => _tutor;
        set => SetProperty(ref _tutor, value);
    }

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

    public AddActivityViewModel(CreateActivityUseCase createActivityUseCase)
    {
        _createActivityUseCase = createActivityUseCase;
        
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Tutor) 
                                                    && !string.IsNullOrWhiteSpace(Classroom);
    }
    

    private async Task SaveAsync()
    {
        CreateActivityDto dto = new CreateActivityDto() { Name = FullName, Tutor = _tutor, Classroom = _classRoom, Date = _date };
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
