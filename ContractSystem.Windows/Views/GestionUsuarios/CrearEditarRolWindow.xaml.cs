using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ContractSystem.Application.Auth;
using ContractSystem.Windows.Models;

namespace ContractSystem.Windows.Views.GestionUsuarios;

public partial class CrearEditarRolWindow : Window
{
    private readonly ObservableCollection<PermisoSeleccionItem> _permisos = new();
    private bool _esEdicion;

    public CrearEditarRolWindow()
    {
        InitializeComponent();
    }

    public void CargarPermisos(IReadOnlyList<PermisoItem> todosPermisos, RolDetailDto? rolExistente)
    {
        _esEdicion = rolExistente is not null;
        TxtTitulo.Text = _esEdicion ? "Editar rol" : "Nuevo rol";
        Title = TxtTitulo.Text;

        if (rolExistente is not null)
        {
            TxtNombre.Text = rolExistente.Nombre;
            TxtDescripcion.Text = rolExistente.Descripcion ?? "";
        }

        var permisoIdsSeleccionados = rolExistente?.PermisoIds.ToHashSet() ?? new HashSet<int>();
        _permisos.Clear();
        foreach (var p in todosPermisos)
        {
            _permisos.Add(new PermisoSeleccionItem
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Categoria = p.Categoria,
                IsSelected = permisoIdsSeleccionados.Contains(p.Id)
            });
        }

        var view = CollectionViewSource.GetDefaultView(_permisos);
        view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(PermisoSeleccionItem.Categoria)));
        PermisosPanel.ItemsSource = view;
    }

    public string NombreRol => (TxtNombre.Text ?? "").Trim();
    public string? DescripcionRol => string.IsNullOrWhiteSpace(TxtDescripcion.Text) ? null : TxtDescripcion.Text.Trim();
    public IReadOnlyList<int> PermisoIdsSeleccionados => _permisos.Where(p => p.IsSelected).Select(p => p.Id).ToList();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); } catch { }
        }
    }

    private void BtnSeleccionarTodos_Click(object sender, RoutedEventArgs e)
    {
        foreach (var p in _permisos)
            p.IsSelected = true;
    }

    private void BtnDeseleccionarTodos_Click(object sender, RoutedEventArgs e)
    {
        foreach (var p in _permisos)
            p.IsSelected = false;
    }

    private void BtnAceptar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNombre.Text))
        {
            MessageBox.Show("El nombre del rol es obligatorio.", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
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
