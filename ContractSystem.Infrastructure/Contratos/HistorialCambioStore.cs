using ContractSystem.Application.Contratos;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class HistorialCambioStore : IHistorialCambioStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public HistorialCambioStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<HistorialCambio>> GetByContratoAsync(int contratoId, TipoCambio? filtroTipo = null, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<HistorialCambio>();

        var query = db.Set<HistorialCambio>()
            .Where(h => h.ContratoId == contratoId);

        if (filtroTipo.HasValue)
            query = query.Where(h => h.TipoCambio == filtroTipo.Value);

        return await query
            .OrderByDescending(h => h.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateAsync(HistorialCambio historial, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<HistorialCambio>().Add(historial);
        await db.SaveChangesAsync(cancellationToken);
    }
}
