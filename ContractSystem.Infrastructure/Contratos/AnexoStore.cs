using ContractSystem.Application.Contratos;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class AnexoStore : IAnexoStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public AnexoStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<Anexo>> GetByContratoAsync(int contratoId, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<Anexo>();

        return await db.Set<Anexo>()
            .Where(e => e.ContratoId == contratoId)
            .OrderBy(e => e.Orden)
            .ToListAsync(cancellationToken);
    }

    public async Task<Anexo?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<Anexo>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Anexo> CreateAsync(Anexo anexo, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        db.Set<Anexo>().Add(anexo);
        await db.SaveChangesAsync(cancellationToken);
        return anexo;
    }

    public async Task UpdateAsync(Anexo anexo, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        db.Set<Anexo>().Update(anexo);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var entity = await db.Set<Anexo>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null) return;

        db.Set<Anexo>().Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }
}
