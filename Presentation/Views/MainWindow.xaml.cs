using System.Windows;
using CONEX_APP.Infrastructure.Data.Context;
using CONEX_APP.Presentation.Services;
using CONEX_APP.Presentation.ViewModels;
using CONEX_APP.Presentation.ViewModels.Activities;
using CONEX_APP.Presentation.ViewModels.Reports;
using CONEX_APP.Presentation.ViewModels.Users;
using CONEX_APP.Presentation.Views.Settings;

namespace CONEX_APP.Presentation.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private readonly AppServices _services;

    public MainWindow()
    {
        InitializeComponent();

        _services = new AppServices(new AppDbContextFactory());

        _mainViewModel = new MainViewModel();
        DataContext = _mainViewModel;

        NavigateToHome();
    }

    private void NavigateToHome()
    {
        _mainViewModel.CurrentViewModel = new HomeViewModel(
            navigateToUsers: NavigateToUsers,
            navigateToClasses: NavigateToClasses,
            navigateToReports: NavigateToReports,
            exitAction: () => this.Close(),
            openSettings: OpenSettingsFromHome
        );
    }

    private void OpenSettingsFromHome()
    {
        SettingsWindow settingsWindow = SettingsWindow.Create(_services.GetAppSettings, _services.SaveAppSettings);
        settingsWindow.Owner = this;
        settingsWindow.ShowDialog();
    }

    private void NavigateToReports()
    {
        _mainViewModel.CurrentViewModel = new ReportsViewModel(_services, goBack: NavigateToHome);
    }

    private void NavigateToUsers()
    {
        _mainViewModel.CurrentViewModel = new UserListViewModel(_services, goBack: NavigateToHome, goToActivities: NavigateToClasses);
    }

    private void NavigateToClasses()
    {
        _mainViewModel.CurrentViewModel = new ActivityScheduleViewModel(_services, goBack: NavigateToHome, goToUsers: NavigateToUsers);
    }
}
