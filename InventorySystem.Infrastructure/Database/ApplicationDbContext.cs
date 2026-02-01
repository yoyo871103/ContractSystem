using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Database;

/// <summary>
/// Contexto de Entity Framework Core para la aplicación.
/// Soporta SQL Server y SQLite según la configuración.
/// Se irán añadiendo DbSets conforme se definan entidades en Domain.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configuraciones de entidades y migraciones
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
