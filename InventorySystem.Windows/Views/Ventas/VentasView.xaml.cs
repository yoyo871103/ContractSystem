using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using InventorySystem.Windows.Shared.Dialogs;

namespace InventorySystem.Windows.Views.Ventas
{
    /// <summary>
    /// Vista de ejemplo (solo diseño) para Ventas: selector de almacén,
    /// carrito con varios artículos, descuento por artículo y general.
    /// </summary>
    public partial class VentasView : UserControl
    {
        public VentasView()
        {
            InitializeComponent();

            // Datos de ejemplo para ver el estilo del carrito (no funcional)
            CarritoGrid.ItemsSource = new List<LineaCarritoEjemplo>
            {
                new LineaCarritoEjemplo { Articulo = "Producto ejemplo A", Cantidad = "2", PrecioUnitario = "10,00", DescuentoPorcentaje = "5", Subtotal = "19,00" },
                new LineaCarritoEjemplo { Articulo = "Producto ejemplo B", Cantidad = "1", PrecioUnitario = "25,50", DescuentoPorcentaje = "0", Subtotal = "25,50" }
            };
            TxtTotal.Text = "44,50";
        }

        private void BtnFinalizarVenta_Click(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);
            var result = MessageDialog.Show(
                owner,
                "Confirmar venta",
                "¿Desea finalizar la venta con los artículos del carrito?",
                MessageDialogButtons.YesNo,
                MessageDialogIcon.Information);

            if (result == MessageDialogResult.Yes)
            {
                MessageDialog.Show(
                    owner,
                    "Venta registrada",
                    "La venta se ha registrado correctamente.",
                    MessageDialogButtons.Ok,
                    MessageDialogIcon.Information);
            }
        }

        /// <summary>
        /// Solo para mostrar filas de ejemplo en el DataGrid (vista de prueba).
        /// </summary>
        private sealed class LineaCarritoEjemplo
        {
            public string Articulo { get; set; } = "";
            public string Cantidad { get; set; } = "";
            public string PrecioUnitario { get; set; } = "";
            public string DescuentoPorcentaje { get; set; } = "";
            public string Subtotal { get; set; } = "";
        }
    }
}
