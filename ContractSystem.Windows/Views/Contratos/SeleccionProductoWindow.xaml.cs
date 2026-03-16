using System.Windows;
using System.Windows.Input;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Windows.Views.Contratos;

public partial class SeleccionProductoWindow : Window
{
    public ProductoServicio? ProductoSeleccionado { get; private set; }

    public SeleccionProductoWindow(IReadOnlyList<ProductoServicio> productos)
    {
        InitializeComponent();
        DgProductos.ItemsSource = productos;
    }

    private void BtnSeleccionar_Click(object sender, RoutedEventArgs e)
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

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void DgProductos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DgProductos.SelectedItem is ProductoServicio p)
        {
            ProductoSeleccionado = p;
            DialogResult = true;
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else
            DragMove();
    }
}
