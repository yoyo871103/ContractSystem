using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ContractSystem.Application.Contratos.Queries.DescargarDocumentoAdjunto;
using ContractSystem.Windows.Shared.Controls;
using MediatR;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace ContractSystem.Windows.Views.Contratos;

/// <summary>
/// Visor universal de archivos (adjuntos, plantillas...) sin necesidad de descargarlos:
/// - PDF e imágenes: render embebido con WebView2 (Edge Chromium).
/// - Texto (.txt): se muestra directamente.
/// - Word/Excel: vista previa embebida usando el "preview handler" de Windows/Office
///   (el mismo del panel de vista previa del Explorador). Si no está disponible,
///   se abre una copia temporal con la aplicación del sistema.
/// El archivo temporal se elimina al cerrar el visor.
/// </summary>
public partial class VisorAdjuntoWindow : Window
{
    private readonly Func<CancellationToken, Task<byte[]>> _cargarContenido;
    private readonly string _nombreArchivo;
    private readonly string _extension;

    private string? _tempFilePath;
    private byte[]? _contenido;

    private static readonly HashSet<string> ExtensionesWebView = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".bmp"
    };

    private static readonly HashSet<string> ExtensionesTexto = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".csv", ".log"
    };

    /// <summary>
    /// Visor de un documento adjunto (se descarga de la base de datos por Id).
    /// </summary>
    public VisorAdjuntoWindow(ISender sender, int adjuntoId, string nombreArchivo, string extension)
        : this(nombreArchivo, extension,
            async ct => (await sender.Send(new DescargarDocumentoAdjuntoQuery(adjuntoId), ct)).Contenido)
    {
    }

    /// <summary>
    /// Visor genérico: <paramref name="cargarContenido"/> devuelve los bytes del archivo.
    /// </summary>
    public VisorAdjuntoWindow(string nombreArchivo, string? extension, Func<CancellationToken, Task<byte[]>> cargarContenido)
    {
        InitializeComponent();
        _cargarContenido = cargarContenido;
        _nombreArchivo = string.IsNullOrWhiteSpace(nombreArchivo) ? "archivo" : nombreArchivo;
        _extension = string.IsNullOrEmpty(extension) ? Path.GetExtension(_nombreArchivo) : extension;

        TxtTitulo.Text = _nombreArchivo;
        Loaded += async (_, _) => await CargarAsync();
        Closed += (_, _) => LimpiarTemporal();
    }

    private async Task CargarAsync()
    {
        try
        {
            _contenido = await _cargarContenido(CancellationToken.None);
            if (_contenido is null || _contenido.Length == 0)
            {
                MostrarPanelExterno("El archivo está vacío o no se encontró.", abrirAutomaticamente: false);
                return;
            }

            // Archivo temporal con el nombre real para que los visores reciban el tipo correcto.
            var tempDir = Path.Combine(Path.GetTempPath(), "ContractSystem", "Visor", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            _tempFilePath = Path.Combine(tempDir, NombreSeguro(_nombreArchivo));
            await File.WriteAllBytesAsync(_tempFilePath, _contenido);

            if (ExtensionesTexto.Contains(_extension))
            {
                MostrarTexto(_contenido);
            }
            else if (ExtensionesWebView.Contains(_extension))
            {
                await CargarEnWebView2(_tempFilePath);
            }
            else if (!await IntentarVistaPreviaAsync(_tempFilePath))
            {
                MostrarPanelExterno(
                    $"No hay un visor integrado disponible para archivos {_extension} en este equipo " +
                    "(la vista previa de Word/Excel requiere tener Microsoft Office instalado).\n\n" +
                    "Se abrirá una copia temporal con la aplicación predeterminada del sistema.",
                    abrirAutomaticamente: true);
            }
        }
        catch (Exception ex)
        {
            TxtCargando.Visibility = Visibility.Collapsed;
            MessageBox.Show("Error al cargar el archivo: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MostrarTexto(byte[] contenido)
    {
        TxtContenido.Text = DecodificarTexto(contenido);
        TxtCargando.Visibility = Visibility.Collapsed;
        TxtContenido.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Respeta el BOM si existe; si no, intenta UTF-8 estricto y cae a Latin-1 (archivos ANSI antiguos).
    /// </summary>
    private static string DecodificarTexto(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        try
        {
            using var reader = new StreamReader(ms, new UTF8Encoding(false, throwOnInvalidBytes: true), detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Latin1.GetString(bytes);
        }
    }

    private async Task<bool> IntentarVistaPreviaAsync(string filePath)
    {
        var clsid = PreviewHandlerHost.BuscarHandler(_extension);
        if (clsid is null) return false;

        try
        {
            TxtCargando.Visibility = Visibility.Collapsed;
            PreviewHost.Visibility = Visibility.Visible;

            // Esperar a que el HwndHost tenga ventana y tamaño reales.
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
            UpdateLayout();

            PreviewHost.Abrir(filePath, clsid.Value);
            return true;
        }
        catch
        {
            PreviewHost.Descargar();
            PreviewHost.Visibility = Visibility.Collapsed;
            return false;
        }
    }

    private async Task CargarEnWebView2(string filePath)
    {
        try
        {
            // User data folder propio en %TEMP% (no requiere permisos especiales).
            var udf = Path.Combine(Path.GetTempPath(), "ContractSystem", "WebView2");
            Directory.CreateDirectory(udf);
            var env = await CoreWebView2Environment.CreateAsync(userDataFolder: udf);
            await WebView.EnsureCoreWebView2Async(env);

            WebView.Source = new Uri(filePath);

            TxtCargando.Visibility = Visibility.Collapsed;
            WebView.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            // Si WebView2 Runtime no está disponible, caemos al modo externo.
            MostrarPanelExterno(
                "No se pudo iniciar el visor embebido (WebView2 no disponible).\n\n" +
                "Detalle: " + ex.Message + "\n\n" +
                "Puede abrir el archivo con la aplicación predeterminada del sistema.",
                abrirAutomaticamente: false);
        }
    }

    private void MostrarPanelExterno(string mensaje, bool abrirAutomaticamente)
    {
        TxtCargando.Visibility = Visibility.Collapsed;
        TxtMensajeExterno.Text = mensaje;
        PanelExterno.Visibility = Visibility.Visible;
        BtnAbrirExternoPie.Visibility = Visibility.Collapsed;

        if (abrirAutomaticamente)
            AbrirConShell();
    }

    private void AbrirConShell()
    {
        if (string.IsNullOrEmpty(_tempFilePath) || !File.Exists(_tempFilePath)) return;
        try
        {
            Process.Start(new ProcessStartInfo(_tempFilePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir el archivo con la aplicación predeterminada: " + ex.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnAbrirExterno_Click(object sender, RoutedEventArgs e) => AbrirConShell();

    private async void BtnGuardarComo_Click(object sender, RoutedEventArgs e)
    {
        if (_contenido is null) return;
        var saveDialog = new SaveFileDialog
        {
            Title = "Guardar archivo",
            FileName = _nombreArchivo,
            Filter = $"Archivo (*{_extension})|*{_extension}|Todos los archivos (*.*)|*.*"
        };
        if (saveDialog.ShowDialog() != true) return;

        try
        {
            await File.WriteAllBytesAsync(saveDialog.FileName, _contenido);
            MessageBox.Show("Archivo guardado correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private static string NombreSeguro(string nombre)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) nombre = nombre.Replace(c, '_');
        return nombre;
    }

    private void LimpiarTemporal()
    {
        // Liberar visores antes de borrar el archivo (pueden tener handle abierto).
        try { PreviewHost.Descargar(); } catch { /* ignorar */ }
        try { WebView.Dispose(); } catch { /* ignorar */ }

        if (string.IsNullOrEmpty(_tempFilePath)) return;
        var dir = Path.GetDirectoryName(_tempFilePath);
        try
        {
            if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
        catch
        {
            // El archivo puede seguir en uso si el SO lo abrió; no es crítico.
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // No interferir con clicks dentro de los visores.
        if (e.OriginalSource is FrameworkElement fe &&
            (IsChildOf(fe, WebView) || IsChildOf(fe, TxtContenido))) return;

        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private static bool IsChildOf(DependencyObject child, DependencyObject parent)
    {
        var current = child;
        while (current != null)
        {
            if (current == parent) return true;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return false;
    }
}
