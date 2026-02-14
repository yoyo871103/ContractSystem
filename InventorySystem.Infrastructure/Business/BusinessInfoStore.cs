using InventorySystem.Application.Business;
using InventorySystem.Application.Database;
using InventorySystem.Domain.Business;
using InventorySystem.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = InventorySystem.Infrastructure.Database.ApplicationDbContext;

namespace InventorySystem.Infrastructure.Business;

public class BusinessInfoStore : IBusinessInfoStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public BusinessInfoStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<BusinessInfo?> GetAsync(CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<BusinessInfo>().FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(BusinessInfo businessInfo, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var existing = await db.Set<BusinessInfo>().FirstOrDefaultAsync(cancellationToken);
        if (existing == null)
        {
            db.Set<BusinessInfo>().Add(businessInfo);
        }
        else
        {
            existing.Nombre = businessInfo.Nombre;
            existing.Logo = businessInfo.Logo;
            existing.Nit = businessInfo.Nit;
            existing.Direccion = businessInfo.Direccion;
            existing.Telefono = businessInfo.Telefono;
            existing.Email = businessInfo.Email;
            existing.Eslogan = businessInfo.Eslogan;
            existing.NombreDueno = businessInfo.NombreDueno;
        }
        
        await db.SaveChangesAsync(cancellationToken);
    }
}
