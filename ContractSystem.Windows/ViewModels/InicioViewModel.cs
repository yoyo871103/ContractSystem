using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Contratos.Queries.GetAllContratos;
using ContractSystem.Application.Contratos.Queries.GetContratosProximosAVencer;
using ContractSystem.Application.Contratos.Queries.GetResumenContratos;
using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel de la pantalla de inicio con dashboard de contratos.
/// </summary>
public sealed partial class InicioViewModel : ObservableObject
{
    private readonly ISender _sender;

    [ObservableProperty]
    private string _mensajeBienvenida = "Bienvenido al Sistema de Contratos";

    [ObservableProperty]
    private int _totalContratos;

    [ObservableProperty]
    private int _borradores;

    [ObservableProperty]
    private int _vigentes;

    [ObservableProperty]
    private int _vencidos;

    [ObservableProperty]
    private int _rescindidos;

    [ObservableProperty]
    private int _ejecutados;

    [ObservableProperty]
    private int _proximosAVencer;

    [ObservableProperty]
    private ObservableCollection<Contrato> _contratosEnAlerta = new();

    [ObservableProperty]
    private ObservableCollection<Contrato> _contratosDetalle = new();

    [ObservableProperty]
    private string _tituloDetalle = "Próximos a vencer";

    [ObservableProperty]
    private string _filtroActivo = "proximos";

    public InicioViewModel(ISender sender)
    {
        _sender = sender;
        _ = CargarDashboardAsync();
    }

    [RelayCommand]
    private async Task CargarDashboardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var resumen = await _sender.Send(new GetResumenContratosQuery(), cancellationToken);
            TotalContratos = resumen.TotalContratos;
            Borradores = resumen.Borradores;
            Vigentes = resumen.Vigentes;
            Vencidos = resumen.Vencidos;
            Rescindidos = resumen.Rescindidos;
            Ejecutados = resumen.Ejecutados;
            ProximosAVencer = resumen.ProximosAVencer;

            // Cargar alertas
            var alertas = await _sender.Send(new GetContratosProximosAVencerQuery(), cancellationToken);
            ContratosEnAlerta.Clear();
            foreach (var c in alertas)
                ContratosEnAlerta.Add(c);

            // Mostrar próximos a vencer por defecto
            await FiltrarPorProximosAsync(cancellationToken);
        }
        catch { }
    }

    [RelayCommand]
    private async Task FiltrarPorTotalAsync(CancellationToken cancellationToken = default)
    {
        FiltroActivo = "total";
        TituloDetalle = "Todos los contratos";
        await CargarDetalleAsync(null, cancellationToken);
    }

    [RelayCommand]
    private async Task FiltrarPorBorradoresAsync(CancellationToken cancellationToken = default)
    {
        FiltroActivo = "borradores";
        TituloDetalle = "Contratos en Borrador";
        await CargarDetalleAsync(EstadoContrato.Borrador, cancellationToken);
    }

    [RelayCommand]
    private async Task FiltrarPorVigentesAsync(CancellationToken cancellationToken = default)
    {
        FiltroActivo = "vigentes";
        TituloDetalle = "Contratos Vigentes";
        await CargarDetalleAsync(EstadoContrato.Vigente, cancellationToken);
    }

    [RelayCommand]
    private async Task FiltrarPorVencidosAsync(CancellationToken cancellationToken = default)
    {
        FiltroActivo = "vencidos";
        TituloDetalle = "Contratos Vencidos";
        await CargarDetalleAsync(EstadoContrato.Vencido, cancellationToken);
    }

    [RelayCommand]
    private async Task FiltrarPorRescindidosAsync(CancellationToken cancellationToken = default)
    {
        FiltroActivo = "rescindidos";
        TituloDetalle = "Contratos Rescindidos";
        await CargarDetalleAsync(EstadoContrato.Rescindido, cancellationToken);
    }

    [RelayCommand]
    private async Task FiltrarPorEjecutadosAsync(CancellationToken cancellationToken = default)
    {
        FiltroActivo = "ejecutados";
        TituloDetalle = "Contratos Ejecutados";
        await CargarDetalleAsync(EstadoContrato.Ejecutado, cancellationToken);
    }

    [RelayCommand]
    private async Task FiltrarPorProximosAsync(CancellationToken cancellationToken = default)
    {
        FiltroActivo = "proximos";
        TituloDetalle = $"Próximos a vencer ({ProximosAVencer})";
        ContratosDetalle.Clear();
        foreach (var c in ContratosEnAlerta)
            ContratosDetalle.Add(c);
    }

    private async Task CargarDetalleAsync(EstadoContrato? estado, CancellationToken cancellationToken = default)
    {
        try
        {
            var lista = await _sender.Send(new GetAllContratosQuery(Estado: estado), cancellationToken);
            ContratosDetalle.Clear();
            foreach (var c in lista)
                ContratosDetalle.Add(c);
        }
        catch { }
    }
}
