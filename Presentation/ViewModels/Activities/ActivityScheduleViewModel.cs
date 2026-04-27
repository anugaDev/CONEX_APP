using System.Collections.ObjectModel;
using System.Windows.Input;
using CONEX_APP.Application.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Views.Users;
using AddActivityWindow = CONEX_APP.Presentation.Views.Activities.AddActivityWindow;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class ActivityScheduleViewModel : ViewModelBase
{
    private readonly GetActivityUseCase _getActivitiesUseCase;

    private readonly CreateActivityUseCase _createActivityUseCase;

    private readonly GetUsersUseCase _getUsersUseCase;

    public ObservableCollection<ActivityScheduleDto> ActivitySchedule { get; set; }

    public ICommand OpenAddUserWindowCommand { get; }

    public ICommand GoBackCommand { get; }

    public ActivityScheduleViewModel(GetActivityUseCase getActivitiesUseCase, CreateActivityUseCase createActivityUseCase, GetUsersUseCase getUsersUseCase, Action goBack)
    {
        _getActivitiesUseCase = getActivitiesUseCase;
        _createActivityUseCase = createActivityUseCase;
        _getUsersUseCase = getUsersUseCase;
        ActivitySchedule = new ObservableCollection<ActivityScheduleDto>();
        
        OpenAddUserWindowCommand = new RelayCommand(_ => OpenAddUserWindow());
        GoBackCommand = new RelayCommand(_ => goBack());

        _ = LoadUsersAsync();
    }

    private void OpenAddUserWindow()
    {
        var addUserViewModel = new AddActivityViewModel(_createActivityUseCase, _getUsersUseCase);
        var addUserWindow = new AddActivityWindow(addUserViewModel);
        
        addUserWindow.ShowDialog();

        if (addUserViewModel.WasSaved)
        {
            _ = LoadUsersAsync();
        }
    }

    public async Task LoadUsersAsync()
    {
        try
        {
            var usersFromDb = await _getActivitiesUseCase.ExecuteAsync();
            ActivitySchedule.Clear();
            foreach (var activity in usersFromDb)
            {
                ActivitySchedule.Add(activity);
            }
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error cargando usuarios: {ex.Message}");
        }
    }

}