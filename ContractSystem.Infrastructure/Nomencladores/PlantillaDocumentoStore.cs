using ContractSystem.Application.Database;
using ContractSystem.Application.Nomencladores;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Nomencladores;

public sealed class PlantillaDocumentoStore : IPlantillaDocumentoStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public PlantillaDocumentoStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<PlantillaDocumento?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<PlantillaDocumento>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PlantillaDocumento>> GetAllAsync(TipoDocumentoPlantilla? tipo = null, RolPlantilla? rol = null, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<PlantillaDocumento>();

        var query = db.Set<PlantillaDocumento>().AsQueryable();

        if (tipo.HasValue)
            query = query.Where(e => e.TipoDocumento == tipo.Value);

        if (rol.HasValue)
            query = query.Where(e => e.Rol == rol.Value);

        // No cargar el archivo en el listado (solo metadatos)
        return await query
            .Select(e => new PlantillaDocumento
            {
                Id = e.Id,
                Nombre = e.Nombre,
                TipoDocumento = e.TipoDocumento,
                Rol = e.Rol,
                NombreArchivo = e.NombreArchivo,
                FechaCreacion = e.FechaCreacion,
                RevisadoPorLegal = e.RevisadoPorLegal,
                Archivo = Array.Empty<byte>() // No cargar blob en listado
            })
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlantillaDocumento> CreateAsync(PlantillaDocumento entity, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<PlantillaDocumento>().Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(PlantillaDocumento entity, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        db.Set<PlantillaDocumento>().Update(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<PlantillaDocumento>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return;

        db.Set<PlantillaDocumento>().Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
