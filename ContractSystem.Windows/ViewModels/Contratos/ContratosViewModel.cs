using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Contratos;
using ContractSystem.Application.Contratos.Commands.CreateContrato;
using ContractSystem.Application.Contratos.Commands.CreateSuplemento;
using ContractSystem.Application.Contratos.Commands.CambiarEstadoContrato;
using ContractSystem.Application.Contratos.Commands.DeleteContrato;
using ContractSystem.Application.Contratos.Commands.EjecutarContrato;
using ContractSystem.Application.Contratos.Commands.RescindirContrato;
using ContractSystem.Application.Contratos.Commands.UpdateContrato;
using ContractSystem.Application.Contratos.Queries.GetAllContratos;
using ContractSystem.Application.Contratos.Queries.GetContratosMarco;
using ContractSystem.Application.Contratos.Queries.GetPagedContratos;
using ContractSystem.Application.Contratos.Queries.GetDocumentosAfectadosPorRescision;
using ContractSystem.Application.Nomencladores;
using ContractSystem.Application.Nomencladores.Queries.GetAllTerceros;
using ContractSystem.Domain.Contratos;
using ContractSystem.Domain.Nomencladores;
using MediatR;
using System.Windows;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel principal del módulo de Contratos. Listado con filtros y acciones CRUD.
/// </summary>
public sealed partial class ContratosViewModel : ObservableObject
{
    private readonly ISender _sender;
    private readonly IDocumentoNumeracionService _numeracionService;

    [ObservableProperty]
    private ObservableCollection<Contrato> _contratos = new();

    [ObservableProperty]
    private Contrato? _seleccionado;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    // --- Filtros ---
    [ObservableProperty]
    private TipoDocumentoContrato? _filtroTipo;

    [ObservableProperty]
    private EstadoContrato? _filtroEstado;

    [ObservableProperty]
    private RolContrato? _filtroRol;

    [ObservableProperty]
    private int? _filtroTerceroId;

    [ObservableProperty]
    private string? _filtroBusqueda;

    [ObservableProperty]
    private string _filtroTerceroTexto = string.Empty;

    [ObservableProperty]
    private bool _includeDeleted;

    // Paginación
    private const int PageSize = 20;

    [ObservableProperty]
    private int _paginaActual = 1;

    [ObservableProperty]
    private int _totalPaginas;

    [ObservableProperty]
    private int _totalRegistros;

    [ObservableProperty]
    private string? _infoPaginacion;

    private CancellationTokenSource? _searchDebounceCts;
    private const int DebounceMs = 400;

    // --- Datos auxiliares para diálogos y filtros ---
    private IReadOnlyList<Contrato> _contratosMarco = Array.Empty<Contrato>();
    private IReadOnlyList<Tercero> _terceros = Array.Empty<Tercero>();

    public ContratosViewModel(ISender sender, IDocumentoNumeracionService numeracionService)
    {
        _sender = sender;
        _numeracionService = numeracionService;
        _ = CargarAsync();
    }

