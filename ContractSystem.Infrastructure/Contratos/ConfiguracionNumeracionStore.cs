using ContractSystem.Application.Contratos;
using ContractSystem.Application.Database;
using ContractSystem.Domain.Contratos;
using Microsoft.EntityFrameworkCore;
using DatabaseApplicationDbContext = ContractSystem.Infrastructure.Database.ApplicationDbContext;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class ConfiguracionNumeracionStore : IConfiguracionNumeracionStore
{
    private readonly IApplicationDbContextFactory _contextFactory;

    public ConfiguracionNumeracionStore(IApplicationDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ConfiguracionNumeracion?> GetActivaAsync(CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<ConfiguracionNumeracion>()
            .FirstOrDefaultAsync(e => e.Activa, cancellationToken);
    }

    public async Task<ConfiguracionNumeracion?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return null;

        return await db.Set<ConfiguracionNumeracion>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<ConfiguracionNumeracion> CreateAsync(ConfiguracionNumeracion configuracion, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        // Desactivar cualquier configuración previa
        var activas = await db.Set<ConfiguracionNumeracion>()
            .Where(e => e.Activa)
            .ToListAsync(cancellationToken);
        foreach (var activa in activas)
            activa.Activa = false;

        configuracion.Activa = true;
        db.Set<ConfiguracionNumeracion>().Add(configuracion);
        await db.SaveChangesAsync(cancellationToken);
        return configuracion;
    }

    public async Task UpdateAsync(ConfiguracionNumeracion configuracion, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            return;

        db.Set<ConfiguracionNumeracion>().Update(configuracion);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ObtenerSiguienteNumeroAsync(int? anio, CancellationToken cancellationToken = default)
    {
        using var context = _contextFactory.CreateDbContext();
        if (context is not DatabaseApplicationDbContext db)
            throw new InvalidOperationException("No se pudo crear el contexto de base de datos.");

        var contador = await db.Set<ContadorNumeracion>()
            .FirstOrDefaultAsync(e => e.Anio == anio, cancellationToken);

        if (contador is null)
        {
            contador = new ContadorNumeracion { Anio = anio, UltimoNumero = 1 };
            db.Set<ContadorNumeracion>().Add(contador);
        }
        else
        {
            contador.UltimoNumero++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return contador.UltimoNumero;
    }
}
