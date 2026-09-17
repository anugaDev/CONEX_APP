using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CONEX_APP.Domain.Enums;

namespace CONEX_APP.Presentation.Converters;

/// <summary>
/// Convierte un RenewalStatus en el color de fondo correspondiente para las filas de la tabla de usuarios.
/// </summary>
public class RenewalStatusToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RenewalStatus status)
        {
            return status switch
            {
                RenewalStatus.Expired     => new SolidColorBrush(Color.FromRgb(0xFF, 0xCD, 0xD2)), // rojo suave
                RenewalStatus.ExpiringSoon => new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0x3B)), // amarillo
                _                          => Brushes.Transparent
            };
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