    [RelayCommand]
    private async Task CargarAsync(CancellationToken cancellationToken = default)
    {
        MensajeError = null;
        EstaCargando = true;
        try
        {
            var result = await _sender.Send(new GetPagedContratosQuery(
                PaginaActual,
                PageSize,
                IncludeDeleted,
                FiltroTipo,
                FiltroEstado,
                FiltroRol,
                FiltroTerceroId,
                TextoBusqueda: FiltroBusqueda,
                TextoTercero: string.IsNullOrWhiteSpace(FiltroTerceroTexto) ? null : FiltroTerceroTexto), cancellationToken);

            Contratos.Clear();
            foreach (var item in result.Items)
                Contratos.Add(item);

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

    [RelayCommand]
    private async Task NuevoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await CargarDatosAuxiliaresAsync(cancellationToken);

            var dialog = new Views.Contratos.ContratoDialogWindow();
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            dialog.CargarDatosAuxiliares(_contratosMarco, _terceros);
            dialog.SetNumeracionService(_numeracionService);

            if (dialog.ShowDialog() == true)
            {
                await CrearContratoAsync(dialog, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir formulario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task CrearContratoAsync(Views.Contratos.ContratoDialogWindow dialog, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new CreateContratoCommand(
                dialog.TipoDocumento,
                dialog.RolContrato,
                dialog.NumeroDocumento,
                dialog.ObjetoContrato,
                dialog.FechaFirma,
                dialog.FechaEntradaVigor,
                dialog.FechaVigencia,
                dialog.Duracion,
                dialog.MiEmpresaId,
                dialog.TerceroId,
                dialog.ContratoPadreId,
                dialog.ValorTotal,
                dialog.CondicionesEntrega,
                dialog.CostosAsociados), cancellationToken);

            MessageBox.Show("Contrato creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al crear: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private async Task EditarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        try
        {
            await CargarDatosAuxiliaresAsync(cancellationToken);

            var dialog = new Views.Contratos.ContratoDialogWindow();
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            dialog.CargarDatosAuxiliares(_contratosMarco, _terceros);
            dialog.CargarContrato(Seleccionado);

            if (dialog.ShowDialog() == true)
            {
                await ActualizarContratoAsync(dialog, Seleccionado.Id, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir formulario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ActualizarContratoAsync(Views.Contratos.ContratoDialogWindow dialog, int id, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new UpdateContratoCommand(
                id,
                dialog.NumeroDocumento,
                dialog.ObjetoContrato,
                dialog.RolContrato,
                dialog.FechaFirma,
                dialog.FechaEntradaVigor,
                dialog.FechaVigencia,
                dialog.Duracion,
                dialog.MiEmpresaId,
                dialog.TerceroId,
                dialog.ContratoPadreId,
                dialog.ValorTotal,
                dialog.CondicionesEntrega,
                dialog.CostosAsociados), cancellationToken);

            MessageBox.Show("Contrato actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al actualizar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeCrearSuplemento))]
    private async Task NuevoSuplementoAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        try
        {
            await CargarDatosAuxiliaresAsync(cancellationToken);

            // Obtener solo los documentos bajo el mismo padre (hermanos + padre)
            var todosContratos = await _sender.Send(new GetAllContratosQuery(), cancellationToken);
            var padreId = Seleccionado.ContratoPadreId ?? Seleccionado.Id;
            var documentosModificables = todosContratos
                .Where(c => c.Id == padreId || c.ContratoPadreId == padreId)
                .ToList() as IReadOnlyList<Contrato>;

            var dialog = new Views.Contratos.SuplementoDialogWindow();
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            dialog.SetNumeracionService(_numeracionService);
            dialog.ConfigurarParaContrato(Seleccionado, documentosModificables, _terceros);

            if (dialog.ShowDialog() == true)
            {
                await CrearSuplementoAsync(dialog, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir formulario: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task CrearSuplementoAsync(Views.Contratos.SuplementoDialogWindow dialog, CancellationToken cancellationToken)
    {
        try
        {
            var modificaciones = dialog.Modificaciones
                .Select(m => new ModificacionDto(m.DocumentoDestinoId, m.Descripcion))
                .ToList();

            await _sender.Send(new CreateSuplementoCommand(
                dialog.ContratoPadreId,
                dialog.RolContrato,
                dialog.NumeroDocumento,
                dialog.ObjetoContrato,
                dialog.EsModificacionGenerales,
                dialog.FechaFirma,
                dialog.FechaEntradaVigor,
                dialog.FechaVigencia,
                dialog.Duracion,
                dialog.MiEmpresaId,
                dialog.TerceroId,
                dialog.ValorTotal,
                dialog.CondicionesEntrega,
                dialog.CostosAsociados,
                modificaciones), cancellationToken);

            MessageBox.Show("Suplemento creado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al crear suplemento: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool PuedeCrearSuplemento() => Seleccionado is not null && !Seleccionado.IsDeleted
        && Seleccionado.Estado != EstadoContrato.Rescindido
        && Seleccionado.TipoDocumento != TipoDocumentoContrato.Suplemento;

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private void VerAnexosLineas()
    {
        if (Seleccionado is null) return;

        var window = new Views.Contratos.AnexosLineasWindow(_sender, Seleccionado.Id, Seleccionado.Numero);
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private void VerAdjuntos()
    {
        if (Seleccionado is null) return;

        var window = new Views.Contratos.DocumentosAdjuntosWindow(_sender, Seleccionado.Id, Seleccionado.Numero);
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    [RelayCommand(CanExecute = nameof(PuedeCambiarEstado))]
    private async Task CambiarEstadoAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        var dialog = new Views.Contratos.CambiarEstadoWindow(Seleccionado.Estado);
        dialog.Owner = System.Windows.Application.Current.MainWindow;

        if (dialog.ShowDialog() == true)
        {
            try
            {
                if (dialog.NuevoEstado == EstadoContrato.Rescindido)
                {
                    await RescindirConCascadaAsync(cancellationToken);
                }
                else if (dialog.NuevoEstado == EstadoContrato.Ejecutado)
                {
                    await EjecutarConPropuestaAsync(cancellationToken);
                }
                else
                {
                    await _sender.Send(new CambiarEstadoContratoCommand(
                        Seleccionado.Id, dialog.NuevoEstado), cancellationToken);
                    MessageBox.Show("Estado actualizado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                await CargarAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar estado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async Task RescindirConCascadaAsync(CancellationToken cancellationToken)
    {
        if (Seleccionado is null) return;

        var afectados = await _sender.Send(
            new GetDocumentosAfectadosPorRescisionQuery(Seleccionado.Id), cancellationToken);

        var mensaje = $"¿Rescindir el contrato '{Seleccionado.Numero}'?";
        if (afectados.Count > 0)
        {
            var listaAfectados = string.Join("\n", afectados.Select(a => $"  • {a.Numero} ({a.TipoDocumento})"));
            mensaje += $"\n\nLos siguientes documentos también serán rescindidos en cascada:\n{listaAfectados}";
        }

        var result = MessageBox.Show(mensaje, "Confirmar rescisión",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        var rescindidos = await _sender.Send(new RescindirContratoCommand(Seleccionado.Id), cancellationToken);
        MessageBox.Show(
            $"Rescisión completada. {rescindidos.Count} documento(s) rescindido(s).",
            "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task EjecutarConPropuestaAsync(CancellationToken cancellationToken)
    {
        if (Seleccionado is null) return;

        // Obtener suplementos del contrato (R08)
        var hijos = await _sender.Send(new GetAllContratosQuery(
            Tipo: TipoDocumentoContrato.Suplemento), cancellationToken);
        var suplementos = hijos.Where(h => h.ContratoPadreId == Seleccionado.Id
            && h.Estado != EstadoContrato.Ejecutado
            && h.Estado != EstadoContrato.Rescindido).ToList();

        var suplementosIds = new List<int>();
        if (suplementos.Count > 0)
        {
            var dialog = new Views.Contratos.EjecutarContratoWindow(Seleccionado, suplementos);
            dialog.Owner = System.Windows.Application.Current.MainWindow;
            if (dialog.ShowDialog() != true) return;
            suplementosIds = dialog.SuplementosSeleccionados.ToList();
        }
        else
        {
            var result = MessageBox.Show(
                $"¿Marcar '{Seleccionado.Numero}' como Ejecutado?",
                "Confirmar ejecución", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
        }

        await _sender.Send(new EjecutarContratoCommand(Seleccionado.Id, suplementosIds), cancellationToken);
        MessageBox.Show("Contrato marcado como ejecutado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand(CanExecute = nameof(HaySeleccionado))]
    private void VerHistorial()
    {
        if (Seleccionado is null) return;

        var window = new Views.Contratos.HistorialCambiosWindow(_sender, Seleccionado.Id, Seleccionado.Numero);
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    private bool PuedeCambiarEstado() => Seleccionado is not null && !Seleccionado.IsDeleted;

    [RelayCommand(CanExecute = nameof(PuedeEliminar))]
    private async Task EliminarAsync(CancellationToken cancellationToken = default)
    {
        if (Seleccionado is null) return;

        var result = MessageBox.Show(
            $"¿Eliminar el contrato '{Seleccionado.Numero}'?",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new DeleteContratoCommand(Seleccionado.Id), cancellationToken);
            MessageBox.Show("Contrato eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            await CargarAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool HaySeleccionado() => Seleccionado is not null;
    private bool PuedeEliminar() => Seleccionado is not null && !Seleccionado.IsDeleted;

    private async Task CargarDatosAuxiliaresAsync(CancellationToken cancellationToken)
    {
        _contratosMarco = await _sender.Send(new GetContratosMarcoQuery(), cancellationToken);
        _terceros = await _sender.Send(new GetAllTercerosQuery(), cancellationToken);
    }

    partial void OnFiltroTipoChanged(TipoDocumentoContrato? value) { PaginaActual = 1; _ = CargarAsync(); }
    partial void OnFiltroEstadoChanged(EstadoContrato? value) { PaginaActual = 1; _ = CargarAsync(); }
    partial void OnFiltroRolChanged(RolContrato? value) { PaginaActual = 1; _ = CargarAsync(); }
    partial void OnFiltroTerceroIdChanged(int? value) { PaginaActual = 1; _ = CargarAsync(); }
    partial void OnIncludeDeletedChanged(bool value) { PaginaActual = 1; _ = CargarAsync(); }

    partial void OnFiltroBusquedaChanged(string? value)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts = new CancellationTokenSource();
        _ = DebounceCargarAsync(_searchDebounceCts.Token);
    }

    partial void OnFiltroTerceroTextoChanged(string value)
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts = new CancellationTokenSource();
        _ = DebounceCargarAsync(_searchDebounceCts.Token);
    }

    private async Task DebounceCargarAsync(CancellationToken ct)
    {
        try { await Task.Delay(DebounceMs, ct); }
        catch (OperationCanceledException) { return; }
        PaginaActual = 1;
        await CargarAsync(ct);
    }

    partial void OnSeleccionadoChanged(Contrato? value)
    {
        EditarCommand.NotifyCanExecuteChanged();
        EliminarCommand.NotifyCanExecuteChanged();
        NuevoSuplementoCommand.NotifyCanExecuteChanged();
        VerAnexosLineasCommand.NotifyCanExecuteChanged();
        VerAdjuntosCommand.NotifyCanExecuteChanged();
        CambiarEstadoCommand.NotifyCanExecuteChanged();
        VerHistorialCommand.NotifyCanExecuteChanged();
    }
}
