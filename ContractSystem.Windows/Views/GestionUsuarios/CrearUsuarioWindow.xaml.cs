using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ContractSystem.Application.Auth;
using ContractSystem.Windows.Models;

namespace ContractSystem.Windows.Views.GestionUsuarios;

public partial class CrearUsuarioWindow : Window
{
    private readonly ObservableCollection<RolSeleccionItem> _roles = new();

    public CrearUsuarioWindow()
    {
        InitializeComponent();
        RolesPanel.ItemsSource = _roles;
    }

    /// <summary>
    /// Carga los roles disponibles para asignación.
    /// </summary>
    public void CargarRoles(IReadOnlyList<RolItem> roles)
    {
        _roles.Clear();
        foreach (var r in roles)
            _roles.Add(new RolSeleccionItem { Id = r.Id, Nombre = r.Nombre, IsSelected = false });
    }

    public string NombreUsuario => (TxtNombreUsuario.Text ?? "").Trim();
    public string NombreParaMostrar => (TxtNombreParaMostrar.Text ?? "").Trim();
    public string? Email => string.IsNullOrWhiteSpace(TxtEmail.Text) ? null : TxtEmail.Text.Trim();

    private string ContraseñaInicial => TxtPasswordVisible.Visibility == Visibility.Visible ? (TxtPasswordVisible.Text ?? "") : (TxtPassword.Password ?? "");
    private string ContraseñaRepetir => TxtPasswordRepetirVisible.Visibility == Visibility.Visible ? (TxtPasswordRepetirVisible.Text ?? "") : (TxtPasswordRepetir.Password ?? "");

    public string ContraseñaPlana => ContraseñaInicial;
    public bool RequiereCambioContraseña => ChkRequiereCambio.IsChecked == true;
    public IReadOnlyList<int> RolIdsSeleccionados => _roles.Where(r => r.IsSelected).Select(r => r.Id).ToList();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { /* ignorar */ }
        }
    }

    private void TxtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (TxtPassword.Password != TxtPasswordVisible.Text)
            TxtPassword.Password = TxtPasswordVisible.Text;
    }

    private void TxtPasswordRepetirVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (TxtPasswordRepetir.Password != TxtPasswordRepetirVisible.Text)
            TxtPasswordRepetir.Password = TxtPasswordRepetirVisible.Text;
    }

    private void BtnVerPassword_Click(object sender, RoutedEventArgs e)
    {
        if (TxtPasswordVisible.Visibility == Visibility.Visible)
        {
            TxtPassword.Password = TxtPasswordVisible.Text;
            TxtPassword.Visibility = Visibility.Visible;
            TxtPasswordVisible.Visibility = Visibility.Collapsed;
            TxtPasswordVisible.Clear();
            IconoOjo.Text = "\uE890";
            BtnVerPassword.ToolTip = "Mostrar contraseña";
        }
        else
        {
            TxtPasswordVisible.Text = TxtPassword.Password;
            TxtPasswordVisible.Visibility = Visibility.Visible;
            TxtPassword.Visibility = Visibility.Collapsed;
            IconoOjo.Text = "\uED1A";
            BtnVerPassword.ToolTip = "Ocultar contraseña";
            TxtPasswordVisible.Focus();
        }
    }

    private void BtnVerPasswordRepetir_Click(object sender, RoutedEventArgs e)
    {
        if (TxtPasswordRepetirVisible.Visibility == Visibility.Visible)
        {
            TxtPasswordRepetir.Password = TxtPasswordRepetirVisible.Text;
            TxtPasswordRepetir.Visibility = Visibility.Visible;
            TxtPasswordRepetirVisible.Visibility = Visibility.Collapsed;
            TxtPasswordRepetirVisible.Clear();
            IconoOjoRepetir.Text = "\uE890";
            BtnVerPasswordRepetir.ToolTip = "Mostrar contraseña";
        }
        else
        {
            TxtPasswordRepetirVisible.Text = TxtPasswordRepetir.Password;
            TxtPasswordRepetirVisible.Visibility = Visibility.Visible;
            TxtPasswordRepetir.Visibility = Visibility.Collapsed;
            IconoOjoRepetir.Text = "\uED1A";
            BtnVerPasswordRepetir.ToolTip = "Ocultar contraseña";
            TxtPasswordRepetirVisible.Focus();
        }
    }

    private void BtnAceptar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombreUsuario.Text))
        {
            MessageBox.Show("El nombre de usuario es obligatorio.", "Nuevo usuario", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNombreUsuario.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(TxtNombreParaMostrar.Text))
        {
            MessageBox.Show("El nombre para mostrar es obligatorio.", "Nuevo usuario", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNombreParaMostrar.Focus();
            return;
        }
        var pwd = ContraseñaInicial;
        if (pwd.Length < 6)
        {
            MessageBox.Show("La contraseña debe tener al menos 6 caracteres.", "Nuevo usuario", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (pwd != ContraseñaRepetir)
        {
            MessageBox.Show("Las contraseñas no coinciden.", "Nuevo usuario", MessageBoxButton.OK, MessageBoxImage.Warning);
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
