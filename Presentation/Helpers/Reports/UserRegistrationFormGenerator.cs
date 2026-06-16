using CONEX_APP.MainApplication.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class UserRegistrationFormGenerator : UserDocumentBase
{
    private const string FilePrefix = "Hoja_Inscripcion";
    private const string DocumentTitle = "HOJA DE INSCRIPCIÓN";
    private const string SectionPersonal = "DATOS PERSONALES";
    private const string SectionActivities = "ACTIVIDADES INSCRITAS";
    private const string SectionSignature = "DECLARACIÓN Y FIRMA";
    private const string SignatureDisclaimer =
        "El/la abajo firmante declara que los datos facilitados son verídicos y acepta " +
        "las condiciones de participación de la Asociación CONEX.";
    private const string NoActivitiesText = "Sin actividades inscritas.";
    private const string ActivityItemFormat = "• ID de actividad: {0}";

    private const int LogoWidthPt = 55;
    private const int DateColumnWidthPt = 110;
    private const int CheckboxWidthPt = 130;
    private const int CheckboxIconWidth = 16;
    private const float ActivitiesMinHeightPt = 50f;

    public string GenerateRegistrationForm(UserDto user)
    {
        string filePath = BuildFilePath(FilePrefix, user.Id);
        byte[] logoBytes = LoadLogoBytes();

        Document.Create(container => container.Page(page =>
        {
            ConfigurePage(page);
            page.Header().PaddingBottom(10).Column(col => RenderHeader(col, logoBytes));
            page.Content().Column(col => RenderContent(col, user));
            page.Footer().AlignCenter()
                .Text(BuildFooterText(user))
                .FontSize(FontSizeXs).FontColor(ColorTextSubtle);
        })).GeneratePdf(filePath);

        return filePath;
    }

    private void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
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
        row.RelativeItem().PaddingLeft(15).Column(RenderTitleBlock);
        row.ConstantItem(DateColumnWidthPt).AlignRight().AlignBottom()
            .Text($"Fecha: {DateTime.Now:dd/MM/yyyy}")
            .FontSize(FontSizeMd).FontColor(ColorTextSubtle);
    }

    private void RenderTitleBlock(ColumnDescriptor col)
    {
        col.Item()
            .Text(DocumentTitle)
            .FontSize(FontSizeXxl).SemiBold().FontColor(ColorPrimary);

        col.Item().PaddingTop(2)
            .Text(AssociationName)
            .FontSize(FontSizeLg).FontColor(ColorTextMuted);
    }

    private void RenderContent(ColumnDescriptor col, UserDto user)
    {
        RenderPersonalDataSection(col, user);
        RenderActivitiesSection(col, user);
        RenderSignatureSection(col);
    }

    private void RenderPersonalDataSection(ColumnDescriptor col, UserDto user)
    {
        col.Item().PaddingTop(SectionGapTopPt - 4).PaddingBottom(4)
            .Element(c => RenderSectionTitle(c, SectionPersonal));

        col.Item()
            .Border(ThinLineWeight).BorderColor(ColorBorder).Padding(10)
            .Column(inner => RenderPersonalDataFields(inner, user));
    }

    private void RenderPersonalDataFields(ColumnDescriptor col, UserDto user)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem(3).Column(c => RenderLabeledField(c, "Nombre", user.Name));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(3).Column(c => RenderLabeledField(c, "Primer apellido", user.Surname));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(3).Column(c => RenderLabeledField(c, "Segundo apellido", user.SecondSurname));
        });

        col.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem(2).Column(c => RenderLabeledField(c, "DNI / NIE", user.IdCard));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(2).Column(c => RenderLabeledField(c, "Teléfono", user.Phone));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(3).Column(c => RenderLabeledField(c, "Correo electrónico", user.Email));
        });

        col.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem(3).Column(c => RenderLabeledField(c, "Dirección", user.Address));
            row.ConstantItem(FieldGutterPt);
            row.RelativeItem(2).Column(c => RenderLabeledField(c, "Localidad", user.Location));
        });

        col.Item().PaddingTop(12).Row(row =>
        {
            row.ConstantItem(CheckboxWidthPt).Row(r => RenderCheckbox(r, "Socio", user.IsPartner));
            row.ConstantItem(CheckboxWidthPt).Row(r => RenderCheckbox(r, "Tutor", user.IsTutor));
        });
    }

    private void RenderCheckbox(RowDescriptor row, string label, bool isChecked)
    {
        row.ConstantItem(CheckboxIconWidth).AlignMiddle()
            .Text(isChecked ? "[✓]" : "[ ]")
            .FontSize(FontSizeBase).Bold();

        row.RelativeItem().PaddingLeft(4).AlignMiddle()
            .Text(label)
            .FontSize(FontSizeBase);
    }

    private void RenderActivitiesSection(ColumnDescriptor col, UserDto user)
    {
        col.Item().PaddingTop(SectionGapTopPt).PaddingBottom(4)
            .Element(c => RenderSectionTitle(c, SectionActivities));

        col.Item()
            .Border(ThinLineWeight).BorderColor(ColorBorder)
            .MinHeight(ActivitiesMinHeightPt).Padding(10)
            .Column(inner => RenderActivitiesList(inner, user.EnrolledActivityIds));
    }

    private void RenderActivitiesList(ColumnDescriptor col, List<int> activityIds)
    {
        if (activityIds.Count == 0)
        {
            col.Item().Text(NoActivitiesText).FontColor(Colors.Grey.Medium).Italic();
            return;
        }

        foreach (int id in activityIds)
            col.Item().PaddingBottom(3).Text(string.Format(ActivityItemFormat, id)).FontSize(FontSizeBase);
    }

    private void RenderSignatureSection(ColumnDescriptor col)
    {
        col.Item().PaddingTop(20).Column(inner =>
        {
            inner.Item().Element(c => RenderSectionTitle(c, SectionSignature));

            inner.Item().PaddingTop(6)
                .Text(SignatureDisclaimer)
                .FontSize(FontSizeMd).FontColor(ColorTextMuted);

            RenderSignatureRow(inner, "Firma del interesado/a", "Firma y sello de la asociación");
        });
    }

    private string BuildFooterText(UserDto user)
        => $"Nº de socio: {user.Id}  |  Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
}
