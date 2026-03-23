using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos.Commands.CrearFactura;
using ContractSystem.Application.Contratos.Commands.ActualizarFactura;
using ContractSystem.Application.Contratos.Commands.EliminarFactura;
using ContractSystem.Application.Contratos.Queries.GetFacturasByContrato;
using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Windows.Views.Contratos;

public partial class FacturasWindow : Window
{
    private readonly ISender _sender;
    private readonly int _contratoId;
    private readonly bool _readOnly;
    private readonly ObservableCollection<Factura> _facturas = new();

    public FacturasWindow(ISender sender, int contratoId, string contratoNumero, bool readOnly = false)
    {
        InitializeComponent();
        _sender = sender;
        _contratoId = contratoId;
        _readOnly = readOnly;
        TxtTitulo.Text = $"Facturas — {contratoNumero}";
        DgFacturas.ItemsSource = _facturas;
        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            var lista = await _sender.Send(new GetFacturasByContratoQuery(_contratoId));
            _facturas.Clear();
            foreach (var f in lista)
                _facturas.Add(f);

            ActualizarTotal();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar facturas: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ActualizarTotal()
    {
        var total = _facturas.Sum(f => f.ImporteTotal);
        TxtTotalFacturado.Text = total.ToString("N2", CultureInfo.CurrentCulture);
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
        if (DgFacturas.SelectedItem is not Factura factura) return;

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
        if (DgFacturas.SelectedItem is not Factura factura) return;

        var result = MessageBox.Show(
            $"¿Eliminar la factura '{factura.Numero}'?",
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
        if (DgFacturas.SelectedItem is not Factura) return;
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is not Factura) return;

        BtnEditar_Click(sender, e);
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement fe &&
            (fe.DataContext is Factura || IsChildOf(fe, DgFacturas)))
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
