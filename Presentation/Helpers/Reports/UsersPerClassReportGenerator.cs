using System.Collections.Generic;
using System.Linq;
using CONEX_APP.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class UsersPerClassReportGenerator : PdfReportBase
{
    public string GenerateReport(List<ActivityScheduleDto> classes)
    {
        string filePath = GetFilePath("Listado_Usuarios_Por_Clase");

        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(c => ComposeHeader(c, "Listado de Usuarios por Clase"));
                page.Content().Element(c => ComposeUsersPerClassContent(c, classes));
                ComposeFooter(page);
            });
        });

        document.GeneratePdf(filePath);
        return filePath;
    }

    private void ComposeUsersPerClassContent(IContainer container, List<ActivityScheduleDto> classes)
    {
        container.Column(column =>
        {
            foreach (ActivityScheduleDto activity in classes)
            {
                column.Item().PaddingBottom(20).Column(classBlock =>
                {
                    classBlock.Item().PaddingBottom(5).Text(text => 
                    {
                        text.Span($"{activity.Name} - {activity.Tutor}").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
                        text.Span($"\nAula: {activity.Classroom} | Horario: {activity.Date:dddd HH:mm} | Alumnos: {activity.EnrolledStudentsCount}").FontSize(10).FontColor(Colors.Grey.Darken3);
                    });

                    if (activity.EnrolledStudentNames.Count == 0)
                    {
                        classBlock.Item().PaddingBottom(10).Text("Sin alumnos matriculados.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        List<string> sortedStudents = activity.EnrolledStudentNames.OrderBy(s => s).ToList();
                        
                        classBlock.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Nº");
                                header.Cell().Element(HeaderStyle).Text("Alumno");

                                static IContainer HeaderStyle(IContainer c) =>
                                    c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(2).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            int index = 1;
                            foreach (string student in sortedStudents)
                            {
                                table.Cell().Element(CellStyle).Text(index.ToString());
                                table.Cell().Element(CellStyle).Text(student);
                                index++;
                            }

                            static IContainer CellStyle(IContainer c) =>
                                c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).PaddingHorizontal(2);
                        });
                    }
                });
            }
        });
    }
}
