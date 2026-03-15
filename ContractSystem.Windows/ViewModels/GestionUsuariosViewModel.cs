using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Auth;
using ContractSystem.Application.Common.Models;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel para la gestión de usuarios (solo visible en modo administrador SQL).
/// Listado paginado y carga asíncrona; permite filtrar, ordenar y resetear contraseña.
/// </summary>
public sealed partial class GestionUsuariosViewModel : ObservableObject
{
    private readonly IUsuarioStore _usuarioStore;
    private readonly IRolStore _rolStore;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthContext _authContext;

    private readonly UsuariosListQuery _query = new() { PageNumber = 1, RowsPerPage = 15 };

    /// <summary>Debounce: milisegundos de espera tras dejar de escribir antes de lanzar la búsqueda.</summary>
    private const int SearchDebounceMs = 500;

    private CancellationTokenSource? _searchDebounceCts;

    [ObservableProperty]
    private ObservableCollection<UsuarioListItem> _usuarios = new();

    [ObservableProperty]
    private UsuarioListItem? _usuarioSeleccionado;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    [ObservableProperty]
    private int _currentPage;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private int _totalRows;

    [ObservableProperty]
    private int _selectedPageSize;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _includeDeleted;

    partial void OnIncludeDeletedChanged(bool value)
    {
        _query.IncludeDeleted = value;
        _query.PageNumber = 1;
        _ = CargarUsuariosAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts = new CancellationTokenSource();
        var cts = _searchDebounceCts;
        _ = RunSearchAfterDebounceAsync(cts.Token);
    }

    private async Task RunSearchAfterDebounceAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(SearchDebounceMs, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        _query.PageNumber = 1;
        await CargarUsuariosAsync(cancellationToken);
    }

    /// <summary>True si el usuario actual es el admin por defecto de la app o el admin SQL (pueden editar a otros Administradores).</summary>
    private bool EsUsuarioAdminPorDefecto =>
        _authContext.IsSqlAdminOnly
        || string.Equals(_authContext.NombreUsuario, DefaultUsers.NombreUsuarioAdmin, StringComparison.OrdinalIgnoreCase);

    public GestionUsuariosViewModel(IUsuarioStore usuarioStore, IRolStore rolStore, IPasswordHasher passwordHasher, IAuthContext authContext, IServiceProvider services)
    {
        _usuarioStore = usuarioStore;
        _rolStore = rolStore;
        _passwordHasher = passwordHasher;
        _authContext = authContext;
        _ = CargarUsuariosAsync();
    }

