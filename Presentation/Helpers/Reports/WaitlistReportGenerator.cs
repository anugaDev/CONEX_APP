using CONEX_APP.MainApplication.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class WaitlistReportGenerator : PdfReportBase
{
    protected override string ReportTitle => "Listas de Espera";
    protected override string FilePrefix => "Listas_Espera";
    protected override PageSize PageSize => PageSizes.A4;

    public string GenerateReport(List<ActivityWaitlistDto> waitlists) =>
        BuildAndSave(container => ComposeContent(container, waitlists));

    private void ComposeContent(IContainer container, List<ActivityWaitlistDto> waitlists)
    {
        container.Column(column =>
        {
            if (waitlists.Count == 0)
            {
                column.Item().Text("No hay nadie en ninguna lista de espera.").Italic().FontColor(Colors.Grey.Darken1);
                return;
            }

            foreach (ActivityWaitlistDto waitlist in waitlists)
                column.Item().PaddingBottom(20).Column(block => BuildActivityBlock(block, waitlist));
        });
    }

    private void BuildActivityBlock(ColumnDescriptor block, ActivityWaitlistDto waitlist)
    {
        ActivityScheduleDto activity = waitlist.Activity;

        block.Item().PaddingBottom(5).Text(text =>
        {
            text.Span($"{activity.Name} - {activity.Tutor}").FontSize(14).SemiBold().FontColor(Colors.Blue.Darken2);
            text.Span($"\nAula: {activity.Classroom} | Horario: {activity.Schedule} | Plazas: {activity.Occupancy} | En espera: {waitlist.Entries.Count}")
                .FontSize(10).FontColor(Colors.Grey.Darken3);
        });

        block.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);
                columns.RelativeColumn();
                columns.ConstantColumn(110);
            });

            table.Header(header =>
            {
                header.Cell().Element(CompactHeaderCell).Text("Nº");
                header.Cell().Element(CompactHeaderCell).Text("Persona");
                header.Cell().Element(CompactHeaderCell).Text("Apuntada el");
            });

            foreach (WaitlistEntryDto entry in waitlist.Entries)
            {
                table.Cell().Element(CompactDataCell).Text(entry.Position.ToString());
                table.Cell().Element(CompactDataCell).Text(entry.FullName);
                table.Cell().Element(CompactDataCell).Text(entry.JoinedAt.ToString("dd/MM/yyyy HH:mm"));
            }
        });
    }
}
