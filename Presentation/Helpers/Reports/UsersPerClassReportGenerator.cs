using System.Collections.Generic;
using System.Linq;
using CONEX_APP.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class UsersPerClassReportGenerator : PdfReportBase
{
    protected override string ReportTitle => "Listado de Usuarios por Clase";
    protected override string FilePrefix => "Listado_Usuarios_Por_Clase";
    protected override PageSize PageSize => PageSizes.A4;

    public string GenerateReport(List<ActivityScheduleDto> classes) =>
        BuildAndSave(container => ComposeContent(container, classes));

    private void ComposeContent(IContainer container, List<ActivityScheduleDto> classes)
    {
        container.Column(column =>
        {
            foreach (ActivityScheduleDto activity in classes)
            {
                column.Item().PaddingBottom(20).Column(classBlock => BuildClassBlock(classBlock, activity));
            }
        });
    }

    private void BuildClassBlock(ColumnDescriptor classBlock, ActivityScheduleDto activity)
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
            classBlock.Item().Table(table => BuildStudentTable(table, sortedStudents));
        }
    }

    private void BuildStudentTable(TableDescriptor table, List<string> students)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.ConstantColumn(30);
            columns.RelativeColumn();
        });

        table.Header(header =>
        {
            header.Cell().Element(CompactHeaderCell).Text("Nº");
            header.Cell().Element(CompactHeaderCell).Text("Alumno");
        });

        int index = 1;
        foreach (string student in students)
        {
            table.Cell().Element(CompactDataCell).Text(index.ToString());
            table.Cell().Element(CompactDataCell).Text(student);
            index++;
        }
    }
}
