using System.IO;
using CONEX_APP.MainApplication.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CONEX_APP.Presentation.Helpers.Reports;

public class UserBadgeGenerator
{
    public UserBadgeGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public string GenerateBadge(UserDto user)
    {
        string fileName = $"Carnet_Usuario_{user.Id}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.pdf";
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

        byte[] logoBytes = LoadLogoBytes();

        Document document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(85.6f, 53.98f, Unit.Millimetre);
                page.Margin(4, Unit.Millimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial));

                page.Content().Column(column =>
                {
                    column.Item().AlignCenter().PaddingBottom(2)
                        .MaxWidth(25, Unit.Millimetre)
                        .Image(logoBytes);

                    column.Item().PaddingVertical(1.5f).LineHorizontal(0.5f).LineColor(Colors.Blue.Darken2);

                    column.Item().AlignCenter().PaddingBottom(1)
                        .Text($"{user.Name} {user.Surname} {user.SecondSurname}")
                        .FontSize(8).SemiBold().FontColor(Colors.Black);

                    column.Item().AlignCenter()
                        .Text($"Nº Carnet: {user.Id}")
                        .FontSize(7).FontColor(Colors.Grey.Darken2);
                });
            });
        });

        document.GeneratePdf(filePath);
        return filePath;
    }

    private byte[] LoadLogoBytes()
    {
        Uri resourceUri = new Uri("pack://application:,,,/Presentation/Resources/Images/conex_logo.png", UriKind.Absolute);
        System.Windows.Resources.StreamResourceInfo resourceInfo = System.Windows.Application.GetResourceStream(resourceUri);

        if (resourceInfo == null)
            throw new FileNotFoundException("No se encontró el logo de CONEX.");

        using MemoryStream ms = new MemoryStream();
        resourceInfo.Stream.CopyTo(ms);
        return ms.ToArray();
    }
}
