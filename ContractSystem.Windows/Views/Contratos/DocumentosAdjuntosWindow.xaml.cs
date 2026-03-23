using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos.Commands.AdjuntarDocumento;
using ContractSystem.Application.Contratos.Commands.EliminarDocumentoAdjunto;
using ContractSystem.Application.Contratos.Queries.DescargarDocumentoAdjunto;
using ContractSystem.Application.Contratos.Queries.GetDocumentosAdjuntos;
using ContractSystem.Domain.Contratos;
using MediatR;
using Microsoft.Win32;

namespace ContractSystem.Windows.Views.Contratos;

public partial class DocumentosAdjuntosWindow : Window
{
    private readonly ISender _sender;
    private readonly int _contratoId;
    private readonly bool _readOnly;
    private readonly ObservableCollection<AdjuntoListItem> _adjuntos = new();

    public DocumentosAdjuntosWindow(ISender sender, int contratoId, string contratoNumero, bool readOnly = false)
    {
        _readOnly = readOnly;
        InitializeComponent();
        _sender = sender;
        _contratoId = contratoId;
        TxtTitulo.Text = $"Documentos Adjuntos — {contratoNumero}";
        DgAdjuntos.ItemsSource = _adjuntos;
        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            var lista = await _sender.Send(new GetDocumentosAdjuntosQuery(_contratoId));
            _adjuntos.Clear();
            foreach (var d in lista)
                _adjuntos.Add(new AdjuntoListItem(d));
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar adjuntos: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnAdjuntar_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        var filtro = string.Join(";", DocumentoAdjunto.ExtensionesPermitidas.Select(ext => "*" + ext));
        var openDialog = new OpenFileDialog
        {
            Title = "Seleccionar archivo para adjuntar",
            Filter = $"Archivos permitidos ({filtro})|{filtro}|Todos los archivos (*.*)|*.*",
            Multiselect = false
        };

        if (openDialog.ShowDialog() != true) return;

        var extension = Path.GetExtension(openDialog.FileName);
        if (!DocumentoAdjunto.ExtensionesPermitidas.Contains(extension))
        {
            MessageBox.Show(
                $"El tipo de archivo '{extension}' no está permitido.\n\nTipos permitidos: {string.Join(", ", DocumentoAdjunto.ExtensionesPermitidas)}",
                "Tipo no permitido", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var objetivo = Microsoft.VisualBasic.Interaction.InputBox(
            "Indique el objetivo/propósito de este documento:", "Objetivo del adjunto", "");
        if (string.IsNullOrWhiteSpace(objetivo))
        {
            MessageBox.Show("El campo 'Objetivo' es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var contenido = await File.ReadAllBytesAsync(openDialog.FileName);
            await _sender.Send(new AdjuntarDocumentoCommand(
                _contratoId,
                Path.GetFileName(openDialog.FileName),
                objetivo,
                contenido));

            await CargarAsync();
            MessageBox.Show("Archivo adjuntado correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al adjuntar: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnDescargar_Click(object sender, RoutedEventArgs e)
    {
        if (DgAdjuntos.SelectedItem is not AdjuntoListItem item) return;

        var saveDialog = new SaveFileDialog
        {
            Title = "Guardar archivo adjunto",
            FileName = item.NombreArchivo,
            Filter = $"Archivo (*{item.Extension})|*{item.Extension}|Todos los archivos (*.*)|*.*"
        };

        if (saveDialog.ShowDialog() != true) return;

        try
        {
            var adjunto = await _sender.Send(new DescargarDocumentoAdjuntoQuery(item.Id));
            await File.WriteAllBytesAsync(saveDialog.FileName, adjunto.Contenido);
            MessageBox.Show("Archivo descargado correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al descargar: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (DgAdjuntos.SelectedItem is not AdjuntoListItem item) return;

        var result = MessageBox.Show(
            $"¿Eliminar el adjunto '{item.NombreArchivo}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new EliminarDocumentoAdjuntoCommand(item.Id));
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DgAdjuntos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true; // Prevent bubble to Window (maximize/minimize)

        if (DgAdjuntos.SelectedItem is not AdjuntoListItem item) return;

        // Avoid firing on header clicks
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is not AdjuntoListItem) return;

        try
        {
            var adjunto = await _sender.Send(new DescargarDocumentoAdjuntoQuery(item.Id));
            var tempDir = Path.Combine(Path.GetTempPath(), "ContractSystem");
            Directory.CreateDirectory(tempDir);
            var tempFile = Path.Combine(tempDir, item.NombreArchivo);
            await File.WriteAllBytesAsync(tempFile, adjunto.Contenido);

            Process.Start(new ProcessStartInfo(tempFile) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir archivo: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Only handle when clicking on title bar area, not on DataGrid or other controls
        if (e.OriginalSource is FrameworkElement fe &&
            (fe.DataContext is AdjuntoListItem || IsChildOf(fe, DgAdjuntos)))
            return;

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

/// <summary>
/// Item de presentación para el DataGrid (evita cargar Contenido y formatea tamaño).
/// </summary>
public class AdjuntoListItem
{
    public int Id { get; }
    public string NombreArchivo { get; }
    public string Extension { get; }
    public string Objetivo { get; }
    public string TamanioTexto { get; }
    public DateTime FechaCarga { get; }

    public AdjuntoListItem(DocumentoAdjunto d)
    {
        Id = d.Id;
        NombreArchivo = d.NombreArchivo;
        Extension = d.Extension;
        Objetivo = d.Objetivo;
        FechaCarga = d.FechaCarga;
        TamanioTexto = FormatearTamanio(d.TamanioBytes);
    }

    private static string FormatearTamanio(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:N1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):N1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):N2} GB"
    };
}
