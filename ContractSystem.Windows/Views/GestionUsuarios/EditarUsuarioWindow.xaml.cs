using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ContractSystem.Application.Auth;
using ContractSystem.Windows.Models;

namespace ContractSystem.Windows.Views.GestionUsuarios;

public partial class EditarUsuarioWindow : Window
{
    private readonly ObservableCollection<RolSeleccionItem> _roles = new();
    private readonly ObservableCollection<PermisoSeleccionItem> _permisos = new();

    public EditarUsuarioWindow()
    {
        InitializeComponent();
        RolesPanel.ItemsSource = _roles;
    }

    public void CargarDatos(UsuarioEditDto usuario, IReadOnlyList<RolItem> todosLosRoles, IReadOnlyList<PermisoItem>? todosPermisos = null)
    {
        TxtNombreUsuario.Text = usuario.NombreUsuario;
        TxtNombreParaMostrar.Text = usuario.NombreParaMostrar;
        TxtEmail.Text = usuario.Email ?? "";
        ChkActivo.IsChecked = usuario.Activo;

        var rolIdsUsuario = usuario.RolIds.ToHashSet();
        _roles.Clear();
        foreach (var r in todosLosRoles)
            _roles.Add(new RolSeleccionItem { Id = r.Id, Nombre = r.Nombre, IsSelected = rolIdsUsuario.Contains(r.Id) });

        if (todosPermisos is not null)
        {
            var permisoIdsDirectos = usuario.PermisoDirectoIds.ToHashSet();
            _permisos.Clear();
            foreach (var p in todosPermisos)
            {
                _permisos.Add(new PermisoSeleccionItem
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Categoria = p.Categoria,
                    IsSelected = permisoIdsDirectos.Contains(p.Id)
                });
            }

            var view = CollectionViewSource.GetDefaultView(_permisos);
            view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PermisoSeleccionItem.Categoria)));
            PermisosPanel.ItemsSource = view;
        }
    }

    public string NombreParaMostrar => (TxtNombreParaMostrar.Text ?? "").Trim();
    public string? Email => string.IsNullOrWhiteSpace(TxtEmail.Text) ? null : TxtEmail.Text.Trim();
    public bool Activo => ChkActivo.IsChecked == true;
    public IReadOnlyList<int> RolIdsSeleccionados => _roles.Where(r => r.IsSelected).Select(r => r.Id).ToList();
    public IReadOnlyList<int> PermisoDirectoIdsSeleccionados => _permisos.Where(p => p.IsSelected).Select(p => p.Id).ToList();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { }
        }
    }

    private void BtnAceptar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombreParaMostrar.Text))
        {
            MessageBox.Show("El nombre para mostrar es obligatorio.", "Editar usuario", MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNombreParaMostrar.Focus();
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
