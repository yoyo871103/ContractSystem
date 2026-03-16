using System.IO;
using System.Windows;
using System.Windows.Input;
using ContractSystem.Domain.Nomencladores;
using Microsoft.Win32;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class PlantillaDialogWindow : Window
{
    private byte[]? _archivoBytes;
    private string? _nombreArchivo;

    public PlantillaDialogWindow()
    {
        InitializeComponent();
    }

    public string NombrePlantilla => (TxtNombre.Text ?? "").Trim();

    public TipoDocumentoPlantilla TipoDocumento =>
        (CmbTipo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() switch
        {
            "1" => TipoDocumentoPlantilla.Especifico,
            "2" => TipoDocumentoPlantilla.Suplemento,
            _ => TipoDocumentoPlantilla.Marco
        };

    public RolPlantilla RolPlantilla =>
        (CmbRol.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() == "1"
            ? RolPlantilla.Cliente
            : RolPlantilla.Proveedor;

    public byte[] ArchivoBytes => _archivoBytes ?? Array.Empty<byte>();
    public string NombreArchivo => _nombreArchivo ?? string.Empty;
    public bool RevisadoPorLegal => ChkRevisadoLegal.IsChecked == true;

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { }
        }
    }

    private void BtnSeleccionarArchivo_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Filter = "Documentos Word (*.doc;*.docx)|*.doc;*.docx|PDF (*.pdf)|*.pdf|Todos los archivos (*.*)|*.*",
            Title = "Seleccionar plantilla de documento"
        };

        if (ofd.ShowDialog() == true)
        {
            _archivoBytes = File.ReadAllBytes(ofd.FileName);
            _nombreArchivo = Path.GetFileName(ofd.FileName);
            TxtArchivoSeleccionado.Text = _nombreArchivo;
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MessageBox.Show("El nombre es obligatorio.", "Plantilla", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNombre.Focus();
            return;
        }

        if (_archivoBytes is null || _archivoBytes.Length == 0)
        {
            MessageBox.Show("Debe seleccionar un archivo.", "Plantilla", MessageBoxButton.OK, MessageBoxImage.Warning);
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
