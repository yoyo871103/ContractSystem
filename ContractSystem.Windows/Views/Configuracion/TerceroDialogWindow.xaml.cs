using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class TerceroDialogWindow : Window
{
    private readonly ObservableCollection<ContactoEditItem> _contactos = new();

    public TerceroDialogWindow()
    {
        InitializeComponent();
        DgContactos.ItemsSource = _contactos;
    }

    public string NombreTercero => (TxtNombre.Text ?? "").Trim();
    public string RazonSocial => (TxtRazonSocial.Text ?? "").Trim();
    public string NifCif => (TxtNifCif.Text ?? "").Trim();
    public string Direccion => (TxtDireccion.Text ?? "").Trim();
    public string TelefonoTercero => (TxtTelefono.Text ?? "").Trim();
    public string EmailTercero => (TxtEmail.Text ?? "").Trim();

    public TipoTercero TipoTercero
    {
        get
        {
            var item = CmbTipo.SelectedItem as System.Windows.Controls.ComboBoxItem;
            return item?.Tag switch
            {
                "1" => TipoTercero.Proveedor,
                "2" => TipoTercero.Ambos,
                _ => TipoTercero.Cliente
            };
        }
    }

    public IReadOnlyList<ContactoEditItem> Contactos =>
        _contactos.Where(c => !string.IsNullOrWhiteSpace(c.Nombre)).ToList();

    /// <summary>
    /// Carga los datos de un tercero existente para edición.
    /// </summary>
    public void CargarTercero(Tercero tercero)
    {
        TxtTitulo.Text = "Editar tercero";
        TxtNombre.Text = tercero.Nombre;
        TxtRazonSocial.Text = tercero.RazonSocial;
        TxtNifCif.Text = tercero.NifCif;
        TxtDireccion.Text = tercero.Direccion;
        TxtTelefono.Text = tercero.Telefono;
        TxtEmail.Text = tercero.Email;

        CmbTipo.SelectedIndex = tercero.Tipo switch
        {
            TipoTercero.Proveedor => 1,
            TipoTercero.Ambos => 2,
            _ => 0
        };

        _contactos.Clear();
        foreach (var c in tercero.Contactos)
        {
            _contactos.Add(new ContactoEditItem
            {
                Nombre = c.Nombre,
                Cargo = c.Cargo,
                Email = c.Email,
                Telefono = c.Telefono
            });
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { }
        }
    }

    private void BtnAgregarContacto_Click(object sender, RoutedEventArgs e)
    {
        _contactos.Add(new ContactoEditItem());
    }

    private void BtnEliminarContacto_Click(object sender, RoutedEventArgs e)
    {
        if (DgContactos.SelectedItem is ContactoEditItem item)
            _contactos.Remove(item);
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MessageBox.Show("El nombre es obligatorio.", "Tercero", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNombre.Focus();
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

/// <summary>
/// Modelo editable para contactos en el DataGrid.
/// </summary>
public class ContactoEditItem
{
    public string Nombre { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
