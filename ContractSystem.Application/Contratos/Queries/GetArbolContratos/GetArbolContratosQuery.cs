using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetArbolContratos;

/// <summary>
/// Nodo del árbol jerárquico de contratos.
/// </summary>
public record NodoArbol(
    int Id,
    string Numero,
    string Objeto,
    TipoDocumentoContrato Tipo,
    EstadoContrato Estado,
    RolContrato Rol,
    DateTime? FechaFirma,
    DateTime? FechaVigencia,
    string? TerceroNombre,
    IReadOnlyList<NodoArbol> Hijos);

/// <summary>
/// Obtiene la estructura jerárquica completa de contratos para la vista de árbol.
/// </summary>
public record GetArbolContratosQuery(
    EstadoContrato? FiltroEstado = null,
    TipoDocumentoContrato? FiltroTipo = null,
    RolContrato? FiltroRol = null,
    int? FiltroTerceroId = null) : IRequest<IReadOnlyList<NodoArbol>>;

public class GetArbolContratosQueryHandler : IRequestHandler<GetArbolContratosQuery, IReadOnlyList<NodoArbol>>
{
    private readonly IContratoStore _store;

    public GetArbolContratosQueryHandler(IContratoStore store) => _store = store;

    public async Task<IReadOnlyList<NodoArbol>> Handle(GetArbolContratosQuery request, CancellationToken cancellationToken)
    {
        // Obtener todos los contratos (sin filtros de texto para construir jerarquía)
        var todos = await _store.GetAllAsync(cancellationToken: cancellationToken);

        // Construir mapa de hijos
        var porId = todos.ToDictionary(c => c.Id);
        var raices = todos.Where(c => c.ContratoPadreId == null)
            .OrderBy(c => c.FechaFirma)
            .ToList();

        var hijosPorPadre = todos.Where(c => c.ContratoPadreId.HasValue)
            .GroupBy(c => c.ContratoPadreId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.FechaFirma).ToList());

        var resultado = new List<NodoArbol>();
        foreach (var raiz in raices)
        {
            var nodo = ConstruirNodo(raiz, hijosPorPadre, request);
            if (nodo is not null)
                resultado.Add(nodo);
        }

        return resultado;
    }

    private NodoArbol? ConstruirNodo(
        Contrato contrato,
        Dictionary<int, List<Contrato>> hijosPorPadre,
        GetArbolContratosQuery filtros)
    {
        var hijosNodos = new List<NodoArbol>();
        if (hijosPorPadre.TryGetValue(contrato.Id, out var hijos))
        {
            foreach (var hijo in hijos)
            {
                var nodoHijo = ConstruirNodo(hijo, hijosPorPadre, filtros);
                if (nodoHijo is not null)
                    hijosNodos.Add(nodoHijo);
            }
        }

        // Aplicar filtros: incluir si el nodo coincide o tiene hijos que coinciden
        var coincide = CumpleFiltros(contrato, filtros);
        if (!coincide && hijosNodos.Count == 0)
            return null;

        return new NodoArbol(
            contrato.Id,
            contrato.Numero,
            contrato.Objeto,
            contrato.TipoDocumento,
            contrato.Estado,
            contrato.Rol,
            contrato.FechaFirma,
            contrato.FechaVigencia,
            contrato.Tercero?.Nombre,
            hijosNodos);
    }

    private static bool CumpleFiltros(Contrato c, GetArbolContratosQuery f)
    {
        if (f.FiltroEstado.HasValue && c.Estado != f.FiltroEstado.Value) return false;
        if (f.FiltroTipo.HasValue && c.TipoDocumento != f.FiltroTipo.Value) return false;
        if (f.FiltroRol.HasValue && c.Rol != f.FiltroRol.Value) return false;
        if (f.FiltroTerceroId.HasValue && c.TerceroId != f.FiltroTerceroId.Value) return false;
        return true;
    }
}
