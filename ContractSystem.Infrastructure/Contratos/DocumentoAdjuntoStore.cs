using System.Linq.Expressions;
using ContractSystem.Application.Contratos;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class DocumentoAdjuntoStore : IDocumentoAdjuntoStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public DocumentoAdjuntoStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // Proyección sin Contenido (blob), expresable en árbol de expresión EF.
    private static readonly Expression<Func<DocumentoAdjunto, DocumentoAdjunto>> SinContenido =
        d => new DocumentoAdjunto
        {
            Id = d.Id,
            NombreArchivo = d.NombreArchivo,
            Extension = d.Extension,
            Objetivo = d.Objetivo,
            TamanioBytes = d.TamanioBytes,
            FechaCarga = d.FechaCarga,
            UsuarioCargaId = d.UsuarioCargaId,
            ContratoId = d.ContratoId,
            FacturaId = d.FacturaId,
            AnexoId = d.AnexoId
        };

    public async Task<IReadOnlyList<DocumentoAdjunto>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<DocumentoAdjunto>();

        return await db.Set<DocumentoAdjunto>()
            .Where(d => d.ContratoId == contratoId && d.FacturaId == null && d.AnexoId == null)
            .Select(SinContenido)
            .OrderBy(d => d.FechaCarga)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentoAdjunto>> GetByFacturaAsync(int facturaId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<DocumentoAdjunto>();

        return await db.Set<DocumentoAdjunto>()
            .Where(d => d.FacturaId == facturaId)
            .Select(SinContenido)
            .OrderBy(d => d.FechaCarga)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentoAdjunto>> GetBySuplementosDeContratoAsync(int contratoPadreId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<DocumentoAdjunto>();

        // Adjuntos cuyo ContratoId pertenece a un suplemento (contrato hijo) del contrato indicado.
        return await db.Set<DocumentoAdjunto>()
            .Where(d => d.FacturaId == null && d.AnexoId == null
                        && db.Contratos.Any(c => c.Id == d.ContratoId && c.ContratoPadreId == contratoPadreId))
            .Select(SinContenido)
            .OrderBy(d => d.FechaCarga)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentoAdjunto>> GetByFacturasDeContratoAsync(int contratoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<DocumentoAdjunto>();

        return await db.Set<DocumentoAdjunto>()
            .Where(d => d.FacturaId != null
                        && db.Facturas.Any(f => f.Id == d.FacturaId && f.ContratoId == contratoId))
            .Select(SinContenido)
            .OrderBy(d => d.FechaCarga)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentoAdjunto>> GetByAnexoAsync(int anexoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<DocumentoAdjunto>();

        return await db.Set<DocumentoAdjunto>()
            .Where(d => d.AnexoId == anexoId)
            .Select(SinContenido)
            .OrderBy(d => d.FechaCarga)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentoAdjunto>> GetByContratosAsync(IReadOnlyCollection<int> contratoIds, CancellationToken cancellationToken = default)
    {
        if (contratoIds.Count == 0) return Array.Empty<DocumentoAdjunto>();

        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<DocumentoAdjunto>();

        var ids = contratoIds.Distinct().ToList();
        return await db.Set<DocumentoAdjunto>()
            .Where(d => ids.Contains(d.ContratoId))
            .Select(SinContenido)
            .OrderBy(d => d.FechaCarga)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentoAdjunto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<DocumentoAdjunto>()
            .Where(d => d.Id == id)
            .Select(SinContenido)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DocumentoAdjunto?> GetByIdConContenidoAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<DocumentoAdjunto>()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task CreateAsync(DocumentoAdjunto adjunto, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<DocumentoAdjunto>().Add(adjunto);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DocumentoAdjunto adjunto, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<DocumentoAdjunto>().FirstOrDefaultAsync(d => d.Id == adjunto.Id, cancellationToken);
        if (entity is null) return;

        db.Set<DocumentoAdjunto>().Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
