using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public abstract class PdfReportBase
{
    protected abstract string ReportTitle { get; }
    protected abstract string FilePrefix { get; }
    protected virtual PageSize PageSize => PageSizes.A4.Landscape();
    protected virtual float MarginCm => 1f;
    protected virtual int BaseFontSize => 10;

    protected PdfReportBase()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    protected string BuildAndSave(Action<IContainer> composeContent)
    {
        string filePath = BuildFilePath();
        Document document = Document.Create(container => container.Page(page => ConfigurePage(page, composeContent)));
        document.GeneratePdf(filePath);
        return filePath;
    }

    private void ConfigurePage(PageDescriptor page, Action<IContainer> composeContent)
    {
        page.Size(PageSize);
        page.Margin(MarginCm, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(BaseFontSize).FontFamily(Fonts.Arial));
        page.Header().Element(ComposeHeader);
        page.Content().Element(composeContent);
        ComposeFooter(page);
    }

    private void ComposeHeader(IContainer container)
    {
        container.PaddingBottom(20).Row(row =>
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(ReportTitle).FontSize(24).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).Text($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken1);
            }));
    }

    private void ComposeFooter(PageDescriptor page)
    {
        page.Footer().AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }

    protected string BuildFilePath()
    {
        string fileName = $"{FilePrefix}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.pdf";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
    }

    protected IContainer HeaderCell(IContainer container) =>
        container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);

    protected IContainer DataCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).PaddingHorizontal(2);

    protected IContainer CompactHeaderCell(IContainer container) =>
        container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(2).BorderBottom(1).BorderColor(Colors.Black);

    protected IContainer CompactDataCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(2).PaddingHorizontal(2);
}
