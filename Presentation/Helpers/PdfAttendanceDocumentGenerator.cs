using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CONEX_APP.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers;

public class PdfAttendanceDocumentGenerator
{
    private readonly ActivityDateCalculator _dateCalculator;

    public PdfAttendanceDocumentGenerator(ActivityDateCalculator dateCalculator)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _dateCalculator = dateCalculator;
    }

    public string GeneratePdf(ActivityScheduleDto activity)
    {
        DateTime firstDate = _dateCalculator.GetNextOccurrenceFromDate(activity.Date);
        DateTime[] dates = Enumerable.Range(0, 4).Select(i => firstDate.AddDays(i * 7)).ToArray();
        string dayOfWeek = GetLocalizedDayName(activity.Date.DayOfWeek);
        string filePath = BuildFilePath(activity, dates[0]);

        Document document = Document.Create(container =>
            container.Page(page => ConfigurePage(page, activity, dayOfWeek, dates)));

        document.GeneratePdf(filePath);
        return filePath;
    }

    private void ConfigurePage(PageDescriptor page, ActivityScheduleDto activity, string dayOfWeek, DateTime[] dates)
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));
        page.Header().Element(c => ComposeHeader(c, activity, dayOfWeek));
        page.Content().Element(c => ComposeContent(c, activity, dates));
        page.Footer().AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }

    private void ComposeHeader(IContainer container, ActivityScheduleDto activity, string dayOfWeek)
    {
        container.Row(row => row.RelativeItem().Column(column =>
        {
            column.Item().Text($"Hoja de Asistencia: {activity.Name}").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5).Text(text => BuildHeaderText(text, activity, dayOfWeek));
        }));
    }

    private void BuildHeaderText(TextDescriptor text, ActivityScheduleDto activity, string dayOfWeek)
    {
        text.Span("Día: ").SemiBold();
        text.Span($"{dayOfWeek}\n");
        text.Span("Hora: ").SemiBold();
        text.Span($"{activity.Date:HH:mm}\n");
        text.Span("Aula/Modalidad: ").SemiBold();
        text.Span($"{activity.Classroom}\n");
        text.Span("Enseñante: ").SemiBold();
        text.Span(activity.Tutor);
    }

    private void ComposeContent(IContainer container, ActivityScheduleDto activity, DateTime[] dates)
    {
        List<string> sortedStudents = activity.EnrolledStudentNames.OrderBy(name => name).ToList();
        container.PaddingVertical(1, Unit.Centimetre).Table(table => BuildAttendanceTable(table, sortedStudents, dates));
    }

    private void BuildAttendanceTable(TableDescriptor table, List<string> students, DateTime[] dates)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(3);
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn();
            columns.RelativeColumn();
        });

        table.Header(header =>
        {
            header.Cell().Element(HeaderCell).Text("Alumno").SemiBold();
            for (int i = 0; i < 4; i++)
            {
                header.Cell().Element(HeaderCell).AlignCenter().Text(dates[i].ToString("dd/MM")).SemiBold();
            }
        });

        if (students.Count == 0)
        {
            table.Cell().ColumnSpan(5).Padding(5).Text("No hay alumnos matriculados en esta clase.").Italic();
        }
        else
        {
            BuildStudentRows(table, students);
        }
    }

    private void BuildStudentRows(TableDescriptor table, List<string> students)
    {
        foreach (string student in students)
        {
            table.Cell().Element(DataCell).Text(student);
            for (int i = 0; i < 4; i++)
            {
                table.Cell().Element(DataCell);
            }
        }
    }

    private string BuildFilePath(ActivityScheduleDto activity, DateTime firstDate)
    {
        string sanitizedName = string.Join("_", activity.Name.Split(Path.GetInvalidFileNameChars()));
        string fileName = $"Asistencia_{sanitizedName}_{firstDate:dd-MM-yyyy}.pdf";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
    }

    private string GetLocalizedDayName(DayOfWeek day)
    {
        System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("es-ES");
        string name = culture.DateTimeFormat.GetDayName(day);
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    private IContainer HeaderCell(IContainer container) =>
        container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);

    private IContainer DataCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);
}
