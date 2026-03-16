using System.Globalization;
using System.Windows;
using System.Windows.Input;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class ProductoServicioDialogWindow : Window
{
    public ProductoServicioDialogWindow()
    {
        InitializeComponent();
    }

    public string? Codigo => string.IsNullOrWhiteSpace(TxtCodigo.Text) ? null : TxtCodigo.Text.Trim();
    public string NombreProducto => (TxtNombre.Text ?? "").Trim();
    public string Descripcion => (TxtDescripcion.Text ?? "").Trim();

    public TipoProductoServicio TipoProducto =>
        (CmbTipo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() == "1"
            ? TipoProductoServicio.Servicio
            : TipoProductoServicio.Producto;

    public int? UnidadMedidaId =>
        (CmbUnidadMedida.SelectedItem as UnidadMedida)?.Id;

    public decimal? PrecioEstimado
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TxtPrecioEstimado.Text)) return null;
            return decimal.TryParse(TxtPrecioEstimado.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var val)
                ? val : null;
        }
    }

    public void CargarUnidadesMedida(IReadOnlyList<UnidadMedida> unidades)
    {
        CmbUnidadMedida.ItemsSource = unidades;
    }

    public void CargarProducto(ProductoServicio producto)
    {
        TxtTitulo.Text = "Editar producto/servicio";
        TxtCodigo.Text = producto.Codigo ?? "";
        TxtNombre.Text = producto.Nombre;
        TxtDescripcion.Text = producto.Descripcion;
        CmbTipo.SelectedIndex = producto.Tipo == TipoProductoServicio.Servicio ? 1 : 0;
        TxtPrecioEstimado.Text = producto.PrecioEstimado?.ToString(CultureInfo.CurrentCulture) ?? "";

        if (producto.UnidadMedidaId.HasValue && CmbUnidadMedida.ItemsSource is IEnumerable<UnidadMedida> unidades)
        {
            CmbUnidadMedida.SelectedItem = unidades.FirstOrDefault(u => u.Id == producto.UnidadMedidaId.Value);
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { }
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MessageBox.Show("El nombre es obligatorio.", "Producto/Servicio", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNombre.Focus();
            return;
        }

        if (!string.IsNullOrWhiteSpace(TxtPrecioEstimado.Text) &&
            !decimal.TryParse(TxtPrecioEstimado.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out _))
        {
            MessageBox.Show("El precio estimado no es un número válido.", "Producto/Servicio", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtPrecioEstimado.Focus();
            return;
        }

        DialogResult = true;
        Close();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
