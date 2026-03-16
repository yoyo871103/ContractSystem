using ContractSystem.Application.Common.Models;
using ContractSystem.Application.Contratos;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Contratos;
using ContractSystem.Infrastructure.Pagination;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class ContratoStore : IContratoStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public ContratoStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Contrato?> GetByIdAsync(int id, bool includeRelaciones = false, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        var query = db.Set<Contrato>().AsQueryable();

        if (includeRelaciones)
        {
            query = query
                .Include(e => e.ContratoPadre)
                .Include(e => e.Hijos)
                .Include(e => e.MiEmpresa)
                .Include(e => e.Tercero)
                .Include(e => e.ModificaA).ThenInclude(m => m.DocumentoDestino)
                .Include(e => e.ModificadoPor).ThenInclude(m => m.DocumentoOrigen);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Contrato>> GetAllAsync(
        bool includeDeleted = false,
        TipoDocumentoContrato? tipo = null,
        EstadoContrato? estado = null,
        RolContrato? rol = null,
        int? terceroId = null,
        DateTime? fechaFirmaDesde = null,
        DateTime? fechaFirmaHasta = null,
        string? textoBusqueda = null,
        string? textoTercero = null,
        CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<Contrato>();

        var query = db.Set<Contrato>()
            .Include(e => e.Tercero)
            .Include(e => e.MiEmpresa)
            .AsQueryable();

        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (tipo.HasValue)
            query = query.Where(e => e.TipoDocumento == tipo.Value);

        if (estado.HasValue)
            query = query.Where(e => e.Estado == estado.Value);

        if (rol.HasValue)
            query = query.Where(e => e.Rol == rol.Value);

        if (terceroId.HasValue)
            query = query.Where(e => e.TerceroId == terceroId.Value);

        if (fechaFirmaDesde.HasValue)
            query = query.Where(e => e.FechaFirma >= fechaFirmaDesde.Value);

        if (fechaFirmaHasta.HasValue)
            query = query.Where(e => e.FechaFirma <= fechaFirmaHasta.Value);

        if (!string.IsNullOrWhiteSpace(textoBusqueda))
        {
            var texto = textoBusqueda.Trim().ToLower();
            query = query.Where(e =>
                e.Numero.ToLower().Contains(texto) ||
                e.Objeto.ToLower().Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(textoTercero))
        {
            var tt = textoTercero.Trim().ToLower();
            query = query.Where(e => e.Tercero != null &&
                (e.Tercero.Nombre.ToLower().Contains(tt) ||
                 (e.Tercero.Codigo != null && e.Tercero.Codigo.ToLower().Contains(tt)) ||
                 e.Tercero.NifCif.ToLower().Contains(tt)));
        }

        return await query
            .OrderByDescending(e => e.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedList<Contrato>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        bool includeDeleted = false,
        TipoDocumentoContrato? tipo = null,
        EstadoContrato? estado = null,
        RolContrato? rol = null,
        int? terceroId = null,
        string? textoBusqueda = null,
        string? textoTercero = null,
        CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return PagedList<Contrato>.Empty(pageSize);

        var query = db.Set<Contrato>()
            .Include(e => e.Tercero)
            .Include(e => e.MiEmpresa)
            .AsQueryable();

        if (includeDeleted)
            query = query.IgnoreQueryFilters();

        if (tipo.HasValue)
            query = query.Where(e => e.TipoDocumento == tipo.Value);

        if (estado.HasValue)
            query = query.Where(e => e.Estado == estado.Value);

        if (rol.HasValue)
            query = query.Where(e => e.Rol == rol.Value);

        if (terceroId.HasValue)
            query = query.Where(e => e.TerceroId == terceroId.Value);

        if (!string.IsNullOrWhiteSpace(textoBusqueda))
        {
            var texto = textoBusqueda.Trim().ToLower();
            query = query.Where(e =>
                e.Numero.ToLower().Contains(texto) ||
                e.Objeto.ToLower().Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(textoTercero))
        {
            var tt = textoTercero.Trim().ToLower();
            query = query.Where(e => e.Tercero != null &&
                (e.Tercero.Nombre.ToLower().Contains(tt) ||
                 (e.Tercero.Codigo != null && e.Tercero.Codigo.ToLower().Contains(tt)) ||
                 e.Tercero.NifCif.ToLower().Contains(tt)));
        }

        return await query
            .OrderByDescending(e => e.FechaCreacion)
            .ToPagedListAsync(page, pageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<Contrato>> GetContratosMarcoAsync(CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<Contrato>();

        return await db.Set<Contrato>()
            .Where(e => e.TipoDocumento == TipoDocumentoContrato.Marco)
            .Where(e => e.Estado != EstadoContrato.Rescindido)
            .OrderBy(e => e.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Contrato>> GetHijosAsync(int contratoPadreId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<Contrato>();

        return await db.Set<Contrato>()
            .Where(e => e.ContratoPadreId == contratoPadreId)
            .OrderBy(e => e.FechaFirma)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteNumeroAsync(string numero, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return false;

        var query = db.Set<Contrato>()
            .IgnoreQueryFilters()
            .Where(e => e.Numero == numero);

        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Contrato> CreateAsync(Contrato contrato, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<Contrato>().Add(contrato);
        await db.SaveChangesAsync(cancellationToken);
        return contrato;
    }

    public async Task UpdateAsync(Contrato contrato, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        db.Set<Contrato>().Update(contrato);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<Contrato>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
            return;

        entity.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
}
