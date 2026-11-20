using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Auth;
using ContractSystem.Application.Contratos.Commands.CrearFactura;
using ContractSystem.Application.Contratos.Commands.ActualizarFactura;
using ContractSystem.Application.Contratos.Commands.EliminarFactura;
using ContractSystem.Application.Contratos.Queries.GetAdjuntosByFactura;
using ContractSystem.Application.Contratos.Queries.GetAdjuntosFacturasByContrato;
using ContractSystem.Application.Contratos.Queries.GetFacturasByContrato;
using ContractSystem.Domain.Contratos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Contratos;

public partial class FacturasWindow : Window
{
    private readonly ISender _sender;
    private readonly int _contratoId;
    private readonly bool _readOnly;
    private readonly ObservableCollection<FacturaRow> _facturas = new();

    public FacturasWindow(ISender sender, int contratoId, string contratoNumero, bool readOnly = false)
    {
        InitializeComponent();
        _sender = sender;
        _contratoId = contratoId;
        _readOnly = readOnly;
        TxtTitulo.Text = $"Facturas — {contratoNumero}";
        DgFacturas.ItemsSource = _facturas;

        if (_readOnly)
        {
            BtnNueva.IsEnabled = false;
            BtnEditar.IsEnabled = false;
            BtnEliminar.IsEnabled = false;
        }

        var auth = App.Services.GetRequiredService<IAuthContext>();
        PanelAdjuntos.Inicializar(sender, soloLectura: !auth.TienePermiso(Permissions.AdjuntosGestionar),
            mostrarAsociacion: false, titulo: "Adjuntos de la factura");
        PanelAdjuntos.TextoVacio = "Esta factura no tiene documentos adjuntos.";
        PanelAdjuntos.Cargador = async () =>
        {
            if (Seleccionada is not { } row) return Array.Empty<AdjuntoListItem>();
            var lista = await _sender.Send(new GetAdjuntosByFacturaQuery(row.Factura.Id));
            return lista.Select(a => new AdjuntoListItem(a, $"Factura {row.Factura.Numero}", TipoAsociacionAdjunto.Factura)).ToList();
        };
        PanelAdjuntos.Destinos = () => Seleccionada is { } row
            ? new[] { new AdjuntoDestino(TipoAsociacionAdjunto.Factura, $"Factura {row.Factura.Numero}", _contratoId, FacturaId: row.Factura.Id) }
            : Array.Empty<AdjuntoDestino>();
        PanelAdjuntos.AdjuntosCambiados += async (_, _) => await RefrescarAdjuntosFilasAsync();
        PanelAdjuntos.SetDisponible(false, "Seleccione una factura para ver sus adjuntos.");

        Loaded += async (_, _) => await CargarAsync();
    }

    private FacturaRow? Seleccionada => DgFacturas.SelectedItem as FacturaRow;

