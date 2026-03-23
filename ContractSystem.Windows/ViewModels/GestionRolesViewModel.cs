using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Auth;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel para la gestión de roles y permisos.
/// </summary>
public sealed partial class GestionRolesViewModel : ObservableObject
{
    private readonly IRolStore _rolStore;
    private readonly IAuthContext _authContext;

    [ObservableProperty]
    private ObservableCollection<RolListItem> _roles = new();

    [ObservableProperty]
    private RolListItem? _rolSeleccionado;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    public GestionRolesViewModel(IRolStore rolStore, IAuthContext authContext)
    {
        _rolStore = rolStore;
        _authContext = authContext;
        _ = CargarAsync();
    }

    [RelayCommand]
    private async Task CargarAsync(CancellationToken cancellationToken = default)
    {
        MensajeError = null;
        EstaCargando = true;
        try
        {
            var result = await _rolStore.GetAllWithDetailsAsync(cancellationToken);
            Roles.Clear();
            foreach (var item in result)
                Roles.Add(item);
        }
        catch (Exception ex)
        {
            MensajeError = "Error al cargar roles: " + ex.Message;
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task NuevoRolAsync()
    {
        var permisos = await _rolStore.GetAllPermisosAsync(default);
        var dialog = new Views.GestionUsuarios.CrearEditarRolWindow
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        dialog.CargarPermisos(permisos, null);
        if (dialog.ShowDialog() != true) return;

        MensajeError = null;
        try
        {
            if (await _rolStore.ExisteNombreAsync(dialog.NombreRol, null, default))
            {
                MensajeError = "Ya existe un rol con ese nombre.";
                return;
            }
            await _rolStore.CreateAsync(dialog.NombreRol, dialog.DescripcionRol, dialog.PermisoIdsSeleccionados, default);
            System.Windows.MessageBox.Show("Rol creado correctamente.", "Gestión de roles",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "Error al crear rol: " + ex.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeEditarRol))]
    private async Task EditarRolAsync()
    {
        if (RolSeleccionado is null) return;

        var permisos = await _rolStore.GetAllPermisosAsync(default);
        var detail = await _rolStore.GetByIdAsync(RolSeleccionado.Id, default);
        if (detail is null)
        {
            MensajeError = "No se encontró el rol.";
            return;
        }

        var dialog = new Views.GestionUsuarios.CrearEditarRolWindow
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        dialog.CargarPermisos(permisos, detail);
        if (dialog.ShowDialog() != true) return;

        MensajeError = null;
        try
        {
            if (await _rolStore.ExisteNombreAsync(dialog.NombreRol, RolSeleccionado.Id, default))
            {
                MensajeError = "Ya existe un rol con ese nombre.";
                return;
            }
            await _rolStore.UpdateAsync(RolSeleccionado.Id, dialog.NombreRol, dialog.DescripcionRol, dialog.PermisoIdsSeleccionados, default);
            System.Windows.MessageBox.Show("Rol actualizado correctamente.", "Gestión de roles",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "Error al actualizar rol: " + ex.Message;
        }
    }

    private bool PuedeEditarRol() => RolSeleccionado is not null && !RolSeleccionado.EsSistema;

    [RelayCommand(CanExecute = nameof(PuedeEliminarRol))]
    private async Task EliminarRolAsync()
    {
        if (RolSeleccionado is null) return;

        var msg = RolSeleccionado.CantidadUsuarios > 0
            ? $"El rol '{RolSeleccionado.Nombre}' tiene {RolSeleccionado.CantidadUsuarios} usuario(s) asignado(s). ¿Eliminar de todas formas?"
            : $"¿Eliminar el rol '{RolSeleccionado.Nombre}'?";

        var result = System.Windows.MessageBox.Show(msg, "Gestión de roles",
            System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
        if (result != System.Windows.MessageBoxResult.Yes) return;

        MensajeError = null;
        try
        {
            await _rolStore.DeleteAsync(RolSeleccionado.Id, default);
            System.Windows.MessageBox.Show("Rol eliminado.", "Gestión de roles",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "Error al eliminar rol: " + ex.Message;
        }
    }

    private bool PuedeEliminarRol() => RolSeleccionado is not null && !RolSeleccionado.EsSistema;

    partial void OnRolSeleccionadoChanged(RolListItem? value)
    {
        EditarRolCommand.NotifyCanExecuteChanged();
        EliminarRolCommand.NotifyCanExecuteChanged();
    }
}
