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

    public async Task<IReadOnlyList<DocumentoAdjunto>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<DocumentoAdjunto>();

        // No cargar Contenido en el listado (proyección sin blob)
        return await db.Set<DocumentoAdjunto>()
            .Where(d => d.ContratoId == contratoId)
            .Select(d => new DocumentoAdjunto
            {
                Id = d.Id,
                NombreArchivo = d.NombreArchivo,
                Extension = d.Extension,
                Objetivo = d.Objetivo,
                TamanioBytes = d.TamanioBytes,
                FechaCarga = d.FechaCarga,
                UsuarioCargaId = d.UsuarioCargaId,
                ContratoId = d.ContratoId
            })
            .OrderBy(d => d.FechaCarga)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentoAdjunto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<DocumentoAdjunto>()
            .Select(d => new DocumentoAdjunto
            {
                Id = d.Id,
                NombreArchivo = d.NombreArchivo,
                Extension = d.Extension,
                Objetivo = d.Objetivo,
                TamanioBytes = d.TamanioBytes,
                FechaCarga = d.FechaCarga,
                UsuarioCargaId = d.UsuarioCargaId,
                ContratoId = d.ContratoId
            })
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
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
