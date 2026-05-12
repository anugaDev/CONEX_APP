using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public abstract class PdfReportBase
{
    protected PdfReportBase()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    protected string GetFilePath(string prefix)
    {
        string fileName = $"{prefix}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.pdf";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
    }

    protected void ComposeHeader(IContainer container, string title)
    {
        container.PaddingBottom(20).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(title).FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    protected void ComposeFooter(PageDescriptor page)
    {
        page.Footer().AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }
}
