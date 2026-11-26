using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers.Reports;
using CONEX_APP.Presentation.Services;

namespace CONEX_APP.Presentation.ViewModels.Reports;

public class ReportsViewModel : ViewModelBase
{
    private readonly AppServices _services;

    public ICommand GoBackCommand { get; }
    public ICommand PrintPartnersCommand { get; }
    public ICommand PrintTutorsCommand { get; }
    public ICommand PrintTotalClassesCommand { get; }
    public ICommand PrintUsersPerClassCommand { get; }
    public ICommand PrintWaitlistsCommand { get; }

    public ReportsViewModel(AppServices services, Action goBack)
    {
        _services = services;

        GoBackCommand = new RelayCommand(_ => goBack());
        PrintPartnersCommand = new RelayCommand(async _ => await PrintPartnersAsync());
        PrintTutorsCommand = new RelayCommand(async _ => await PrintTutorsAsync());
        PrintTotalClassesCommand = new RelayCommand(async _ => await PrintTotalClassesAsync());
        PrintUsersPerClassCommand = new RelayCommand(async _ => await PrintUsersPerClassAsync());
        PrintWaitlistsCommand = new RelayCommand(async _ => await PrintWaitlistsAsync());
    }

    private Task PrintPartnersAsync() => GenerateAsync(async () =>
    {
        IEnumerable<UserDto> users = await _services.GetUsers.ExecuteAsync();
        List<UserDto> partners = users.Where(u => u.IsPartner).OrderBy(u => u.Name).ToList();
        return new UserListReportGenerator("Listado de Socios", "Listado_Socios").GenerateReport(partners);
    });

    private Task PrintTutorsAsync() => GenerateAsync(async () =>
    {
        IEnumerable<UserDto> users = await _services.GetUsers.ExecuteAsync();
        List<UserDto> tutors = users.Where(u => u.IsTutor).OrderBy(u => u.Name).ToList();
        return new UserListReportGenerator("Listado de Enseñantes", "Listado_Enseñantes").GenerateReport(tutors);
    });

    private Task PrintTotalClassesAsync() => GenerateAsync(async () =>
    {
        IEnumerable<ActivityScheduleDto> classes = await _services.GetActivities.ExecuteAsync();
        return new TotalClassesReportGenerator().GenerateReport(classes.OrderBy(c => c.Name).ToList());
    });

    private Task PrintUsersPerClassAsync() => GenerateAsync(async () =>
    {
        IEnumerable<ActivityScheduleDto> classes = await _services.GetActivities.ExecuteAsync();
        return new UsersPerClassReportGenerator().GenerateReport(classes.OrderBy(c => c.Name).ToList());
    });

    private Task PrintWaitlistsAsync() => GenerateAsync(async () =>
    {
        List<ActivityWaitlistDto> waitlists = await _services.GetWaitlistReport.ExecuteAsync();
        return new WaitlistReportGenerator().GenerateReport(waitlists);
    });

    private static async Task GenerateAsync(Func<Task<string>> generate)
    {
        try
        {
            string pdfPath = await generate();
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar el listado: {ex.Message}");
        }
    }
}
