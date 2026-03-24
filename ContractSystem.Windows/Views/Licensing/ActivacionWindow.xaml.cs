using System.Windows;
using ContractSystem.Application.Licensing;

namespace ContractSystem.Windows.Views.Licensing;

public partial class ActivacionWindow : Window
{
    private readonly ILicenciaService _licenciaService;
    private string _fingerprint = string.Empty;

    /// <summary>
    /// Indica si la licencia fue activada exitosamente.
    /// </summary>
    public bool Activada { get; private set; }

    /// <summary>
    /// Resultado de la validación tras la activación.
    /// </summary>
    public LicenciaValidationResult? Resultado { get; private set; }

    public ActivacionWindow(ILicenciaService licenciaService, string? mensaje = null, string? fingerprint = null)
    {
        _licenciaService = licenciaService;
        InitializeComponent();

        // Contacto del proveedor (datos de la pestaña AcercaDe)
        TxtContacto.Text = "Contacto: yoyo871103@gmail.com | (+53) 5 555-1803";

        Loaded += async (_, _) =>
        {
            try
            {
                _fingerprint = fingerprint ?? await _licenciaService.GetFingerprintAsync();
                TxtFingerprint.Text = _fingerprint;
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error al obtener código de instalación: {ex.Message}", true);
            }

            if (mensaje is not null)
                MostrarMensaje(mensaje, true);
        };
    }

    private void BtnCopiar_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(TxtFingerprint.Text))
        {
            Clipboard.SetText(TxtFingerprint.Text);
            MostrarMensaje("Código copiado al portapapeles.", false);
        }
    }

    private async void BtnActivar_Click(object sender, RoutedEventArgs e)
    {
        var clave = TxtClave.Text?.Trim();
        if (string.IsNullOrWhiteSpace(clave))
        {
            MostrarMensaje("Ingrese la clave de licencia.", true);
            return;
        }

        BtnActivar.IsEnabled = false;
        try
        {
            var resultado = await _licenciaService.ActivarLicenciaAsync(clave);
            if (resultado.EsValida)
            {
                Activada = true;
                Resultado = resultado;
                MessageBox.Show(
                    $"Licencia activada exitosamente.\nVálida hasta: {resultado.FechaExpiracion:dd/MM/yyyy}",
                    "Activación exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MostrarMensaje(resultado.Mensaje ?? "Clave de licencia inválida.", true);
            }
        }
        catch (Exception ex)
        {
            MostrarMensaje($"Error al activar: {ex.Message}", true);
        }
        finally
        {
            BtnActivar.IsEnabled = true;
        }
    }

    private void BtnSalir_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void MostrarMensaje(string mensaje, bool esError)
    {
        PnlMensaje.Visibility = Visibility.Visible;
        PnlMensaje.Background = esError
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(253, 237, 237))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(237, 247, 237));
        TxtMensaje.Foreground = esError
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(196, 43, 28))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 125, 50));
        TxtMensaje.Text = mensaje;
    }
}
