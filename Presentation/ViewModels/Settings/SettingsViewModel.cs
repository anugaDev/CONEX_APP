using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Settings;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.ViewModels;

namespace CONEX_APP.Presentation.ViewModels.Settings;

public class SettingsViewModel : ViewModelBase
{
    private readonly GetAppSettingsUseCase _getSettingsUseCase;
    private readonly SaveAppSettingsUseCase _saveSettingsUseCase;

    private int _renewalPeriodMonths;
    public int RenewalPeriodMonths
    {
        get => _renewalPeriodMonths;
        set => SetProperty(ref _renewalPeriodMonths, value);
    }

    private decimal _renewalCost;
    public decimal RenewalCost
    {
        get => _renewalCost;
        set => SetProperty(ref _renewalCost, value);
    }

    private decimal _classCost;
    public decimal ClassCost
    {
        get => _classCost;
        set => SetProperty(ref _classCost, value);
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CloseCommand { get; }

    public SettingsViewModel(GetAppSettingsUseCase getSettingsUseCase, SaveAppSettingsUseCase saveSettingsUseCase, Action closeAction)
    {
        _getSettingsUseCase = getSettingsUseCase;
        _saveSettingsUseCase = saveSettingsUseCase;

        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        CloseCommand = new RelayCommand(_ => closeAction());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            AppSettingsDto dto = await _getSettingsUseCase.ExecuteAsync();
            RenewalPeriodMonths = dto.RenewalPeriodMonths;
            RenewalCost = dto.RenewalCost;
            ClassCost = dto.ClassCost;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar la configuración: {ex.Message}";
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            if (RenewalPeriodMonths <= 0)
            {
                StatusMessage = "El período de renovación debe ser mayor que cero.";
                return;
            }

            AppSettingsDto dto = new AppSettingsDto
            {
                RenewalPeriodMonths = RenewalPeriodMonths,
                RenewalCost = RenewalCost,
                ClassCost = ClassCost
            };

            await _saveSettingsUseCase.ExecuteAsync(dto);
            StatusMessage = "✔ Configuración guardada correctamente.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al guardar: {ex.Message}";
        }
    }
}
