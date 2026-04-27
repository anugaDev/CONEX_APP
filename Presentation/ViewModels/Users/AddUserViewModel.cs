using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;

namespace CONEX_APP.Presentation.ViewModels.Users;

public class AddUserViewModel : ViewModelBase
{
    private readonly CreateUserUseCase _createUserUseCase;
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

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddUserViewModel(CreateUserUseCase createUserUseCase)
    {
        _createUserUseCase = createUserUseCase;
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email);
    }

    private async Task SaveAsync()
    {
        CreateUserDto dto = GetNewUSerDto();
        await _createUserUseCase.ExecuteAsync(dto);
        WasSaved = true;
        CloseAction?.Invoke();
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
            IsPartner = IsPartner
        };
    }

    private void Cancel()
    {
        WasSaved = false;
        CloseAction?.Invoke();
    }
}
