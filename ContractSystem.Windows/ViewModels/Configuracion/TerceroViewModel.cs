using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Nomencladores.Commands.CreateTercero;
using ContractSystem.Application.Nomencladores.Commands.DeleteTercero;
using ContractSystem.Application.Nomencladores.Commands.ReactivarTercero;
using ContractSystem.Application.Nomencladores.Commands.UpdateTercero;
using ContractSystem.Application.Nomencladores.Queries.GetPagedTerceros;
using ContractSystem.Application.Nomencladores.Queries.GetTerceroById;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Application.Auth;
using MediatR;
using System.Windows;

namespace ContractSystem.Windows.ViewModels;

public sealed partial class TerceroViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly IAuthContext _authContext;
    private const int PageSize = 20;

    [ObservableProperty]
    private ObservableCollection<Tercero> _terceros = new();

    [ObservableProperty]
    private Tercero? _seleccionado;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    [ObservableProperty]
    private bool _includeDeleted;

    [ObservableProperty]
    private TipoTercero? _filtroTipo;

    [ObservableProperty]
    private string? _busqueda;

    // Paginación
    [ObservableProperty]
    private int _paginaActual = 1;

    [ObservableProperty]
    private int _totalPaginas;

    [ObservableProperty]
    private int _totalRegistros;

    [ObservableProperty]
    private string? _infoPaginacion;

    private CancellationTokenSource? _searchCts;

    public bool PuedeCrearTercero => _authContext.TienePermiso(Permissions.TercerosCrear);
    public bool PuedeEditarTercero => _authContext.TienePermiso(Permissions.TercerosEditar);
    public bool PuedeEliminarTercero => _authContext.TienePermiso(Permissions.TercerosEliminar);

    public TerceroViewModel(ISender sender, IAuthContext authContext)
    {
        _sender = sender;
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
            var result = await _sender.Send(new GetPagedTercerosQuery(
                PaginaActual, PageSize, IncludeDeleted, FiltroTipo, Busqueda), cancellationToken);

            Terceros.Clear();
            foreach (var item in result.Items)
                Terceros.Add(item);

            PaginaActual = result.CurrentPage;
            TotalPaginas = result.TotalPages;
            TotalRegistros = result.TotalRows;
            InfoPaginacion = TotalRegistros > 0
                ? $"Página {PaginaActual} de {TotalPaginas} ({TotalRegistros} registros)"
                : "Sin resultados";

            PaginaAnteriorCommand.NotifyCanExecuteChanged();
            PaginaSiguienteCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            MensajeError = "No se pudo cargar la lista: " + ex.Message;
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeRetroceder))]
    private async Task PaginaAnteriorAsync()
    {
        PaginaActual--;
        await CargarAsync();
    }

    [RelayCommand(CanExecute = nameof(PuedeAvanzar))]
    private async Task PaginaSiguienteAsync()
    {
        PaginaActual++;
        await CargarAsync();
    }

    private bool PuedeRetroceder() => PaginaActual > 1;
    private bool PuedeAvanzar() => PaginaActual < TotalPaginas;

    [RelayCommand(CanExecute = nameof(PuedeCrearTercero))]
    private void Nuevo()
    {
        var dialog = new Views.Configuracion.TerceroDialogWindow();
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        if (dialog.ShowDialog() == true)
        {
            _ = CrearTerceroAsync(dialog);
        }
    }

    private async Task CrearTerceroAsync(Views.Configuracion.TerceroDialogWindow dialog)
    {
        try
        {
            var contactos = dialog.Contactos.Select(c =>
                new ContactoTerceroDto(c.Nombre, c.Cargo, c.Email, c.Telefono)).ToList();

            await _sender.Send(new CreateTerceroCommand(
                dialog.CodigoTercero,
                dialog.NombreTercero,
                dialog.RazonSocial,
                dialog.NifCif,
                dialog.Direccion,
                dialog.TelefonoTercero,
                dialog.EmailTercero,
                dialog.TipoTercero,
                dialog.UbicacionExpediente,
                contactos));

            MessageBox.Show("Tercero creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al crear: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private void Editar()
    {
        if (Seleccionado is null) return;

        var dialog = new Views.Configuracion.TerceroDialogWindow();
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        dialog.CargarTercero(Seleccionado);
        if (dialog.ShowDialog() == true)
        {
            _ = ActualizarTerceroAsync(dialog, Seleccionado.Id);
        }
    }

    private async Task ActualizarTerceroAsync(Views.Configuracion.TerceroDialogWindow dialog, int id)
    {
        try
        {
            var contactos = dialog.Contactos.Select(c =>
                new ContactoTerceroDto(c.Nombre, c.Cargo, c.Email, c.Telefono)).ToList();

            await _sender.Send(new UpdateTerceroCommand(
                id,
                dialog.CodigoTercero,
                dialog.NombreTercero,
                dialog.RazonSocial,
                dialog.NifCif,
                dialog.Direccion,
                dialog.TelefonoTercero,
                dialog.EmailTercero,
                dialog.TipoTercero,
                dialog.UbicacionExpediente,
                contactos));

            MessageBox.Show("Tercero actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool HaySeleccionado() => Seleccionado is not null && _authContext.TienePermiso(Permissions.TercerosEditar);

    [RelayCommand(CanExecute = nameof(PuedeEliminar))]
    private async Task EliminarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        var result = MessageBox.Show(
            $"¿Eliminar el tercero '{Seleccionado.Nombre}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new DeleteTerceroCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Tercero eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool PuedeEliminar() => Seleccionado is not null && !Seleccionado.IsDeleted && _authContext.TienePermiso(Permissions.TercerosEliminar);

    [RelayCommand(CanExecute = nameof(PuedeReactivar))]
    private async Task ReactivarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null || !Seleccionado.IsDeleted) return;

        var result = MessageBox.Show(
            $"¿Reactivar el tercero '{Seleccionado.Nombre}'?",
            "Confirmar reactivación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new ReactivarTerceroCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Tercero reactivado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al reactivar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool PuedeReactivar() => Seleccionado is not null && Seleccionado.IsDeleted && _authContext.TienePermiso(Permissions.TercerosEliminar);

    partial void OnIncludeDeletedChanged(bool value) { PaginaActual = 1; _ = CargarAsync(); }
    partial void OnFiltroTipoChanged(TipoTercero? value) { PaginaActual = 1; _ = CargarAsync(); }

    partial void OnBusquedaChanged(string? value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        _ = DebounceBuscarAsync(_searchCts.Token);
    }

    private async Task DebounceBuscarAsync(CancellationToken ct)
    {
        try { await Task.Delay(400, ct); }
        catch (OperationCanceledException) { return; }
        PaginaActual = 1;
        await CargarAsync(ct);
    }

    partial void OnSeleccionadoChanged(Tercero? value)
    {
        EditarCommand.NotifyCanExecuteChanged();
        EliminarCommand.NotifyCanExecuteChanged();
        ReactivarCommand.NotifyCanExecuteChanged();
    }
}
