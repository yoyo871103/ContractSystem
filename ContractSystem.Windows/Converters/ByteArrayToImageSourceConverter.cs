using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ContractSystem.Windows.Converters;

/// <summary>
/// Convierte byte[] (imagen) a ImageSource para mostrar en Image. Devuelve null si el valor no es byte[] válido.
/// </summary>
public sealed class ByteArrayToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not byte[] bytes || bytes.Length == 0)
            return null;

        try
        {
            using var stream = new MemoryStream(bytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
