using System.Windows;
using CONEX_APP.MainApplication.UseCases.Settings;
using CONEX_APP.Presentation.ViewModels.Settings;

namespace CONEX_APP.Presentation.Views.Settings;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>Factory que crea la ventana y pasa su propio Close() como acción de cierre.</summary>
    public static SettingsWindow Create(GetAppSettingsUseCase getSettingsUseCase, SaveAppSettingsUseCase saveSettingsUseCase)
    {
        SettingsWindow window = new SettingsWindow(null!);
        SettingsViewModel vm = new SettingsViewModel(getSettingsUseCase, saveSettingsUseCase, () => window.Close());
        window.DataContext = vm;
        return window;
    }
}
