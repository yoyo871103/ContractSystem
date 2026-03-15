using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ContractSystem.Windows.Converters;

/// <summary>
/// Convierte bool a Visibility. true -> Visible, false -> Collapsed.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var visible = value is true;
        if (parameter is string p && string.Equals(p, "Invert", StringComparison.OrdinalIgnoreCase))
            visible = !visible;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}
