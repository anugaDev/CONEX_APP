using System.Collections.Generic;
using CONEX_APP.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class TotalClassesReportGenerator : PdfReportBase
{
    public string GenerateReport(List<ActivityScheduleDto> classes)
    {
        string filePath = GetFilePath("Listado_Clases");

        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(c => ComposeHeader(c, "Listado Total de Clases"));
                page.Content().Element(c => ComposeClassesTable(c, classes));
                ComposeFooter(page);
            });
        });

        document.GeneratePdf(filePath);
        return filePath;
    }

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
                header.Cell().Element(HeaderStyle).Text("ID");
                header.Cell().Element(HeaderStyle).Text("Nombre de Actividad");
                header.Cell().Element(HeaderStyle).Text("Enseñante");
                header.Cell().Element(HeaderStyle).Text("Aula");
                header.Cell().Element(HeaderStyle).Text("Horario");
                header.Cell().Element(HeaderStyle).Text("Alumnos");

                static IContainer HeaderStyle(IContainer c) =>
                    c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var activity in classes)
            {
                table.Cell().Element(CellStyle).Text(activity.Id.ToString());
                table.Cell().Element(CellStyle).Text(activity.Name);
                table.Cell().Element(CellStyle).Text(activity.Tutor);
                table.Cell().Element(CellStyle).Text(activity.Classroom);
                table.Cell().Element(CellStyle).Text(activity.Date.ToString("dddd HH:mm"));
                table.Cell().Element(CellStyle).Text(activity.Occupancy);
            }

            static IContainer CellStyle(IContainer c) =>
                c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);
        });
    }
}
