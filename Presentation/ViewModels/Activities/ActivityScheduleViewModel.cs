using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CONEX_APP.Application.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Registrations;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers;
using AddActivityWindow = CONEX_APP.Presentation.Views.Activities.AddActivityWindow;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class ActivityScheduleViewModel : ViewModelBase
{
    private readonly CreateActivityUseCase _createActivityUseCase;
    private readonly UpdateActivityUseCase _updateActivityUseCase;
    private readonly DeleteActivityUseCase _deleteActivityUseCase;
    private readonly GetActivityUseCase _getActivitiesUseCase;
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly RemoveUserFromActivityUseCase _removeUserFromActivityUseCase;

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

    public ActivityScheduleViewModel(
        GetActivityUseCase getActivitiesUseCase,
        CreateActivityUseCase createActivityUseCase,
        UpdateActivityUseCase updateActivityUseCase,
        DeleteActivityUseCase deleteActivityUseCase,
        GetUsersUseCase getUsersUseCase,
        RemoveUserFromActivityUseCase removeUserFromActivityUseCase,
        Action goBack,
        Action goToUsers)
    {
        _getActivitiesUseCase = getActivitiesUseCase;
        _createActivityUseCase = createActivityUseCase;
        _updateActivityUseCase = updateActivityUseCase;
        _deleteActivityUseCase = deleteActivityUseCase;
        _getUsersUseCase = getUsersUseCase;
        _removeUserFromActivityUseCase = removeUserFromActivityUseCase;
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
        AddActivityViewModel vm = new AddActivityViewModel(
            _createActivityUseCase, _updateActivityUseCase,
            _getUsersUseCase, _removeUserFromActivityUseCase);
        AddActivityWindow win = new AddActivityWindow(vm);
        win.ShowDialog();

        if (vm.WasSaved)
            _ = LoadActivitiesAsync();
    }

    private void EditActivity()
    {
        if (SelectedActivity == null) return;

        AddActivityViewModel vm = new AddActivityViewModel(
            _createActivityUseCase, _updateActivityUseCase,
            _getUsersUseCase, _removeUserFromActivityUseCase,
            SelectedActivity);
        AddActivityWindow win = new AddActivityWindow(vm);
        win.ShowDialog();

        if (vm.WasSaved)
            _ = LoadActivitiesAsync();
    }

    public async Task LoadActivitiesAsync()
    {
        try
        {
            IEnumerable<ActivityScheduleDto> activities = await _getActivitiesUseCase.ExecuteAsync();
            ActivitySchedule.Clear();
            foreach (ActivityScheduleDto activity in activities)
                ActivitySchedule.Add(activity);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error cargando actividades: {ex.Message}");
        }
    }

    private async Task DeleteActivityAsync()
    {
        if (SelectedActivity == null) return;

        MessageBoxResult result = MessageBox.Show(
            $"¿Seguro que quieres eliminar la clase \"{SelectedActivity.Name}\"?",
            "Confirmar Eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            await _deleteActivityUseCase.ExecuteAsync(SelectedActivity.Id);
            await LoadActivitiesAsync();
        }
    }

    private void PrintAttendance()
    {
        if (SelectedActivity == null) return;

        try
        {
            ActivityDateCalculator dateCalculator = new ActivityDateCalculator();
            PdfAttendanceDocumentGenerator generator = new PdfAttendanceDocumentGenerator(dateCalculator);
            string pdfPath = generator.GeneratePdf(SelectedActivity);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}