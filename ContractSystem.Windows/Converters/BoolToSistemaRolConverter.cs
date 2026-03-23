using System.Globalization;
using System.Windows.Data;

namespace ContractSystem.Windows.Converters;

/// <summary>
/// Convierte bool EsSistema a texto: true -> "Sistema", false -> "Personalizado".
/// </summary>
public sealed class BoolToSistemaRolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? "Sistema" : "Personalizado";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
