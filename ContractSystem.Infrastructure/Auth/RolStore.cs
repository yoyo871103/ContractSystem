using ContractSystem.Application.Auth;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Auth;

internal sealed class RolStore : IRolStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public RolStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<RolItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return Array.Empty<RolItem>();

        return await db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .Select(r => new RolItem(r.Id, r.Nombre))
            .ToListAsync(cancellationToken);
    }
}
