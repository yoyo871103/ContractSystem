using System.IO;
using System.Windows;
using System.Windows.Input;
using ContractSystem.Domain.Contratos;
using Microsoft.Win32;

namespace ContractSystem.Windows.Views.Contratos;

/// <summary>
/// Diálogo para adjuntar uno o varios archivos indicando el documento al que se asocian
/// y el objetivo del adjunto.
/// </summary>
public partial class AdjuntarArchivoDialog : Window
{
    private readonly List<string> _archivos = new();

    public AdjuntarArchivoDialog(IReadOnlyList<AdjuntoDestino> destinos, AdjuntoDestino? destinoPorDefecto = null,
        IEnumerable<string>? archivosIniciales = null)
    {
        InitializeComponent();

        if (destinos.Count == 1)
        {
            CmbDestino.Visibility = Visibility.Collapsed;
            TxtDestinoFijo.Visibility = Visibility.Visible;
            TxtDestinoFijo.Text = destinos[0].Descripcion;
        }
        CmbDestino.ItemsSource = destinos;
        CmbDestino.SelectedItem = destinoPorDefecto is not null && destinos.Contains(destinoPorDefecto)
            ? destinoPorDefecto
            : destinos.FirstOrDefault();

        TxtTiposPermitidos.Text = "Tipos permitidos: " + string.Join(", ", DocumentoAdjunto.ExtensionesPermitidas);

        if (archivosIniciales is not null)
            EstablecerArchivos(archivosIniciales);

        Loaded += (_, _) =>
        {
            if (_archivos.Count == 0) SeleccionarArchivos();
            TxtObjetivo.Focus();
        };
    }

    public IReadOnlyList<string> Archivos => _archivos;
    public AdjuntoDestino? Destino => CmbDestino.SelectedItem as AdjuntoDestino;
    public string Objetivo => (TxtObjetivo.Text ?? string.Empty).Trim();

    private void SeleccionarArchivos()
    {
        var filtro = string.Join(";", DocumentoAdjunto.ExtensionesPermitidas.Select(ext => "*" + ext));
        var dlg = new OpenFileDialog
        {
            Title = "Seleccionar archivo(s) para adjuntar",
            Filter = $"Archivos permitidos ({filtro})|{filtro}|Todos los archivos (*.*)|*.*",
            Multiselect = true
        };
        if (dlg.ShowDialog(this) == true)
            EstablecerArchivos(dlg.FileNames);
    }

    private void EstablecerArchivos(IEnumerable<string> archivos)
    {
        _archivos.Clear();
        _archivos.AddRange(archivos.Where(File.Exists));
        TxtArchivos.Text = _archivos.Count switch
        {
            0 => "Ningún archivo seleccionado",
            1 => Path.GetFileName(_archivos[0]),
            _ => $"{_archivos.Count} archivos: " + string.Join(", ", _archivos.Select(Path.GetFileName))
        };
        TxtArchivos.ToolTip = string.Join(Environment.NewLine, _archivos);

        if (string.IsNullOrWhiteSpace(TxtObjetivo.Text) && _archivos.Count == 1)
            TxtObjetivo.Text = Path.GetFileNameWithoutExtension(_archivos[0]);
    }

    private void BtnSeleccionar_Click(object sender, RoutedEventArgs e) => SeleccionarArchivos();

    private void BtnAceptar_Click(object sender, RoutedEventArgs e)
    {
        if (Destino is null)
        {
            MessageBox.Show("Seleccione el documento al que se asocia el adjunto.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (_archivos.Count == 0)
        {
            MessageBox.Show("Seleccione al menos un archivo.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var noPermitidos = _archivos
            .Select(Path.GetExtension)
            .Where(ext => string.IsNullOrEmpty(ext) || !DocumentoAdjunto.ExtensionesPermitidas.Contains(ext!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (noPermitidos.Count > 0)
        {
            MessageBox.Show(
                $"Tipo(s) de archivo no permitido(s): {string.Join(", ", noPermitidos.Select(x => string.IsNullOrEmpty(x) ? "(sin extensión)" : x))}\n\n" +
                $"Tipos permitidos: {string.Join(", ", DocumentoAdjunto.ExtensionesPermitidas)}",
                "Tipo no permitido", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtObjetivo.Text))
        {
            MessageBox.Show("El campo 'Objetivo' es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtObjetivo.Focus();
            return;
        }

        DialogResult = true;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { /* ignorar */ }
        }
    }
}
