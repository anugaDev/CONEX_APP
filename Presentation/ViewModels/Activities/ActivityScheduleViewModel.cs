using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CONEX_APP.Application.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using AddActivityWindow = CONEX_APP.Presentation.Views.Activities.AddActivityWindow;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class ActivityScheduleViewModel : ViewModelBase
{
    private readonly CreateActivityUseCase _createActivityUseCase;

    private readonly UpdateActivityUseCase _updateActivityUseCase;

    private readonly DeleteActivityUseCase _deleteActivityUseCase;

    private readonly GetActivityUseCase _getActivitiesUseCase;

    private readonly GetUsersUseCase _getUsersUseCase;

    public ObservableCollection<ActivityScheduleDto> ActivitySchedule { get; set; }

    private ActivityScheduleDto? _selectedActivity;
    public ActivityScheduleDto? SelectedActivity
    {
        get => _selectedActivity;
        set => SetProperty(ref _selectedActivity, value);
    }

    public ICommand OpenAddUserWindowCommand { get; }

    public ICommand EditActivityCommand { get; }

    public ICommand DeleteActivityCommand { get; }

    public ICommand PrintAttendanceCommand { get; }

    public ICommand GoBackCommand { get; }

    public ICommand GoToUsersCommand { get; }

    public ActivityScheduleViewModel(GetActivityUseCase getActivitiesUseCase, CreateActivityUseCase createActivityUseCase, UpdateActivityUseCase updateActivityUseCase, DeleteActivityUseCase deleteActivityUseCase, GetUsersUseCase getUsersUseCase, Action goBack, Action goToUsers)
    {
        _getActivitiesUseCase = getActivitiesUseCase;
        _createActivityUseCase = createActivityUseCase;
        _updateActivityUseCase = updateActivityUseCase;
        _deleteActivityUseCase = deleteActivityUseCase;
        _getUsersUseCase = getUsersUseCase;
        ActivitySchedule = new ObservableCollection<ActivityScheduleDto>();
        
        OpenAddUserWindowCommand = new RelayCommand(_ => OpenAddActivityWindow());
        EditActivityCommand = new RelayCommand(_ => EditActivity(), _ => SelectedActivity != null);
        DeleteActivityCommand = new RelayCommand(async _ => await DeleteActivityAsync(), _ => SelectedActivity != null);
        PrintAttendanceCommand = new RelayCommand(_ => PrintAttendance(), _ => SelectedActivity != null);
        GoBackCommand = new RelayCommand(_ => goBack());
        GoToUsersCommand = new RelayCommand(_ => goToUsers());

        _ = LoadActivitiesAsync();
    }

    private void OpenAddActivityWindow()
    {
        AddActivityViewModel addUserViewModel = new AddActivityViewModel(_createActivityUseCase, _updateActivityUseCase, _getUsersUseCase);
        AddActivityWindow addUserWindow = new AddActivityWindow(addUserViewModel);
        
        addUserWindow.ShowDialog();

        if (!addUserViewModel.WasSaved)
        {
            return;
        }

        _ = LoadActivitiesAsync();
    }

    private void EditActivity()
    {
        if (SelectedActivity != null)
        {
            AddActivityViewModel addActivityViewModel = new AddActivityViewModel(_createActivityUseCase, _updateActivityUseCase, _getUsersUseCase, SelectedActivity);
            AddActivityWindow addActivityWindow = new AddActivityWindow(addActivityViewModel);
            
            addActivityWindow.ShowDialog();

            if (!addActivityViewModel.WasSaved)
            {
                return;
            }

            _ = LoadActivitiesAsync();
        }
    }

    public async Task LoadActivitiesAsync()
    {
        try
        {
            IEnumerable<ActivityScheduleDto> usersFromDb = await _getActivitiesUseCase.ExecuteAsync();
            ActivitySchedule.Clear();
            foreach (ActivityScheduleDto activity in usersFromDb)
            {
                ActivitySchedule.Add(activity);
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error cargando actividades: {ex.Message}");
        }
    }

    private async Task DeleteActivityAsync()
    {
        if (SelectedActivity != null)
        {
            MessageBoxResult result = MessageBox.Show(
                $"¿Seguro que quieres eliminar la clase de {SelectedActivity.Name}?", 
                "Confirmar Eliminación", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                await _deleteActivityUseCase.ExecuteAsync(SelectedActivity.Id);
                await LoadActivitiesAsync();
            }
        }
    }

    private void PrintAttendance()
    {
        if (SelectedActivity != null)
        {
            try
            {
                string pdfPath = Helpers.PdfAttendanceDocumentGenerator.GeneratePdf(SelectedActivity);
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}