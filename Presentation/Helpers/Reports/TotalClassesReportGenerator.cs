using System.Collections.Generic;
using CONEX_APP.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class TotalClassesReportGenerator : PdfReportBase
{
    protected override string ReportTitle => "Listado Total de Clases";
    protected override string FilePrefix => "Listado_Clases";

    public string GenerateReport(List<ActivityScheduleDto> classes) =>
        BuildAndSave(container => ComposeClassesTable(container, classes));

    private void ComposeClassesTable(IContainer container, List<ActivityScheduleDto> classes)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("ID");
                header.Cell().Element(HeaderCell).Text("Nombre de Actividad");
                header.Cell().Element(HeaderCell).Text("Enseñante");
                header.Cell().Element(HeaderCell).Text("Aula");
                header.Cell().Element(HeaderCell).Text("Horario");
                header.Cell().Element(HeaderCell).Text("Alumnos");
            });

            foreach (ActivityScheduleDto activity in classes)
            {
                table.Cell().Element(DataCell).Text(activity.Id.ToString());
                table.Cell().Element(DataCell).Text(activity.Name);
                table.Cell().Element(DataCell).Text(activity.Tutor);
                table.Cell().Element(DataCell).Text(activity.Classroom);
                table.Cell().Element(DataCell).Text(activity.Date.ToString("dddd HH:mm"));
                table.Cell().Element(DataCell).Text(activity.Occupancy);
            }
        });
    }
}
