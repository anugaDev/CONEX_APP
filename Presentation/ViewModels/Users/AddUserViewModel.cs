using System;
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

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
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
        return !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Email);
    }

    private async Task SaveAsync()
    {
        var dto = new CreateUserDto { FullName = FullName, Email = Email };
        await _createUserUseCase.ExecuteAsync(dto);
        
        WasSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        WasSaved = false;
        CloseAction?.Invoke();
    }
}
