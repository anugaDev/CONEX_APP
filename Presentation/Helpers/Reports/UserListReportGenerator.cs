using System.Collections.Generic;
using CONEX_APP.MainApplication.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class UserListReportGenerator : PdfReportBase
{
    protected override string ReportTitle { get; }
    protected override string FilePrefix { get; }

    public UserListReportGenerator(string reportTitle, string filePrefix)
    {
        ReportTitle = reportTitle;
        FilePrefix = filePrefix;
    }

    public string GenerateReport(List<UserDto> users) =>
        BuildAndSave(container => ComposeUserTable(container, users));

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
                header.Cell().Element(HeaderCell).Text("ID");
                header.Cell().Element(HeaderCell).Text("Nombre");
                header.Cell().Element(HeaderCell).Text("Apellidos");
                header.Cell().Element(HeaderCell).Text("DNI");
                header.Cell().Element(HeaderCell).Text("Teléfono");
                header.Cell().Element(HeaderCell).Text("Email");
                header.Cell().Element(HeaderCell).Text("Población");
            });

            foreach (UserDto user in users)
            {
                table.Cell().Element(DataCell).Text(user.Id.ToString());
                table.Cell().Element(DataCell).Text(user.Name);
                table.Cell().Element(DataCell).Text($"{user.Surname} {user.SecondSurname}");
                table.Cell().Element(DataCell).Text(user.IdCard);
                table.Cell().Element(DataCell).Text(user.Phone);
                table.Cell().Element(DataCell).Text(user.Email);
                table.Cell().Element(DataCell).Text(user.Location);
            }
        });
    }
}
