using System;
using System.IO;
using System.Linq;
using CONEX_APP.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers;

public static class PdfAttendanceDocumentGenerator
{
    public static string GeneratePdf(ActivityScheduleDto activity)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        DateTime nextDate = DateTime.Today;
        while (nextDate.DayOfWeek != activity.Date.DayOfWeek)
        {
            nextDate = nextDate.AddDays(1);
        }

        DateTime[] dates = new DateTime[4];
        for (int i = 0; i < 4; i++)
        {
            dates[i] = nextDate.AddDays(i * 7);
        }

        string dayOfWeek = new System.Globalization.CultureInfo("es-ES").DateTimeFormat.GetDayName(activity.Date.DayOfWeek);
        dayOfWeek = char.ToUpper(dayOfWeek[0]) + dayOfWeek.Substring(1);

        string sanitizedName = string.Join("_", activity.Name.Split(Path.GetInvalidFileNameChars()));
        string fileName = $"Asistencia_{sanitizedName}_{dates[0]:dd-MM-yyyy}.pdf";
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

        Document document = Document.Create(container =>
        {
            container.Page(page =>
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
            });
        });

        document.GeneratePdf(filePath);
        return filePath;
    }

    private static void ComposeHeader(IContainer container, ActivityScheduleDto activity, string dayOfWeek)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Hoja de Asistencia: {activity.Name}").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Día: ").SemiBold();
                    text.Span($"{dayOfWeek}\n");
                    
                    text.Span("Hora: ").SemiBold();
                    text.Span($"{activity.Date:HH:mm}\n");
                    
                    text.Span("Aula/Modalidad: ").SemiBold();
                    text.Span($"{activity.Classroom}\n");
                    
                    text.Span("Enseñante: ").SemiBold();
                    text.Span($"{activity.Tutor}");
                });
            });
        });
    }

    private static void ComposeContent(IContainer container, ActivityScheduleDto activity, DateTime[] dates)
    {
        container.PaddingVertical(1, Unit.Centimetre).Table(table =>
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
                header.Cell().Element(CellStyle).Text("Alumno").SemiBold();
                for (int i = 0; i < 4; i++)
                {
                    header.Cell().Element(CellStyle).AlignCenter().Text(dates[i].ToString("dd/MM")).SemiBold();
                }

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            List<string> sortedStudents = activity.EnrolledStudentNames.OrderBy(name => name).ToList();

            if (sortedStudents.Count == 0)
            {
                table.Cell().ColumnSpan(5).Padding(5).Text("No hay alumnos matriculados en esta clase.").Italic();
            }
            else
            {
                foreach (string student in sortedStudents)
                {
                    table.Cell().Element(CellStyle).Text(student);
                    for (int i = 0; i < 4; i++)
                    {
                        table.Cell().Element(CellStyle);
                    }
                }
            }

            static IContainer CellStyle(IContainer container)
            {
                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);
            }
        });
    }
}
