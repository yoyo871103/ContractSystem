using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos.Commands.CreateAnexo;
using ContractSystem.Application.Contratos.Commands.CreateLineaDetalle;
using ContractSystem.Application.Contratos.Commands.DeleteAnexo;
using ContractSystem.Application.Contratos.Commands.DeleteLineaDetalle;
using ContractSystem.Application.Contratos.Commands.UpdateLineaDetalle;
using ContractSystem.Application.Contratos.Queries.GetAnexosByContrato;
using ContractSystem.Application.Contratos.Queries.GetLineasByContrato;
using ContractSystem.Application.Nomencladores.Queries.GetAllProductosServicios;
using ContractSystem.Domain.Contratos;
using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Windows.Views.Contratos;

public partial class AnexosLineasWindow : Window
{
    private readonly ISender _sender;
    private readonly int _contratoId;
    private readonly bool _readOnly;
    private readonly ObservableCollection<Anexo> _anexos = new();
    private readonly ObservableCollection<LineaDetalle> _lineas = new();

    private Anexo? _anexoSeleccionado;

    public AnexosLineasWindow(ISender sender, int contratoId, string contratoNumero, bool readOnly = false)
    {
        InitializeComponent();
        _sender = sender;
        _contratoId = contratoId;
        _readOnly = readOnly;
        TxtTitulo.Text = $"Anexos y Líneas — {contratoNumero}";
        LstAnexos.ItemsSource = _anexos;
        DgLineas.ItemsSource = _lineas;
        if (_readOnly)
            DgLineas.IsReadOnly = true;
        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            var anexos = await _sender.Send(new GetAnexosByContratoQuery(_contratoId));
            _anexos.Clear();
            foreach (var a in anexos) _anexos.Add(a);

            await CargarLineasAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task CargarLineasAsync()
    {
        var todasLineas = await _sender.Send(new GetLineasByContratoQuery(_contratoId));
        _lineas.Clear();

        if (_anexoSeleccionado is not null)
        {
            foreach (var l in todasLineas.Where(l => l.AnexoId == _anexoSeleccionado.Id))
                _lineas.Add(l);
        }

        ActualizarTotal();
    }

    private void ActualizarTotal()
    {
        var total = _lineas.Sum(l => l.ImporteTotal);
        TxtTotal.Text = total.ToString("N2", CultureInfo.CurrentCulture);
    }

    private void LstAnexos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _anexoSeleccionado = LstAnexos.SelectedItem as Anexo;
        var hayAnexo = _anexoSeleccionado is not null;
        BtnDesdeCatalogo.IsEnabled = hayAnexo;
        BtnInline.IsEnabled = hayAnexo;
        TxtAnexoSeleccionado.Text = hayAnexo
            ? $"Líneas — {_anexoSeleccionado!.Nombre}"
            : "Seleccione un anexo";

        if (_sender is not null && IsLoaded)
            _ = CargarLineasAsync();
    }

    // --- Anexos ---

    private async void BtnAgregarAnexo_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        var nombre = Microsoft.VisualBasic.Interaction.InputBox(
            "Nombre del anexo:", "Nuevo Anexo", "Anexo " + (_anexos.Count + 1));
        if (string.IsNullOrWhiteSpace(nombre)) return;

        try
        {
            await _sender.Send(new CreateAnexoCommand(_contratoId, nombre, null, null, _anexos.Count + 1));
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnEliminarAnexo_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (LstAnexos.SelectedItem is not Anexo anexo) return;

        var result = MessageBox.Show($"¿Eliminar el anexo '{anexo.Nombre}' y todas sus líneas?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new DeleteAnexoCommand(anexo.Id));
            _anexoSeleccionado = null;
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Líneas de detalle ---

    private async void BtnAgregarDesdeCatalogo_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (_anexoSeleccionado is null) return;

        try
        {
            var productos = await _sender.Send(new GetAllProductosServiciosQuery());
            if (productos.Count == 0)
            {
                MessageBox.Show("No hay productos/servicios en el catálogo.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SeleccionProductoWindow(productos);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true && dialog.ProductoSeleccionado is not null)
            {
                var p = dialog.ProductoSeleccionado;
                var cantidad = 1m;
                var precio = p.PrecioEstimado ?? 0;

                await _sender.Send(new CreateLineaDetalleCommand(
                    _contratoId,
                    _anexoSeleccionado.Id,
                    p.Tipo,
                    p.Nombre,
                    p.Descripcion,
                    cantidad,
                    p.UnidadMedida?.NombreCorto,
                    p.UnidadMedidaId,
                    precio,
                    p.Id,
                    _lineas.Count + 1));

                await CargarLineasAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnAgregarInline_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (_anexoSeleccionado is null) return;

        var concepto = Microsoft.VisualBasic.Interaction.InputBox(
            "Concepto de la línea:", "Nueva Línea Inline", "");
        if (string.IsNullOrWhiteSpace(concepto)) return;

        try
        {
            await _sender.Send(new CreateLineaDetalleCommand(
                _contratoId,
                _anexoSeleccionado.Id,
                TipoProductoServicio.Servicio,
                concepto,
                null,
                1,
                null,
                null,
                0,
                null,
                _lineas.Count + 1));

            await CargarLineasAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnEliminarLinea_Click(object sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (DgLineas.SelectedItem is not LineaDetalle linea) return;

        var result = MessageBox.Show($"¿Eliminar la línea '{linea.Concepto}'?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new DeleteLineaDetalleCommand(linea.Id));
            await CargarLineasAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Edición de celdas ---

    private async void DgLineas_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (_readOnly) { e.Cancel = true; return; }
        if (e.EditAction == DataGridEditAction.Cancel) return;
        if (e.Row.Item is not LineaDetalle linea) return;

        // Let the binding commit first
        var editedElement = e.EditingElement as TextBox;
        if (editedElement is null) return;

        var newValue = editedElement.Text;
        var columnHeader = e.Column.Header?.ToString();

        // Apply the new value manually so we can compute and save
        try
        {
            switch (columnHeader)
            {
                case "Concepto":
                    linea.Concepto = newValue;
                    break;
                case "Cant.":
                    if (decimal.TryParse(newValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var cant))
                        linea.Cantidad = cant;
                    break;
                case "U.M.":
                    linea.UnidadMedidaTexto = newValue;
                    break;
                case "P. Unit.":
                    if (decimal.TryParse(newValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var precio))
                        linea.PrecioUnitario = precio;
                    break;
                default:
                    return;
            }

            // Recalculate importe
            linea.ImporteTotal = linea.Cantidad * linea.PrecioUnitario;

            await _sender.Send(new UpdateLineaDetalleCommand(
                linea.Id,
                linea.Tipo,
                linea.Concepto,
                linea.Descripcion,
                linea.Cantidad,
                linea.UnidadMedidaTexto,
                linea.UnidadMedidaId,
                linea.PrecioUnitario,
                linea.ImporteTotal,
                linea.Orden));

            // Refresh the grid to show updated ImporteTotal
            DgLineas.Items.Refresh();
            ActualizarTotal();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- Window chrome ---

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
