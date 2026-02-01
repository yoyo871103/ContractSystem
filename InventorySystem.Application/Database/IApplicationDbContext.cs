namespace InventorySystem.Application.Database;

/// <summary>
/// Abstracción del contexto de base de datos de la aplicación.
/// Permite acceder a la BD sin depender del proveedor concreto (SQL Server o SQLite).
/// </summary>
public interface IApplicationDbContext : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