    [RelayCommand]
    private async Task CargarUsuariosAsync(CancellationToken cancellationToken = default)
    {
        MensajeError = null;
        EstaCargando = true;
        try
        {
            _query.PageNumber = _query.PageNumber >= 1 ? _query.PageNumber : 1;
            _query.RowsPerPage = _query.RowsPerPage >= 1 ? _query.RowsPerPage : 15;
            _query.SearchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            _query.IncludeDeleted = IncludeDeleted;

            var result = await _usuarioStore.ListPagedAsync(_query, cancellationToken);

            Usuarios.Clear();
            foreach (var item in result.Items)
                Usuarios.Add(item);

            CurrentPage = result.CurrentPage;
            TotalPages = result.TotalPages;
            TotalRows = result.TotalRows;
            SelectedPageSize = result.SelectedPageSize;
        }
        catch (OperationCanceledException)
        {
            // Carga cancelada
        }
        catch (Exception ex)
        {
            MensajeError = "No se pudo cargar la lista de usuarios: " + ex.Message;
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HayPaginaAnterior))]
    private async Task PaginaAnteriorAsync(CancellationToken cancellationToken = default)
    {
        if (_query.PageNumber <= 1) return;
        _query.PageNumber--;
        await CargarUsuariosAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(HayPaginaSiguiente))]
    private async Task PaginaSiguienteAsync(CancellationToken cancellationToken = default)
    {
        if (_query.PageNumber >= TotalPages) return;
        _query.PageNumber++;
        await CargarUsuariosAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(HayPaginaAnterior))]
    private async Task PrimeraPaginaAsync(CancellationToken cancellationToken = default)
    {
        if (_query.PageNumber <= 1) return;
        _query.PageNumber = 1;
        await CargarUsuariosAsync(cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(HayPaginaSiguiente))]
    private async Task UltimaPaginaAsync(CancellationToken cancellationToken = default)
    {
        if (TotalPages <= 0 || _query.PageNumber >= TotalPages) return;
        _query.PageNumber = TotalPages;
        await CargarUsuariosAsync(cancellationToken);
    }

    private bool HayPaginaAnterior() => CurrentPage > 1;
    private bool HayPaginaSiguiente() => TotalPages > 0 && CurrentPage < TotalPages;

    partial void OnCurrentPageChanged(int value)
    {
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
        PrimeraPaginaCommand.NotifyCanExecuteChanged();
        UltimaPaginaCommand.NotifyCanExecuteChanged();
    }

    partial void OnTotalPagesChanged(int value)
    {
        PaginaAnteriorCommand.NotifyCanExecuteChanged();
        PaginaSiguienteCommand.NotifyCanExecuteChanged();
        PrimeraPaginaCommand.NotifyCanExecuteChanged();
        UltimaPaginaCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(PuedeResetearContraseña))]
    private async Task ResetearContraseñaAsync()
    {
        if (UsuarioSeleccionado is null)
            return;

        var dialog = new Views.GestionUsuarios.ResetPasswordWindow
        {
            NombreUsuario = UsuarioSeleccionado.NombreUsuario,
            Owner = System.Windows.Application.Current.MainWindow
        };
        if (dialog.ShowDialog() != true)
            return;

        var contraseñaTemporal = dialog.ContraseñaTemporal;
        if (string.IsNullOrWhiteSpace(contraseñaTemporal))
            return;

        MensajeError = null;
        try
        {
            var (hash, salt) = _passwordHasher.HashPassword(contraseñaTemporal);
            await _usuarioStore.UpdatePasswordAsync(UsuarioSeleccionado.Id, hash, salt, requiereCambioContraseña: true, default);

            System.Windows.MessageBox.Show(
                $"Contraseña temporal establecida para '{UsuarioSeleccionado.NombreUsuario}'. El usuario deberá cambiarla en el próximo inicio de sesión.",
                "Gestión de usuarios",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            await CargarUsuariosAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "Error al actualizar la contraseña: " + ex.Message;
            System.Windows.MessageBox.Show(MensajeError, "Gestión de usuarios",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    private bool PuedeResetearContraseña() => UsuarioSeleccionado is not null && !UsuarioSeleccionado.IsDeleted
        && UsuarioSeleccionado.Id != _authContext.UsuarioId
        && (!UsuarioSeleccionado.IsDefaultAdmin || _authContext.IsSqlAdminOnly)
        && (!UsuarioSeleccionado.IsAdministrador || EsUsuarioAdminPorDefecto);

    [RelayCommand(CanExecute = nameof(PuedeEditarUsuario))]
    private async Task EditarUsuarioAsync()
    {
        if (UsuarioSeleccionado is null) return;

        var roles = await _rolStore.GetAllAsync(default);
        var dto = await _usuarioStore.GetByIdForEditAsync(UsuarioSeleccionado.Id, default);
        if (dto is null)
        {
            MensajeError = "No se encontró el usuario.";
            return;
        }

        var dialog = new Views.GestionUsuarios.EditarUsuarioWindow
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        dialog.CargarDatos(dto, roles);
        if (dialog.ShowDialog() != true) return;

        MensajeError = null;
        try
        {
            await _usuarioStore.UpdateUsuarioAsync(
                UsuarioSeleccionado.Id,
                dialog.NombreParaMostrar,
                dialog.Email,
                dialog.Activo,
                dialog.RolIdsSeleccionados,
                default);
            System.Windows.MessageBox.Show("Usuario actualizado correctamente.", "Gestión de usuarios",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            await CargarUsuariosAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "Error al actualizar: " + ex.Message;
        }
    }

    private bool PuedeEditarUsuario() => UsuarioSeleccionado is not null && !UsuarioSeleccionado.IsDeleted
        && UsuarioSeleccionado.Id != _authContext.UsuarioId
        && (!UsuarioSeleccionado.IsDefaultAdmin || _authContext.IsSqlAdminOnly)
        && (!UsuarioSeleccionado.IsAdministrador || EsUsuarioAdminPorDefecto);

    [RelayCommand]
    private async Task NuevoUsuarioAsync()
    {
        var roles = await _rolStore.GetAllAsync(default);
        var dialog = new Views.GestionUsuarios.CrearUsuarioWindow
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        dialog.CargarRoles(roles);
        if (dialog.ShowDialog() != true) return;

        MensajeError = null;
        try
        {
            var (hash, salt) = _passwordHasher.HashPassword(dialog.ContraseñaPlana);
            var request = new CreateUsuarioRequest
            {
                NombreUsuario = dialog.NombreUsuario,
                NombreParaMostrar = dialog.NombreParaMostrar,
                Email = dialog.Email,
                HashContraseña = hash,
                Salt = salt,
                RequiereCambioContraseña = dialog.RequiereCambioContraseña,
                RolIds = dialog.RolIdsSeleccionados
            };
            var creado = await _usuarioStore.CreateAsync(request, default);
            if (creado is null)
            {
                MensajeError = "No se pudo crear el usuario. Compruebe que el nombre de usuario no exista.";
                return;
            }
            System.Windows.MessageBox.Show("Usuario creado correctamente.", "Gestión de usuarios",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            await CargarUsuariosAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "Error al crear usuario: " + ex.Message;
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeEliminarUsuario))]
    private async Task EliminarUsuarioAsync()
    {
        if (UsuarioSeleccionado is null) return;

        var result = System.Windows.MessageBox.Show(
            $"¿Eliminar el usuario '{UsuarioSeleccionado.NombreUsuario}'? El usuario no podrá iniciar sesión.",
            "Gestión de usuarios",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result != System.Windows.MessageBoxResult.Yes) return;

        MensajeError = null;
        try
        {
            await _usuarioStore.SetDeletedAsync(UsuarioSeleccionado.Id, default);
            System.Windows.MessageBox.Show("Usuario eliminado.", "Gestión de usuarios",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            await CargarUsuariosAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "Error al eliminar: " + ex.Message;
        }
    }

    private bool PuedeEliminarUsuario() => UsuarioSeleccionado is not null && !UsuarioSeleccionado.IsDeleted
        && UsuarioSeleccionado.Id != _authContext.UsuarioId
        && (!UsuarioSeleccionado.IsDefaultAdmin || _authContext.IsSqlAdminOnly)
        && (!UsuarioSeleccionado.IsAdministrador || EsUsuarioAdminPorDefecto);

    [RelayCommand(CanExecute = nameof(PuedeReactivarUsuario))]
    private async Task ReactivarUsuarioAsync()
    {
        if (UsuarioSeleccionado is null || !UsuarioSeleccionado.IsDeleted)
            return;

        var result = System.Windows.MessageBox.Show(
            $"¿Reactivar el usuario '{UsuarioSeleccionado.NombreUsuario}'? Podrá iniciar sesión de nuevo.",
            "Gestión de usuarios",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        MensajeError = null;
        try
        {
            await _usuarioStore.SetUndeletedAsync(UsuarioSeleccionado.Id, default);
            System.Windows.MessageBox.Show("Usuario reactivado correctamente.", "Gestión de usuarios",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            await CargarUsuariosAsync();
        }
        catch (Exception ex)
        {
            MensajeError = "Error al reactivar: " + ex.Message;
        }
    }

    private bool PuedeReactivarUsuario() => UsuarioSeleccionado is not null && UsuarioSeleccionado.IsDeleted
        && (!UsuarioSeleccionado.IsDefaultAdmin || _authContext.IsSqlAdminOnly)
        && (!UsuarioSeleccionado.IsAdministrador || EsUsuarioAdminPorDefecto);

    partial void OnUsuarioSeleccionadoChanged(UsuarioListItem? value)
    {
        ResetearContraseñaCommand.NotifyCanExecuteChanged();
        EditarUsuarioCommand.NotifyCanExecuteChanged();
        EliminarUsuarioCommand.NotifyCanExecuteChanged();
        ReactivarUsuarioCommand.NotifyCanExecuteChanged();
    }
}
