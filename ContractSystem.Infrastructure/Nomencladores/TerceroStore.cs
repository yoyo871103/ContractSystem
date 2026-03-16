using ContractSystem.Application.Database;
using ContractSystem.Application.Nomencladores;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Nomencladores;

public sealed class TerceroStore : ITerceroStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public TerceroStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Tercero?> GetByIdAsync(int id, bool includeContactos = false, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        var query = db.Set<Tercero>().AsQueryable();
        if (includeContactos)
            query = query.Include(e => e.Contactos);

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Tercero>> GetAllAsync(bool includeDeleted = false, TipoTercero? tipo = null, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<Tercero>();

        var query = db.Set<Tercero>().AsQueryable();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (tipo.HasValue)
            query = query.Where(e => e.Tipo == tipo.Value || e.Tipo == TipoTercero.Ambos);

        return await query
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tercero> CreateAsync(Tercero tercero, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<Tercero>().Add(tercero);
        await db.SaveChangesAsync(cancellationToken);
        return tercero;
    }

    public async Task UpdateAsync(Tercero tercero, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        // Adjuntar el tercero y sus contactos
        db.Set<Tercero>().Update(tercero);

        // Gestionar contactos eliminados: borrar los que ya no están en la colección
        var existingContactIds = await db.Set<ContactoTercero>()
            .Where(c => c.TerceroId == tercero.Id)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var currentContactIds = tercero.Contactos.Where(c => c.Id > 0).Select(c => c.Id).ToHashSet();
        var toDelete = existingContactIds.Where(id => !currentContactIds.Contains(id));

        foreach (var contactId in toDelete)
        {
            var contact = new ContactoTercero { Id = contactId };
            db.Set<ContactoTercero>().Attach(contact);
            db.Set<ContactoTercero>().Remove(contact);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<Tercero>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UndeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<Tercero>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return;

        entity.DeletedAt = null;
        await db.SaveChangesAsync(cancellationToken);
    }
}
