using CONEX_APP.MainApplication.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class UserBadgeGenerator : UserDocumentBase
{
    private const string FilePrefix = "Carnet_Usuario";
    private const float BadgeWidthMm = 85.6f;
    private const float BadgeHeightMm = 53.98f;
    private const float BadgeMarginMm = 4f;
    private const float LogoMaxWidthMm = 25f;
    private const float DividerPaddingV = 1.5f;
    private const int NameFontSize = 8;
    private const int NumberFontSize = 7;

    public string GenerateBadge(UserDto user)
    {
        string filePath = BuildFilePath(FilePrefix, user.Id);
        byte[] logoBytes = LoadLogoBytes();

        Document.Create(container => container.Page(page =>
        {
            ConfigurePage(page);
            page.Content().Column(col => RenderContent(col, user, logoBytes));
        })).GeneratePdf(filePath);

        return filePath;
    }

    private void ConfigurePage(PageDescriptor page)
    {
        page.Size(BadgeWidthMm, BadgeHeightMm, Unit.Millimetre);
        page.Margin(BadgeMarginMm, Unit.Millimetre);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial));
    }

    private void RenderContent(ColumnDescriptor col, UserDto user, byte[] logoBytes)
    {
        RenderLogo(col, logoBytes);
        RenderDivider(col);
        RenderUserName(col, user);
        RenderCardNumber(col, user);
    }

    private void RenderLogo(ColumnDescriptor col, byte[] logoBytes)
    {
        col.Item()
            .AlignCenter().PaddingBottom(2)
            .MaxWidth(LogoMaxWidthMm, Unit.Millimetre)
            .Image(logoBytes);
    }

    private void RenderDivider(ColumnDescriptor col)
    {
        col.Item()
            .PaddingVertical(DividerPaddingV)
            .LineHorizontal(ThinLineWeight).LineColor(ColorPrimaryLight);
    }

    private void RenderUserName(ColumnDescriptor col, UserDto user)
    {
        col.Item().AlignCenter().PaddingBottom(1)
            .Text($"{user.Name} {user.Surname} {user.SecondSurname}")
            .FontSize(NameFontSize).SemiBold().FontColor(Colors.Black);
    }

    private void RenderCardNumber(ColumnDescriptor col, UserDto user)
    {
        col.Item().AlignCenter()
            .Text($"Nº Carnet: {user.Id}")
            .FontSize(NumberFontSize).FontColor(ColorTextMuted);
    }
}
