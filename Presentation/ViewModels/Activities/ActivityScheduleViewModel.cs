using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers;
using CONEX_APP.Presentation.Services;
using AddActivityWindow = CONEX_APP.Presentation.Views.Activities.AddActivityWindow;

namespace CONEX_APP.Presentation.ViewModels.Activities;

public class ActivityScheduleViewModel : ViewModelBase
{
    private readonly AppServices _services;

    public ObservableCollection<ActivityScheduleDto> ActivitySchedule { get; set; }

    public ICollectionView ActivityScheduleView { get; }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ActivityScheduleView.Refresh();
        }
    }

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
    public ICommand ClearSearchCommand { get; }

    public ActivityScheduleViewModel(AppServices services, Action goBack, Action goToUsers)
    {
        _services = services;
        ActivitySchedule = new ObservableCollection<ActivityScheduleDto>();
        ActivityScheduleView = CollectionViewSource.GetDefaultView(ActivitySchedule);
        ActivityScheduleView.Filter = FilterActivity;

        OpenAddUserWindowCommand = new RelayCommand(_ => OpenAddActivityWindow());
        EditActivityCommand = new RelayCommand(_ => EditActivity(), _ => SelectedActivity != null);
        DeleteActivityCommand = new RelayCommand(async _ => await DeleteActivityAsync(), _ => SelectedActivity != null);
        PrintAttendanceCommand = new RelayCommand(_ => PrintAttendance(), _ => SelectedActivity != null);
        GoBackCommand = new RelayCommand(_ => goBack());
        GoToUsersCommand = new RelayCommand(_ => goToUsers());
        ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);

        _ = LoadActivitiesAsync();
    }

    private void OpenAddActivityWindow() => ShowActivityForm(null);

    private void EditActivity()
    {
        if (SelectedActivity != null)
            ShowActivityForm(SelectedActivity);
    }

    private void ShowActivityForm(ActivityScheduleDto? activityToEdit)
    {
        AddActivityViewModel vm = _services.CreateActivityForm(activityToEdit);
        AddActivityWindow win = new AddActivityWindow(vm) { Owner = System.Windows.Application.Current.MainWindow };
        win.ShowDialog();

        if (vm.WasSaved)
            _ = LoadActivitiesAsync();
    }

    private bool FilterActivity(object obj)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
            return true;

        if (obj is not ActivityScheduleDto activity)
            return false;

        string search = _searchText.Trim().ToLowerInvariant();
        return activity.Name.ToLowerInvariant().Contains(search)
            || activity.Tutor.ToLowerInvariant().Contains(search)
            || activity.Classroom.ToLowerInvariant().Contains(search);
    }

    public async Task LoadActivitiesAsync()
    {
        try
        {
            IEnumerable<ActivityScheduleDto> activities = await _services.GetActivities.ExecuteAsync();
            ActivitySchedule.Clear();
            foreach (ActivityScheduleDto activity in activities)
                ActivitySchedule.Add(activity);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando actividades: {ex.Message}");
        }
    }

    private async Task DeleteActivityAsync()
    {
        if (SelectedActivity is not { } activity) return;

        string details = activity.EnrolledStudentsCount > 0 || activity.WaitlistCount > 0
            ? $"\n\nSe darán de baja sus {activity.EnrolledStudentsCount} alumnos y se borrará su lista de espera ({activity.WaitlistCount})."
            : string.Empty;

        MessageBoxResult result = MessageBox.Show(
            $"¿Seguro que quieres eliminar la clase \"{activity.Name}\"?{details}",
            "Confirmar Eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _services.DeleteActivity.ExecuteAsync(activity.Id);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar la clase: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        await LoadActivitiesAsync();
    }

    private void PrintAttendance()
    {
        if (SelectedActivity == null) return;

        try
        {
            ActivityDateCalculator dateCalculator = new ActivityDateCalculator();
            PdfAttendanceDocumentGenerator generator = new PdfAttendanceDocumentGenerator(dateCalculator);
            string pdfPath = generator.GeneratePdf(SelectedActivity);

            Process.Start(new ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