    private async Task CargarAsync()
    {
        try
        {
            var seleccionadaId = Seleccionada?.Factura.Id;

            var facturasTask = _sender.Send(new GetFacturasByContratoQuery(_contratoId));
            var adjuntosTask = _sender.Send(new GetAdjuntosFacturasByContratoQuery(_contratoId));
            await Task.WhenAll(facturasTask, adjuntosTask);

            var porFactura = adjuntosTask.Result
                .Where(a => a.FacturaId.HasValue)
                .GroupBy(a => a.FacturaId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(a => a.NombreArchivo).ToList());

            _facturas.Clear();
            foreach (var f in facturasTask.Result)
                _facturas.Add(new FacturaRow(f, porFactura.TryGetValue(f.Id, out var n) ? n : new List<string>()));

            ActualizarTotal();

            if (seleccionadaId is int id)
                DgFacturas.SelectedItem = _facturas.FirstOrDefault(r => r.Factura.Id == id);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar facturas: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Actualiza solo la columna "Adjuntos" de las filas (tras adjuntar/eliminar desde el panel).
    /// </summary>
    private async Task RefrescarAdjuntosFilasAsync()
    {
        try
        {
            var adjuntos = await _sender.Send(new GetAdjuntosFacturasByContratoQuery(_contratoId));
            var porFactura = adjuntos
                .Where(a => a.FacturaId.HasValue)
                .GroupBy(a => a.FacturaId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(a => a.NombreArchivo).ToList());
            foreach (var row in _facturas)
                row.SetAdjuntos(porFactura.TryGetValue(row.Factura.Id, out var n) ? n : new List<string>());
            DgFacturas.Items.Refresh();
        }
        catch
        {
            // No crítico: la columna se actualizará al recargar.
        }
    }

    private void ActualizarTotal()
    {
        var total = _facturas.Sum(f => f.Factura.ImporteTotal);
        TxtTotalFacturado.Text = total.ToString("N2", CultureInfo.CurrentCulture);
    }

    private async void DgFacturas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Seleccionada is { } row)
        {
            PanelAdjuntos.Titulo = $"Adjuntos de la factura {row.Factura.Numero}";
            PanelAdjuntos.SetDisponible(true);
            await PanelAdjuntos.RecargarAsync();
        }
        else
        {
            PanelAdjuntos.Titulo = "Adjuntos de la factura";
            PanelAdjuntos.SetDisponible(false, "Seleccione una factura para ver sus adjuntos.");
        }
    }

    private async void BtnNueva_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        var dialog = new FacturaDialogWindow();
        dialog.Owner = this;

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _sender.Send(new CrearFacturaCommand(
                    _contratoId,
                    dialog.NumeroFactura,
                    dialog.FechaFactura,
                    dialog.ImporteTotal,
                    dialog.DescripcionFactura));

                await CargarAsync();
                DgFacturas.SelectedItem = _facturas.FirstOrDefault(r => r.Factura.Numero == dialog.NumeroFactura);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear factura: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void BtnEditar_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (Seleccionada is not { } row) return;
        var factura = row.Factura;

        var dialog = new FacturaDialogWindow();
        dialog.Owner = this;
        dialog.CargarFactura(factura);

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _sender.Send(new ActualizarFacturaCommand(
                    factura.Id,
                    dialog.NumeroFactura,
                    dialog.FechaFactura,
                    dialog.ImporteTotal,
                    dialog.DescripcionFactura));

                await CargarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar factura: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (Seleccionada is not { } row) return;
        var factura = row.Factura;

        var aviso = row.CantidadAdjuntos > 0
            ? $"\n\nTambién se eliminarán sus {row.CantidadAdjuntos} adjunto(s)."
            : string.Empty;
        var result = MessageBox.Show(
            $"¿Eliminar la factura '{factura.Numero}'?{aviso}",
            "Confirmar eliminación",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new EliminarFacturaCommand(factura.Id));
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DgFacturas_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (Seleccionada is null) return;
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is not FacturaRow) return;

        BtnEditar_Click(sender, e);
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement fe &&
            (fe.DataContext is FacturaRow or AdjuntoListItem
             || IsChildOf(fe, DgFacturas) || IsChildOf(fe, PanelAdjuntos)))
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
/// Fila del listado de facturas con el resumen de sus adjuntos.
/// </summary>
public sealed class FacturaRow
{
    public Factura Factura { get; }
    private List<string> _adjuntos;

    public FacturaRow(Factura factura, List<string> adjuntos)
    {
        Factura = factura;
        _adjuntos = adjuntos;
    }

    public void SetAdjuntos(List<string> adjuntos) => _adjuntos = adjuntos;

    public int CantidadAdjuntos => _adjuntos.Count;
    public bool TieneAdjuntos => _adjuntos.Count > 0;

    public string AdjuntosTexto => _adjuntos.Count switch
    {
        0 => "—",
        1 => _adjuntos[0],
        _ => $"({_adjuntos.Count}) {string.Join(", ", _adjuntos)}"
    };

    public string? AdjuntosTooltip => _adjuntos.Count == 0
        ? null
        : string.Join(Environment.NewLine, _adjuntos);
}
