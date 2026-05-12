using System.Collections.Generic;
using CONEX_APP.MainApplication.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class PartnersReportGenerator : PdfReportBase
{
    public string GenerateReport(List<UserDto> partners)
    {
        string filePath = GetFilePath("Listado_Socios");

        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(c => ComposeHeader(c, "Listado de Socios"));
                page.Content().Element(c => ComposeUserTable(c, partners));
                ComposeFooter(page);
            });
        });

        document.GeneratePdf(filePath);
        return filePath;
    }

    private void ComposeUserTable(IContainer container, List<UserDto> users)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).Text("ID");
                header.Cell().Element(HeaderStyle).Text("Nombre");
                header.Cell().Element(HeaderStyle).Text("Apellidos");
                header.Cell().Element(HeaderStyle).Text("DNI");
                header.Cell().Element(HeaderStyle).Text("Teléfono");
                header.Cell().Element(HeaderStyle).Text("Email");
                header.Cell().Element(HeaderStyle).Text("Población");

                static IContainer HeaderStyle(IContainer c) =>
                    c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
            });

            foreach (var user in users)
            {
                table.Cell().Element(CellStyle).Text(user.Id.ToString());
                table.Cell().Element(CellStyle).Text(user.Name);
                table.Cell().Element(CellStyle).Text($"{user.Surname} {user.SecondSurname}");
                table.Cell().Element(CellStyle).Text(user.IdCard);
                table.Cell().Element(CellStyle).Text(user.Phone);
                table.Cell().Element(CellStyle).Text(user.Email);
                table.Cell().Element(CellStyle).Text(user.Location);
            }

            static IContainer CellStyle(IContainer c) =>
                c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);
        });
    }
}
