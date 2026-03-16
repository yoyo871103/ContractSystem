using ContractSystem.Application.Contratos;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class ModificacionDocumentoStore : IModificacionDocumentoStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public ModificacionDocumentoStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<ModificacionDocumento>> GetModificaAAsync(int documentoOrigenId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<ModificacionDocumento>();

        return await db.Set<ModificacionDocumento>()
            .Include(m => m.DocumentoDestino)
            .Where(m => m.DocumentoOrigenId == documentoOrigenId)
            .OrderBy(m => m.FechaRegistro)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ModificacionDocumento>> GetModificadoPorAsync(int documentoDestinoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<ModificacionDocumento>();

        return await db.Set<ModificacionDocumento>()
            .Include(m => m.DocumentoOrigen)
            .Where(m => m.DocumentoDestinoId == documentoDestinoId)
            .OrderBy(m => m.FechaRegistro)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteRelacionAsync(int documentoOrigenId, int documentoDestinoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return false;

        // Verifica relación directa (A→B) o inversa (B→A) para prevenir ciclos (R04)
        return await db.Set<ModificacionDocumento>()
            .AnyAsync(m =>
                (m.DocumentoOrigenId == documentoOrigenId && m.DocumentoDestinoId == documentoDestinoId) ||
                (m.DocumentoOrigenId == documentoDestinoId && m.DocumentoDestinoId == documentoOrigenId),
                cancellationToken);
    }

    public async Task<ModificacionDocumento> CreateAsync(ModificacionDocumento modificacion, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<ModificacionDocumento>().Add(modificacion);
        await db.SaveChangesAsync(cancellationToken);
        return modificacion;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<ModificacionDocumento>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return;

        db.Set<ModificacionDocumento>().Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
