using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventorySystem.Application.Auth;
using InventorySystem.Application.Common.Models;

namespace InventorySystem.Windows.ViewModels;

/// <summary>
/// ViewModel para la gestión de usuarios (solo visible en modo administrador SQL).
/// Listado paginado y carga asíncrona; permite filtrar, ordenar y resetear contraseña.
/// </summary>
public sealed partial class GestionUsuariosViewModel : ObservableObject
{
    private readonly IUsuarioStore _usuarioStore;
    private readonly IPasswordHasher _passwordHasher;

    private readonly UsuariosListQuery _query = new() { PageNumber = 1, RowsPerPage = 15 };

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

    public GestionUsuariosViewModel(IUsuarioStore usuarioStore, IPasswordHasher passwordHasher, IServiceProvider services)
    {
        _usuarioStore = usuarioStore;
        _passwordHasher = passwordHasher;
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

    private bool PuedeResetearContraseña() => UsuarioSeleccionado is not null;

    partial void OnUsuarioSeleccionadoChanged(UsuarioListItem? value)
    {
        ResetearContraseñaCommand.NotifyCanExecuteChanged();
    }
}
