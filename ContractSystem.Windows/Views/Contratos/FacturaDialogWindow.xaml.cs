using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Domain.Contratos;

namespace ContractSystem.Windows.Views.Contratos;

public partial class FacturaDialogWindow : Window
{
    public FacturaDialogWindow()
    {
        InitializeComponent();
    }

    // --- Propiedades públicas ---

    public string NumeroFactura => (TxtNumero.Text ?? "").Trim();

    public DateTime FechaFactura => DpFecha.SelectedDate ?? DateTime.Today;

    public decimal ImporteTotal
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TxtImporteTotal.Text)) return 0;
            return decimal.TryParse(TxtImporteTotal.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var val) ? val : 0;
        }
    }

    public string DescripcionFactura => (TxtDescripcion.Text ?? "").Trim();

    // --- Cargar para edición ---

    public void CargarFactura(Factura factura)
    {
        TxtTitulo.Text = "Editar factura";
        TxtNumero.Text = factura.Numero;
        DpFecha.SelectedDate = factura.Fecha;
        TxtImporteTotal.Text = factura.ImporteTotal.ToString(CultureInfo.CurrentCulture);
        TxtDescripcion.Text = factura.Descripcion;
    }

    // --- Event handlers ---

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNumero.Text))
        {
            MessageBox.Show("El número de factura es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNumero.Focus();
            return;
        }

        if (DpFecha.SelectedDate is null)
        {
            MessageBox.Show("La fecha es obligatoria.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            DpFecha.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtImporteTotal.Text) || ImporteTotal <= 0)
        {
            MessageBox.Show("El importe total debe ser mayor que cero.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtImporteTotal.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtDescripcion.Text))
        {
            MessageBox.Show("La descripción es obligatoria.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtDescripcion.Focus();
            return;
        }

        DialogResult = true;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else
            DragMove();
    }

    private void OnlyDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var textBox = sender as TextBox;
        var newText = textBox?.Text.Insert(textBox.CaretIndex, e.Text) ?? e.Text;
        e.Handled = !decimal.TryParse(newText, NumberStyles.Any, CultureInfo.CurrentCulture, out _)
                    && e.Text != separator;
    }
}
