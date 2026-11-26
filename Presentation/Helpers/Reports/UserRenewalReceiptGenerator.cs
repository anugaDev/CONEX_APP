using System.Globalization;
using CONEX_APP.MainApplication.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class UserRenewalReceiptGenerator : UserDocumentBase
{
    private const string FilePrefix = "Recibo_Renovacion";

    private const string DocumentTitle = "RECIBO DE RENOVACIÓN";

    private const string SectionMemberData = "DATOS DEL SOCIO";

    private const string SectionConcept = "CONCEPTO";

    private const string TableHeaderDesc = "Descripción";

    private const string TableHeaderAmount = "Importe";

    private const string TotalLabel = "TOTAL";

    private const string MemberTypeSocioTutor = "Socio / Tutor";

    private const string MemberTypeSocio = "Socio";

    private const string MemberTypeTutor = "Tutor";

    private const int LogoWidthPt = 50;

    private const int ReceiptSignatureGap = 30;

    private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");

    public string GenerateReceipt(UserDto user, RenewalReceiptDto receipt)
    {
        string filePath = BuildFilePath(FilePrefix, user.Id);
        byte[] logoBytes = LoadLogoBytes();
        DateTime generatedAt = DateTime.Now;

        Document.Create(container => container.Page(page =>
        {
            ConfigurePage(page);
            page.Header().PaddingBottom(8).Column(col => RenderHeader(col, logoBytes));
            page.Content().Column(col => RenderContent(col, user, receipt));
            page.Footer().AlignCenter()
                .Text(BuildFooterText(generatedAt))
                .FontSize(FontSizeXs).FontColor(ColorTextSubtle);
        })).GeneratePdf(filePath);

        return filePath;
    }

    private void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A5);
        page.Margin(PageMarginCm, Unit.Centimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(FontSizeBase));
    }

    private void RenderHeader(ColumnDescriptor col, byte[] logoBytes)
    {
        col.Item().Row(row => RenderHeaderRow(row, logoBytes));
        col.Item().Element(RenderHeaderDivider);
    }

    private void RenderHeaderRow(RowDescriptor row, byte[] logoBytes)
    {
        row.ConstantItem(LogoWidthPt).Image(logoBytes);
        row.RelativeItem().PaddingLeft(12).Column(RenderTitleBlock);
    }

    private void RenderTitleBlock(ColumnDescriptor col)
    {
        col.Item()
            .Text(DocumentTitle)
            .FontSize(FontSizeXl).SemiBold().FontColor(ColorPrimary);

        col.Item().PaddingTop(2)
            .Text(AssociationName)
            .FontSize(FontSizeBase).FontColor(ColorTextMuted);
    }

    private void RenderContent(ColumnDescriptor col, UserDto user, RenewalReceiptDto receipt)
    {
        RenderReceiptMetaRow(col, receipt);
        col.Item().PaddingTop(SectionGapTopPt).LineHorizontal(ThinLineWeight).LineColor(ColorBorder);
        RenderMemberDataSection(col, user);
        RenderConceptSection(col, receipt);
        RenderSignatureRow(col, "Firma del interesado/a", "Sello y firma de la asociación", ReceiptSignatureGap);
    }

    private void RenderReceiptMetaRow(ColumnDescriptor col, RenewalReceiptDto receipt)
    {
        col.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem().Column(c =>
                RenderLabeledField(c, "Nº de recibo", receipt.ReceiptNumber));

            row.ConstantItem(FieldGutterPt);

            row.RelativeItem().Column(c =>
                RenderColoredDateField(c, "Fecha de renovación", receipt.RenewalDate.ToString("dd/MM/yyyy"), ColorPrimaryLight));

            row.ConstantItem(FieldGutterPt);

            row.RelativeItem().Column(c =>
                RenderColoredDateField(c, "Válido hasta", receipt.ValidUntil.ToString("dd/MM/yyyy"), ColorSuccess));
        });
    }

    private void RenderColoredDateField(ColumnDescriptor col, string label, string dateText, string color)
    {
        col.Item().Text(label).FontSize(FontSizeSm).FontColor(ColorTextMuted);
        col.Item().PaddingTop(2)
            .Text(dateText)
            .FontSize(FontSizeBase).SemiBold().FontColor(color);
    }

    private void RenderMemberDataSection(ColumnDescriptor col, UserDto user)
    {
        col.Item().PaddingTop(12).PaddingBottom(4)
            .Element(c => RenderSectionTitle(c, SectionMemberData));

        col.Item()
            .Border(ThinLineWeight).BorderColor(ColorBorder).Padding(10)
            .Column(inner => RenderMemberFields(inner, user));
    }

    private void RenderMemberFields(ColumnDescriptor col, UserDto user)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem(2).Column(c => RenderLabeledField(c, "Nombre completo", user.FullName));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(1).Column(c => RenderLabeledField(c, "Nº de socio", user.Id.ToString()));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(1).Column(c => RenderLabeledField(c, "DNI / NIE", user.IdCard));
        });

        col.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem(1).Column(c => RenderLabeledField(c, "Teléfono", user.Phone));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(2).Column(c => RenderLabeledField(c, "Correo electrónico", user.Email));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(1).Column(c => RenderLabeledField(c, "Tipo", BuildMemberType(user)));
        });
    }

    private void RenderConceptSection(ColumnDescriptor col, RenewalReceiptDto receipt)
    {
        col.Item().PaddingTop(SectionGapTopPt).PaddingBottom(4)
            .Element(c => RenderSectionTitle(c, SectionConcept));

        col.Item()
            .Border(ThinLineWeight).BorderColor(ColorBorder)
            .Table(table => RenderConceptTable(table, receipt));
    }

    private void RenderConceptTable(TableDescriptor table, RenewalReceiptDto receipt)
    {
        table.ColumnsDefinition(cols =>
        {
            cols.RelativeColumn(4);
            cols.RelativeColumn(1);
        });

        table.Header(header =>
        {
            header.Cell().Background(ColorPrimary).Padding(6)
                .Text(TableHeaderDesc).FontSize(FontSizeMd).SemiBold().FontColor(Colors.White);
            header.Cell().Background(ColorPrimary).Padding(6).AlignRight()
                .Text(TableHeaderAmount).FontSize(FontSizeMd).SemiBold().FontColor(Colors.White);
        });

        // Cuota de renovación
        table.Cell().Element(ConceptCell).Column(c =>
        {
            c.Item().Text(BuildRenewalConcept(receipt)).FontSize(FontSizeBase);
            c.Item().PaddingTop(2)
                .Text($"Periodo: {receipt.RenewalDate:dd/MM/yyyy} – {receipt.ValidUntil:dd/MM/yyyy}")
                .FontSize(FontSizeSm).FontColor(ColorTextMuted);
        });
        table.Cell().Element(ConceptCell).AlignRight()
            .Text(FormatAmount(receipt.RenewalAmount)).FontSize(FontSizeBase);

        // Una línea por clase
        foreach (ReceiptClassLineDto cls in receipt.Classes)
        {
            table.Cell().Element(ConceptCell).Column(c =>
            {
                c.Item().Text($"Clase: {cls.ActivityName}").FontSize(FontSizeBase);
                string details = string.Join(" · ", new[] { cls.Schedule, cls.Tutor }.Where(s => !string.IsNullOrWhiteSpace(s)));
                if (details.Length > 0)
                    c.Item().PaddingTop(2).Text(details).FontSize(FontSizeSm).FontColor(ColorTextMuted);
            });
            table.Cell().Element(ConceptCell).AlignRight()
                .Text(FormatAmount(receipt.HasAmounts ? cls.Price : null)).FontSize(FontSizeBase);
        }

        // Total
        table.Cell().Background(Colors.Grey.Lighten4).Padding(8)
            .Text(TotalLabel).FontSize(FontSizeLg).SemiBold();
        table.Cell().Background(Colors.Grey.Lighten4).Padding(8).AlignRight()
            .Text(FormatAmount(receipt.Total)).FontSize(FontSizeLg).SemiBold().FontColor(ColorPrimary);
    }

    private IContainer ConceptCell(IContainer container) =>
        container.BorderBottom(ThinLineWeight).BorderColor(ColorBorder).Padding(8);

    private static string BuildRenewalConcept(RenewalReceiptDto receipt) => receipt.PeriodMonths switch
    {
        12 => "Cuota anual de renovación de socio",
        1 => "Cuota mensual de renovación de socio",
        > 0 => $"Cuota de renovación de socio ({receipt.PeriodMonths} meses)",
        _ => "Cuota de renovación de socio"
    };

    private static string FormatAmount(decimal? amount) =>
        amount.HasValue ? amount.Value.ToString("C2", SpanishCulture) : EmptyFieldPlaceholder;

    private string BuildFooterText(DateTime generatedAt)
        => $"Documento generado el {generatedAt:dd/MM/yyyy HH:mm}  |  {AssociationName}";

    private string BuildMemberType(UserDto user)
        => (user.IsPartner, user.IsTutor) switch
        {
            (true, true)  => MemberTypeSocioTutor,
            (true, false) => MemberTypeSocio,
            (false, true) => MemberTypeTutor,
            _             => EmptyFieldPlaceholder
        };
}
