using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public abstract class UserDocumentBase
{
    protected const string AssociationName = "Asociación CONEX";
    protected const string LogoResourceUri =
        "pack://application:,,,/Presentation/Resources/Images/conex_logo.png";

    protected const int FontSizeXs = 7;
    protected const int FontSizeSm = 8;
    protected const int FontSizeMd = 9;
    protected const int FontSizeBase = 10;
    protected const int FontSizeLg = 11;
    protected const int FontSizeXl = 16;
    protected const int FontSizeXxl = 20;

    protected const float PageMarginCm = 1.5f;
    protected const float HeaderDividerWeight = 1.5f;
    protected const float ThinLineWeight = 0.5f;
    protected const int FieldGutterPt = 10;
    protected const int SectionGapTopPt = 14;
    protected const int SignatureGapTopPt = 25;
    protected const int SignatureSpacingPt = 40;

    protected static readonly string ColorPrimary = Colors.Blue.Darken3;
    protected static readonly string ColorPrimaryLight = Colors.Blue.Darken2;
    protected static readonly string ColorSuccess = Colors.Green.Darken2;
    protected static readonly string ColorTextMuted = Colors.Grey.Darken2;
    protected static readonly string ColorTextSubtle = Colors.Grey.Darken1;
    protected static readonly string ColorBorder = Colors.Grey.Lighten1;

    protected const string EmptyFieldPlaceholder = "—";

    protected UserDocumentBase()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    protected string BuildFilePath(string prefix, int userId)
    {
        string fileName = $"{prefix}_{userId}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.pdf";
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
    }

    protected byte[] LoadLogoBytes()
    {
        Uri uri = new Uri(LogoResourceUri, UriKind.Absolute);
        System.Windows.Resources.StreamResourceInfo resource =
            System.Windows.Application.GetResourceStream(uri);

        if (resource == null)
            throw new FileNotFoundException("No se encontró el logo de CONEX.");

        using MemoryStream ms = new MemoryStream();
        resource.Stream.CopyTo(ms);
        return ms.ToArray();
    }

    protected void RenderSectionTitle(IContainer container, string title)
    {
        container.Text(title).FontSize(FontSizeLg).SemiBold().FontColor(ColorPrimaryLight);
    }

    protected void RenderLabeledField(ColumnDescriptor col, string label, string value)
    {
        string displayValue = string.IsNullOrWhiteSpace(value) ? " " : value;

        col.Item()
            .Text(label)
            .FontSize(FontSizeSm)
            .FontColor(ColorTextMuted);

        col.Item()
            .PaddingTop(2).PaddingBottom(3)
            .BorderBottom(ThinLineWeight).BorderColor(ColorBorder)
            .Text(displayValue)
            .FontSize(FontSizeBase);
    }

    protected void RenderHeaderDivider(IContainer container)
    {
        container.PaddingTop(8).LineHorizontal(HeaderDividerWeight).LineColor(ColorPrimary);
    }

    protected void RenderSignatureRow(
        ColumnDescriptor col,
        string leftLabel,
        string rightLabel,
        int spacingBetween = SignatureSpacingPt)
    {
        col.Item().PaddingTop(SignatureGapTopPt).Row(row =>
        {
            row.RelativeItem().Column(c => RenderSignatureBlock(c, leftLabel));
            row.ConstantItem(spacingBetween);
            row.RelativeItem().Column(c => RenderSignatureBlock(c, rightLabel));
        });
    }

    private void RenderSignatureBlock(ColumnDescriptor col, string label)
    {
        col.Item().LineHorizontal(ThinLineWeight).LineColor(Colors.Black);
        col.Item().PaddingTop(4).AlignCenter()
            .Text(label)
            .FontSize(FontSizeMd).FontColor(ColorTextSubtle);
    }
}
