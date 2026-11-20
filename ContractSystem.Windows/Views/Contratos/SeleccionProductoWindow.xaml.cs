using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Auth;
using ContractSystem.Application.Nomencladores.Commands.CreateProductoServicio;
using ContractSystem.Application.Nomencladores.Queries.GetAllProductosServicios;
using ContractSystem.Application.Nomencladores.Queries.GetAllUnidadesMedida;
using ContractSystem.Domain.Nomencladores;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Contratos;

/// <summary>
/// Selección de un producto/servicio del catálogo con búsqueda incremental y
/// posibilidad de darlo de alta en el momento.
/// </summary>
public partial class SeleccionProductoWindow : Window
{
    private readonly ISender _sender;
    private List<ProductoServicio> _productos;

    public ProductoServicio? ProductoSeleccionado { get; private set; }

    public SeleccionProductoWindow(ISender sender, IReadOnlyList<ProductoServicio> productos)
    {
        InitializeComponent();
        _sender = sender;
        _productos = productos.ToList();

        var auth = App.Services.GetRequiredService<IAuthContext>();
        BtnNuevo.Visibility = auth.TienePermiso(Permissions.ProductosCrear) ? Visibility.Visible : Visibility.Collapsed;

        Filtrar();
        Loaded += (_, _) => TxtBuscar.Focus();
    }

    // ---------- Búsqueda ----------

    private void Filtrar()
    {
        var terminos = Normalizar(TxtBuscar.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var lista = _productos
            .Where(p => terminos.Length == 0 || terminos.All(t => TextoBusqueda(p).Contains(t, StringComparison.Ordinal)))
            .OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        DgProductos.ItemsSource = lista;
        if (lista.Count > 0) DgProductos.SelectedIndex = 0;

        TxtBuscarPlaceholder.Visibility = string.IsNullOrEmpty(TxtBuscar.Text) ? Visibility.Visible : Visibility.Collapsed;
        TxtContador.Text = $"{lista.Count} de {_productos.Count} producto(s)/servicio(s)";

        TxtVacio.Visibility = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TxtVacio.Text = _productos.Count == 0
            ? "El catálogo está vacío."
            : "No hay coincidencias.";
        if (BtnNuevo.Visibility == Visibility.Visible)
            TxtVacio.Text += "\nPulse \"Nuevo producto/servicio\" para darlo de alta.";
    }

    private static string TextoBusqueda(ProductoServicio p) =>
        Normalizar($"{p.Nombre} {p.Codigo} {p.Descripcion}");

    private static string Normalizar(string s)
    {
        var descompuesto = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString();
    }

    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) => Filtrar();

    private void TxtBuscar_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var count = DgProductos.Items.Count;
        switch (e.Key)
        {
            case Key.Down when count > 0:
                DgProductos.SelectedIndex = Math.Min(DgProductos.SelectedIndex + 1, count - 1);
                DgProductos.ScrollIntoView(DgProductos.SelectedItem);
                e.Handled = true;
                break;
            case Key.Up when count > 0:
                DgProductos.SelectedIndex = Math.Max(DgProductos.SelectedIndex - 1, 0);
                DgProductos.ScrollIntoView(DgProductos.SelectedItem);
                e.Handled = true;
                break;
            case Key.Enter:
                Confirmar();
                e.Handled = true;
                break;
            case Key.Escape:
                DialogResult = false;
                e.Handled = true;
                break;
        }
    }

    // ---------- Alta rápida ----------

    private async void BtnNuevo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var unidades = await _sender.Send(new GetAllUnidadesMedidaQuery());
            var dialog = new Configuracion.ProductoServicioDialogWindow { Owner = this };
            dialog.CargarUnidadesMedida(unidades);
            dialog.PrecargarNombre(TxtBuscar.Text);

            if (dialog.ShowDialog() != true) return;

            var creado = await _sender.Send(new CreateProductoServicioCommand(
                dialog.Codigo, dialog.NombreProducto, dialog.Descripcion,
                dialog.TipoProducto, dialog.UnidadMedidaId, dialog.PrecioEstimado));

            // Recargar para traer la unidad de medida (navegación) y seleccionarlo.
            var productos = await _sender.Send(new GetAllProductosServiciosQuery());
            _productos = productos.ToList();
            ProductoSeleccionado = _productos.FirstOrDefault(p => p.Id == creado.Id) ?? creado;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al crear el producto/servicio: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ---------- Selección ----------

    private void Confirmar()
    {
        if (DgProductos.SelectedItem is ProductoServicio p)
        {
            ProductoSeleccionado = p;
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Seleccione un producto/servicio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnSeleccionar_Click(object sender, RoutedEventArgs e) => Confirmar();

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void DgProductos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is not ProductoServicio) return;
        Confirmar();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
