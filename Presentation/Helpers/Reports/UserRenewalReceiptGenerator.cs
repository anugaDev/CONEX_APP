using CONEX_APP.Application.DTOs;
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

    private const string SectionClasses = "CLASES INCLUIDAS";

    private const string ReceiptNumberPrefix = "REC";

    private const string ConceptDescription = "Cuota anual de renovación de socio";

    private const string TableHeaderDesc = "Descripción";

    private const string TableHeaderPeriod = "Periodo";

    private const string MemberTypeSocioTutor = "Socio / Tutor";

    private const string MemberTypeSocio = "Socio";

    private const string MemberTypeTutor = "Tutor";

    private const int LogoWidthPt = 50;

    private const int ReceiptSignatureGap = 30;

    /// <param name="user">Datos del usuario.</param>
    /// <param name="selectedActivities">Clases seleccionadas para incluir en el recibo. Puede ser null o vacío.</param>
    public string GenerateReceipt(UserDto user, IEnumerable<ActivityScheduleDto>? selectedActivities = null)
    {
        DateTime renewalDate = DateTime.Now;
        string filePath = BuildFilePath(FilePrefix, user.Id);
        byte[] logoBytes = LoadLogoBytes();
        List<ActivityScheduleDto> classes = selectedActivities?.ToList() ?? [];

        Document.Create(container => container.Page(page =>
        {
            ConfigurePage(page);
            page.Header().PaddingBottom(8).Column(col => RenderHeader(col, logoBytes));
            page.Content().Column(col => RenderContent(col, user, renewalDate, classes));
            page.Footer().AlignCenter()
                .Text(BuildFooterText(renewalDate))
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

    private void RenderContent(ColumnDescriptor col, UserDto user, DateTime renewalDate, List<ActivityScheduleDto> classes)
    {
        RenderReceiptMetaRow(col, user, renewalDate);
        col.Item().PaddingTop(SectionGapTopPt).LineHorizontal(ThinLineWeight).LineColor(ColorBorder);
        RenderMemberDataSection(col, user);
        RenderConceptSection(col, renewalDate);
        if (classes.Count > 0)
            RenderClassesSection(col, classes);
        RenderSignatureRow(col, "Firma del interesado/a", "Sello y firma de la asociación", ReceiptSignatureGap);
    }

    private void RenderReceiptMetaRow(ColumnDescriptor col, UserDto user, DateTime renewalDate)
    {
        col.Item().PaddingTop(10).Row(row =>
        {
            row.RelativeItem().Column(c =>
                RenderLabeledField(c, "Nº de recibo", BuildReceiptNumber(user.Id, renewalDate)));

            row.ConstantItem(FieldGutterPt);

            row.RelativeItem().Column(c =>
                RenderColoredDateField(c, "Fecha de renovación", renewalDate.ToString("dd/MM/yyyy"), ColorPrimaryLight));

            row.ConstantItem(FieldGutterPt);

            row.RelativeItem().Column(c =>
                RenderColoredDateField(c, "Válido hasta", renewalDate.AddYears(1).ToString("dd/MM/yyyy"), ColorSuccess));
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
            row.RelativeItem(2).Column(c => RenderLabeledField(c, "Nombre completo", BuildFullName(user)));
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

    private void RenderConceptSection(ColumnDescriptor col, DateTime renewalDate)
    {
        col.Item().PaddingTop(SectionGapTopPt).PaddingBottom(4)
            .Element(c => RenderSectionTitle(c, SectionConcept));

        col.Item()
            .Border(ThinLineWeight).BorderColor(ColorBorder).Padding(10)
            .Table(table => RenderConceptTable(table, renewalDate));
    }

    private void RenderConceptTable(TableDescriptor table, DateTime renewalDate)
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
                .Text(TableHeaderPeriod).FontSize(FontSizeMd).SemiBold().FontColor(Colors.White);
        });

        table.Cell().BorderBottom(ThinLineWeight).BorderColor(ColorBorder).Padding(8)
            .Text(ConceptDescription).FontSize(FontSizeBase);
        table.Cell().BorderBottom(ThinLineWeight).BorderColor(ColorBorder).Padding(8).AlignRight()
            .Text(BuildRenewalPeriod(renewalDate)).FontSize(FontSizeBase);
    }

    private void RenderClassesSection(ColumnDescriptor col, List<ActivityScheduleDto> classes)
    {
        col.Item().PaddingTop(SectionGapTopPt).PaddingBottom(4)
            .Element(c => RenderSectionTitle(c, SectionClasses));

        col.Item()
            .Border(ThinLineWeight).BorderColor(ColorBorder)
            .Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3); // Clase
                    cols.RelativeColumn(2); // Tutor
                });

                table.Header(header =>
                {
                    header.Cell().Background(ColorPrimary).Padding(6)
                        .Text("Clase").FontSize(FontSizeMd).SemiBold().FontColor(Colors.White);
                    header.Cell().Background(ColorPrimary).Padding(6)
                        .Text("Tutor/a").FontSize(FontSizeMd).SemiBold().FontColor(Colors.White);
                });

                foreach (ActivityScheduleDto cls in classes)
                {
                    table.Cell().BorderBottom(ThinLineWeight).BorderColor(ColorBorder).Padding(7)
                        .Text(cls.Name).FontSize(FontSizeBase);
                    table.Cell().BorderBottom(ThinLineWeight).BorderColor(ColorBorder).Padding(7)
                        .Text(cls.Tutor).FontSize(FontSizeBase).FontColor(ColorTextMuted);
                }
            });
    }

    private string BuildFooterText(DateTime renewalDate)
        => $"Documento generado el {renewalDate:dd/MM/yyyy HH:mm}  |  {AssociationName}";

    private string BuildReceiptNumber(int userId, DateTime date)
        => $"{ReceiptNumberPrefix}-{userId:D4}-{date:yyyyMMddHHmmss}";

    private string BuildFullName(UserDto user)
        => $"{user.Name} {user.Surname} {user.SecondSurname}".Trim();

    private string BuildRenewalPeriod(DateTime date)
        => $"{date:yyyy} – {date.AddYears(1):yyyy}";

    private string BuildMemberType(UserDto user)
        => (user.IsPartner, user.IsTutor) switch
        {
            (true, true)  => MemberTypeSocioTutor,
            (true, false) => MemberTypeSocio,
            (false, true) => MemberTypeTutor,
            _             => EmptyFieldPlaceholder
        };
}
