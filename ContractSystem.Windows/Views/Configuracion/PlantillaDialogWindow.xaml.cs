using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Domain.Nomencladores;
using Microsoft.Win32;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class PlantillaDialogWindow : Window
{
    private byte[]? _archivoBytes;
    private string? _nombreArchivo;
    private readonly bool _esEdicion;

    /// <summary>
    /// Constructor para crear nueva plantilla.
    /// </summary>
    public PlantillaDialogWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor para editar plantilla existente.
    /// </summary>
    public PlantillaDialogWindow(PlantillaDocumento plantilla) : this()
    {
        _esEdicion = true;
        PlantillaId = plantilla.Id;

        // Título del diálogo
        var titulo = this.FindName("TxtTitulo") as TextBlock;
        if (titulo is not null)
            titulo.Text = "Editar plantilla de documento";

        // Cargar datos existentes
        TxtNombre.Text = plantilla.Nombre;
        CmbTipo.SelectedIndex = (int)plantilla.TipoDocumento;
        CmbRol.SelectedIndex = (int)plantilla.Rol;
        ChkRevisadoLegal.IsChecked = plantilla.RevisadoPorLegal;
        _nombreArchivo = plantilla.NombreArchivo;
        TxtArchivoSeleccionado.Text = plantilla.NombreArchivo;
    }

    public int? PlantillaId { get; }

    public string NombrePlantilla => (TxtNombre.Text ?? "").Trim();

    public TipoDocumentoPlantilla TipoDocumento =>
        (CmbTipo.SelectedItem as ComboBoxItem)?.Tag?.ToString() switch
        {
            "1" => TipoDocumentoPlantilla.Especifico,
            "2" => TipoDocumentoPlantilla.Independiente,
            "3" => TipoDocumentoPlantilla.Suplemento,
            _ => TipoDocumentoPlantilla.Marco
        };

    public RolPlantilla RolPlantilla =>
        (CmbRol.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "1"
            ? RolPlantilla.Cliente
            : RolPlantilla.Proveedor;

    public byte[]? ArchivoBytes => _archivoBytes;
    public string NombreArchivo => _nombreArchivo ?? string.Empty;
    public bool RevisadoPorLegal => ChkRevisadoLegal.IsChecked == true;
    public bool EsEdicion => _esEdicion;

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

        if (!_esEdicion && (_archivoBytes is null || _archivoBytes.Length == 0))
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
