using ContractSystem.Application.Common.Models;
using ContractSystem.Application.Database;
using ContractSystem.Application.Nomencladores;
using ContractSystem.Domain.DatosLegales;
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
        {
            query = query
                .Include(e => e.Contactos)
                .Include(e => e.Representantes)
                .Include(e => e.CuentasBancarias)
                .AsSplitQuery();
        }

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

    public async Task<PagedList<Tercero>> GetPagedAsync(int page, int pageSize, bool includeDeleted = false, TipoTercero? tipo = null, string? searchText = null, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return PagedList<Tercero>.Empty(pageSize);

        var query = db.Set<Tercero>().AsQueryable();
        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (tipo.HasValue)
            query = query.Where(e => e.Tipo == tipo.Value || e.Tipo == TipoTercero.Ambos);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            query = query.Where(e =>
                e.Nombre.ToLower().Contains(term) ||
                (e.Codigo != null && e.Codigo.ToLower().Contains(term)) ||
                e.NifCif.ToLower().Contains(term));
        }

        var totalRows = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalRows / pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var items = await query
            .OrderBy(e => e.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Tercero>(items, page, totalPages, pageSize, totalRows, items.Count);
    }

    public async Task<IReadOnlyList<Tercero>> SearchAsync(string texto, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<Tercero>();

        var query = db.Set<Tercero>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var term = texto.Trim().ToLower();
            query = query.Where(e =>
                e.Nombre.ToLower().Contains(term) ||
                (e.Codigo != null && e.Codigo.ToLower().Contains(term)) ||
                e.NifCif.ToLower().Contains(term));
        }

        return await query
            .OrderBy(e => e.Nombre)
            .Take(maxResults)
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

    public async Task UpdateAsync(Tercero tercero, bool actualizarDatosLegales = false, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        // Si no se actualizan los datos legales, no tocar sus filas (podrían venir sin cargar).
        if (!actualizarDatosLegales)
        {
            tercero.Representantes = new List<RepresentanteLegal>();
            tercero.CuentasBancarias = new List<CuentaBancaria>();
        }

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

        if (actualizarDatosLegales)
        {
            // Las colecciones traen el estado completo: se borran las filas que ya no están.
            var representantesActuales = tercero.Representantes.Where(r => r.Id > 0).Select(r => r.Id).ToHashSet();
            var representantesABorrar = await db.Set<RepresentanteLegal>()
                .Where(r => r.TerceroId == tercero.Id && !representantesActuales.Contains(r.Id))
                .ToListAsync(cancellationToken);
            db.Set<RepresentanteLegal>().RemoveRange(representantesABorrar);

            var cuentasActuales = tercero.CuentasBancarias.Where(c => c.Id > 0).Select(c => c.Id).ToHashSet();
            var cuentasABorrar = await db.Set<CuentaBancaria>()
                .Where(c => c.TerceroId == tercero.Id && !cuentasActuales.Contains(c.Id))
                .ToListAsync(cancellationToken);
            db.Set<CuentaBancaria>().RemoveRange(cuentasABorrar);
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
