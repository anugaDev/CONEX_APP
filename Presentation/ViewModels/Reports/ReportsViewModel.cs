using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CONEX_APP.MainApplication.DTOs;
using CONEX_APP.MainApplication.UseCases.Activities;
using CONEX_APP.MainApplication.UseCases.Users;
using CONEX_APP.Presentation.Commands;
using CONEX_APP.Presentation.Helpers.Reports;

namespace CONEX_APP.Presentation.ViewModels.Reports;

public class ReportsViewModel : ViewModelBase
{
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly GetActivityUseCase _getActivitiesUseCase;

    public ICommand GoBackCommand { get; }
    public ICommand PrintPartnersCommand { get; }
    public ICommand PrintTutorsCommand { get; }
    public ICommand PrintTotalClassesCommand { get; }
    public ICommand PrintUsersPerClassCommand { get; }

    public ReportsViewModel(GetUsersUseCase getUsersUseCase, GetActivityUseCase getActivitiesUseCase, Action goBack)
    {
        _getUsersUseCase = getUsersUseCase;
        _getActivitiesUseCase = getActivitiesUseCase;
        
        GoBackCommand = new RelayCommand(_ => goBack());
        PrintPartnersCommand = new RelayCommand(async _ => await PrintPartnersAsync());
        PrintTutorsCommand = new RelayCommand(async _ => await PrintTutorsAsync());
        PrintTotalClassesCommand = new RelayCommand(async _ => await PrintTotalClassesAsync());
        PrintUsersPerClassCommand = new RelayCommand(async _ => await PrintUsersPerClassAsync());
    }

    private async System.Threading.Tasks.Task PrintPartnersAsync()
    {
        try
        {
            var users = await _getUsersUseCase.ExecuteAsync();
            var partners = users.Where(u => u.IsPartner).OrderBy(u => u.Name).ToList();
            
            var generator = new PartnersReportGenerator();
            string pdfPath = generator.GenerateReport(partners);
            OpenPdf(pdfPath);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar el listado: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task PrintTutorsAsync()
    {
        try
        {
            var users = await _getUsersUseCase.ExecuteAsync();
            var tutors = users.Where(u => u.IsTutor).OrderBy(u => u.Name).ToList();
            
            var generator = new TutorsReportGenerator();
            string pdfPath = generator.GenerateReport(tutors);
            OpenPdf(pdfPath);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar el listado: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task PrintTotalClassesAsync()
    {
        try
        {
            var classes = await _getActivitiesUseCase.ExecuteAsync();
            var sortedClasses = classes.OrderBy(c => c.Name).ToList();
            
            var generator = new TotalClassesReportGenerator();
            string pdfPath = generator.GenerateReport(sortedClasses);
            OpenPdf(pdfPath);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar el listado: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task PrintUsersPerClassAsync()
    {
        try
        {
            var classes = await _getActivitiesUseCase.ExecuteAsync();
            var sortedClasses = classes.OrderBy(c => c.Name).ToList();
            
            var generator = new UsersPerClassReportGenerator();
            string pdfPath = generator.GenerateReport(sortedClasses);
            OpenPdf(pdfPath);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar el listado: {ex.Message}");
        }
    }

    private void OpenPdf(string pdfPath)
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = pdfPath,
            UseShellExecute = true
        };
        System.Diagnostics.Process.Start(psi);
    }
}
