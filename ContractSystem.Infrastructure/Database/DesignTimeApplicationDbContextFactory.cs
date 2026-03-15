using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ContractSystem.Infrastructure.Database;

/// <summary>
/// Factory de diseño para que las herramientas de EF Core (migraciones) puedan crear el DbContext
/// sin depender de la configuración de conexión en runtime.
/// </summary>
internal sealed class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite("Data Source=DesignTime.db");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
