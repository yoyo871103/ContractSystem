using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.Contratos.Queries.GetAllContratos;
using ContractSystem.Application.Nomencladores.Queries.GetAllTerceros;
using ContractSystem.Domain.Contratos;
using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel para la vista de Expediente por Tercero.
/// Muestra un TreeView donde el nodo raíz es el tercero y sus hijos son los contratos asociados.
/// </summary>
public sealed partial class ExpedienteTerceroViewModel : ObservableObject
{
    private readonly ISender _sender;

    [ObservableProperty]
    private ObservableCollection<Tercero> _terceros = new();

    [ObservableProperty]
    private Tercero? _terceroSeleccionado;

    [ObservableProperty]
    private ObservableCollection<NodoExpediente> _nodos = new();

    [ObservableProperty]
    private bool _estaCargando;

    [ObservableProperty]
    private string? _resumen;

    public ExpedienteTerceroViewModel(ISender sender)
    {
        _sender = sender;
        _ = CargarTercerosAsync();
    }

    private async Task CargarTercerosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var lista = await _sender.Send(new GetAllTercerosQuery(), cancellationToken);
            Terceros.Clear();
            foreach (var t in lista) Terceros.Add(t);
        }
        catch { }
    }

    partial void OnTerceroSeleccionadoChanged(Tercero? value) => _ = CargarExpedienteAsync();

    [RelayCommand]
    private async Task CargarExpedienteAsync(CancellationToken cancellationToken = default)
    {
        Nodos.Clear();
        Resumen = null;

        if (TerceroSeleccionado is null) return;

        EstaCargando = true;
        try
        {
            var tercero = TerceroSeleccionado;

            // Obtener todos los contratos del tercero
            var contratos = await _sender.Send(new GetAllContratosQuery(
                TerceroId: tercero.Id), cancellationToken);

            // Construir jerarquía: separar raíces (sin padre, o padre fuera del set) de hijos
            var ids = contratos.Select(c => c.Id).ToHashSet();
            var raices = contratos.Where(c => !c.ContratoPadreId.HasValue || !ids.Contains(c.ContratoPadreId.Value))
                .OrderBy(c => c.FechaFirma)
                .ToList();
            var hijosPorPadre = contratos.Where(c => c.ContratoPadreId.HasValue && ids.Contains(c.ContratoPadreId.Value))
                .GroupBy(c => c.ContratoPadreId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderBy(c => c.FechaFirma).ToList());

            // Nodo raíz = tercero
            var hijosDelTercero = new ObservableCollection<NodoExpediente>();
            foreach (var raiz in raices)
                hijosDelTercero.Add(ConstruirNodoContrato(raiz, hijosPorPadre));

            var tipoTexto = tercero.Tipo switch
            {
                TipoTercero.Cliente => "Cliente",
                TipoTercero.Proveedor => "Proveedor",
                TipoTercero.Ambos => "Cliente/Proveedor",
                _ => ""
            };

            var nodoRaiz = new NodoExpediente(
                tercero.Nombre,
                $"{tipoTexto} — {tercero.RazonSocial}",
                "\uE77B", // Person icon
                "#0078D4",
                true,
                hijosDelTercero);

            Nodos.Add(nodoRaiz);

            // Resumen
            var vigentes = contratos.Count(c => c.Estado == EstadoContrato.Vigente);
            var borradores = contratos.Count(c => c.Estado == EstadoContrato.Borrador);
            var vencidos = contratos.Count(c => c.Estado == EstadoContrato.Vencido);
            Resumen = $"{contratos.Count} contrato(s): {vigentes} vigente(s), {borradores} borrador(es), {vencidos} vencido(s)";
        }
        catch { }
        finally
        {
            EstaCargando = false;
        }
    }

    private static NodoExpediente ConstruirNodoContrato(Contrato contrato, Dictionary<int, List<Contrato>> hijosPorPadre)
    {
        var hijos = new ObservableCollection<NodoExpediente>();
        if (hijosPorPadre.TryGetValue(contrato.Id, out var lista))
        {
            foreach (var hijo in lista)
                hijos.Add(ConstruirNodoContrato(hijo, hijosPorPadre));
        }

        var icono = contrato.TipoDocumento switch
        {
            TipoDocumentoContrato.Marco => "\uE8A5",
            TipoDocumentoContrato.Especifico => "\uE7C3",
            TipoDocumentoContrato.Independiente => "\uE729",
            TipoDocumentoContrato.Suplemento => "\uE70F",
            _ => "\uE7C3"
        };

        var color = contrato.Estado switch
        {
            EstadoContrato.Borrador => "#888888",
            EstadoContrato.Vigente => "#28A745",
            EstadoContrato.Vencido => "#FFC107",
            EstadoContrato.Rescindido => "#DC3545",
            EstadoContrato.Ejecutado => "#17A2B8",
            _ => "#333333"
        };

        var objeto = contrato.Objeto.Length > 60 ? contrato.Objeto[..60] + "..." : contrato.Objeto;
        var titulo = $"{contrato.Numero} [{contrato.Estado}]";
        var subtitulo = $"{contrato.TipoDocumento} — {objeto}";

        return new NodoExpediente(titulo, subtitulo, icono, color, false, hijos, contrato.Id);
    }
}

/// <summary>
/// Nodo genérico para el TreeView del expediente. Puede ser un tercero o un contrato.
/// </summary>
public class NodoExpediente
{
    public int? ContratoId { get; }
    public string Titulo { get; }
    public string Subtitulo { get; }
    public string Icono { get; }
    public string Color { get; }
    public bool EsRaiz { get; }
    public ObservableCollection<NodoExpediente> Hijos { get; }
    public bool IsExpanded { get; set; }

    public NodoExpediente(string titulo, string subtitulo, string icono, string color, bool esRaiz, ObservableCollection<NodoExpediente> hijos, int? contratoId = null)
    {
        ContratoId = contratoId;
        Titulo = titulo;
        Subtitulo = subtitulo;
        Icono = icono;
        Color = color;
        EsRaiz = esRaiz;
        Hijos = hijos;
        IsExpanded = esRaiz;
    }
}
