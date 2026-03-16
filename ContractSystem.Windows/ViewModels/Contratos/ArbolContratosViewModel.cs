using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Contratos.Queries.GetArbolContratos;
using ContractSystem.Domain.Contratos;
using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Windows.ViewModels;

public sealed partial class ArbolContratosViewModel : ObservableObject
{
    private readonly ISender _sender;
    private CancellationTokenSource? _debounceCts;
    private const int DebounceMs = 400;

    [ObservableProperty]
    private ObservableCollection<NodoArbolViewModel> _nodos = new();

    [ObservableProperty]
    private bool _estaCargando;

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
    private string _filtroTerceroTexto = string.Empty;

    public ArbolContratosViewModel(ISender sender)
    {
        _sender = sender;
        _ = CargarAsync();
    }

    [RelayCommand]
    private async Task CargarAsync(CancellationToken cancellationToken = default)
    {
        EstaCargando = true;
        try
        {
            var arbol = await _sender.Send(new GetArbolContratosQuery(
                FiltroEstado, FiltroTipo, FiltroRol, FiltroTerceroId,
                string.IsNullOrWhiteSpace(FiltroTerceroTexto) ? null : FiltroTerceroTexto), cancellationToken);
            Nodos.Clear();
            foreach (var nodo in arbol)
                Nodos.Add(new NodoArbolViewModel(nodo));
        }
        catch { }
        finally
        {
            EstaCargando = false;
        }
    }

    partial void OnFiltroTipoChanged(TipoDocumentoContrato? value) => _ = CargarAsync();
    partial void OnFiltroEstadoChanged(EstadoContrato? value) => _ = CargarAsync();
    partial void OnFiltroRolChanged(RolContrato? value) => _ = CargarAsync();
    partial void OnFiltroTerceroIdChanged(int? value) => _ = CargarAsync();

    partial void OnFiltroTerceroTextoChanged(string value)
    {
        _debounceCts?.Cancel();
        _debounceCts = new CancellationTokenSource();
        _ = DebounceCargarAsync(_debounceCts.Token);
    }

    private async Task DebounceCargarAsync(CancellationToken ct)
    {
        try { await Task.Delay(DebounceMs, ct); }
        catch (OperationCanceledException) { return; }
        await CargarAsync(ct);
    }
}

/// <summary>
/// ViewModel para cada nodo del TreeView.
/// </summary>
public class NodoArbolViewModel
{
    public int Id { get; }
    public string Numero { get; }
    public string Objeto { get; }
    public string TipoTexto { get; }
    public string EstadoTexto { get; }
    public string? TerceroNombre { get; }
    public string Icono { get; }
    public string ColorEstado { get; }
    public string Titulo { get; }
    public string TerceroDisplay { get; }
    public ObservableCollection<NodoArbolViewModel> Hijos { get; } = new();

    public NodoArbolViewModel(NodoArbol nodo)
    {
        Id = nodo.Id;
        Numero = nodo.Numero;
        Objeto = nodo.Objeto.Length > 60 ? nodo.Objeto[..60] + "..." : nodo.Objeto;
        TipoTexto = nodo.Tipo.ToString();
        EstadoTexto = nodo.Estado.ToString();
        TerceroNombre = nodo.TerceroNombre;

        Icono = nodo.Tipo switch
        {
            TipoDocumentoContrato.Marco => "\uE8A5",
            TipoDocumentoContrato.Especifico => "\uE7C3",
            TipoDocumentoContrato.Independiente => "\uE729",
            TipoDocumentoContrato.Suplemento => "\uE70F",
            _ => "\uE7C3"
        };

        ColorEstado = nodo.Estado switch
        {
            EstadoContrato.Borrador => "#888888",
            EstadoContrato.Vigente => "#28A745",
            EstadoContrato.Vencido => "#FFC107",
            EstadoContrato.Rescindido => "#DC3545",
            EstadoContrato.Ejecutado => "#17A2B8",
            _ => "#333333"
        };

        var etiquetaTipo = nodo.Tipo == TipoDocumentoContrato.Suplemento ? "Sup." : "Cont.";
        Titulo = $"{etiquetaTipo} {nodo.Numero} [{nodo.Estado}]";
        TerceroDisplay = string.IsNullOrEmpty(nodo.TerceroNombre) ? "" : $" | {nodo.TerceroNombre}";

        foreach (var hijo in nodo.Hijos)
            Hijos.Add(new NodoArbolViewModel(hijo));
    }
}
