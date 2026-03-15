using System.Globalization;
using System.Windows.Data;

namespace ContractSystem.Windows.Converters;

/// <summary>
/// Convierte bool IsDeleted a texto para columna Estado: true -> "Eliminado", false -> "Activo".
/// </summary>
public sealed class BoolToEstadoUsuarioConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? "Eliminado" : "Activo";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
