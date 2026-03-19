using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Contratos.Queries.GetInformesContratos;
using MediatR;

namespace ContractSystem.Windows.ViewModels;

public sealed partial class InformesViewModel : ObservableObject
{
    private readonly ISender _sender;

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _mensajeError;

    [ObservableProperty]
    private string _informeSeleccionado = "Facturación por contrato";

    // --- Colecciones para cada informe ---

    [ObservableProperty]
    private ObservableCollection<FacturacionPorContratoDto> _facturacionPorContrato = new();

    [ObservableProperty]
    private ObservableCollection<ContratosPorTerceroDto> _contratosPorTercero = new();

    [ObservableProperty]
    private ObservableCollection<ContratosPorTipoDto> _contratosPorTipo = new();

    [ObservableProperty]
    private ObservableCollection<ContratosPorEstadoDto> _contratosPorEstado = new();

    [ObservableProperty]
    private ObservableCollection<ContratoProximoVencerDto> _proximosAVencer = new();

    [ObservableProperty]
    private ObservableCollection<ContratosPorRolDto> _contratosPorRol = new();

    // --- Totales resumen ---

    [ObservableProperty]
    private decimal _totalValorContratos;

    [ObservableProperty]
    private decimal _totalFacturado;

    [ObservableProperty]
    private int _totalContratos;

    [ObservableProperty]
    private int _totalFacturas;

    public InformesViewModel(ISender sender)
    {
        _sender = sender;
        _ = CargarAsync();
    }

    [RelayCommand]
    private async Task CargarAsync(CancellationToken cancellationToken = default)
    {
        MensajeError = null;
        EstaCargando = true;
        try
        {
            var result = await _sender.Send(new GetInformesContratosQuery(), cancellationToken);

            // Facturación por contrato
            FacturacionPorContrato.Clear();
            foreach (var item in result.FacturacionPorContrato)
                FacturacionPorContrato.Add(item);

            // Contratos por tercero
            ContratosPorTercero.Clear();
            foreach (var item in result.ContratosPorTercero)
                ContratosPorTercero.Add(item);

            // Contratos por tipo
            ContratosPorTipo.Clear();
            foreach (var item in result.ContratosPorTipo)
                ContratosPorTipo.Add(item);

            // Contratos por estado
            ContratosPorEstado.Clear();
            foreach (var item in result.ContratosPorEstado)
                ContratosPorEstado.Add(item);

            // Próximos a vencer
            ProximosAVencer.Clear();
            foreach (var item in result.ProximosAVencer)
                ProximosAVencer.Add(item);

            // Contratos por rol
            ContratosPorRol.Clear();
            foreach (var item in result.ContratosPorRol)
                ContratosPorRol.Add(item);

            // Totales
            TotalValorContratos = result.FacturacionPorContrato.Sum(f => f.ValorContrato);
            TotalFacturado = result.FacturacionPorContrato.Sum(f => f.TotalFacturado);
            TotalContratos = result.ContratosPorEstado.Sum(e => e.Cantidad);
            TotalFacturas = result.FacturacionPorContrato.Sum(f => f.CantidadFacturas);
        }
        catch (Exception ex)
        {
            MensajeError = "Error al cargar informes: " + ex.Message;
        }
        finally
        {
            EstaCargando = false;
        }
    }
}
