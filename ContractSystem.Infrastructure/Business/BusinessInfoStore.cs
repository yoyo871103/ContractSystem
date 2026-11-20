using ContractSystem.Application.Business;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Business;
using ContractSystem.Domain.DatosLegales;
using ContractSystem.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Business;

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

        return await db.Set<BusinessInfo>()
            .Include(e => e.Representantes)
            .Include(e => e.CuentasBancarias)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateAsync(BusinessInfo businessInfo, bool actualizarDatosLegales = false, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        var existing = await db.Set<BusinessInfo>()
            .Include(e => e.Representantes)
            .Include(e => e.CuentasBancarias)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

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

            if (actualizarDatosLegales)
            {
                existing.CodigoReeup = businessInfo.CodigoReeup;
                existing.RegistroMercantil = businessInfo.RegistroMercantil;
                existing.DocumentoConstitucion = businessInfo.DocumentoConstitucion;

                // Reemplazo completo: se borran las filas actuales (quitar de la colección solo
                // anularía la FK, que es opcional, y dejaría filas huérfanas) y se insertan copias nuevas.
                db.Set<RepresentanteLegal>().RemoveRange(existing.Representantes);
                db.Set<CuentaBancaria>().RemoveRange(existing.CuentasBancarias);

                foreach (var r in businessInfo.Representantes)
                {
                    db.Set<RepresentanteLegal>().Add(new RepresentanteLegal
                    {
                        BusinessInfoId = existing.Id,
                        Nombre = r.Nombre,
                        Cargo = r.Cargo,
                        CarneIdentidad = r.CarneIdentidad,
                        DocumentoNombramiento = r.DocumentoNombramiento,
                        FechaNombramiento = r.FechaNombramiento,
                        EsPredeterminado = r.EsPredeterminado
                    });
                }

                foreach (var c in businessInfo.CuentasBancarias)
                {
                    db.Set<CuentaBancaria>().Add(new CuentaBancaria
                    {
                        BusinessInfoId = existing.Id,
                        Numero = c.Numero,
                        Moneda = c.Moneda,
                        Banco = c.Banco,
                        Sucursal = c.Sucursal,
                        DireccionSucursal = c.DireccionSucursal,
                        Titular = c.Titular,
                        EsPredeterminada = c.EsPredeterminada
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
