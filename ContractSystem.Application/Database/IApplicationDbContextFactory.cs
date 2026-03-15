namespace ContractSystem.Application.Database;

/// <summary>
/// Factory que crea instancias de DbContext según la configuración de conexión actual.
/// Necesario porque la conexión se configura en runtime (tras el setup del usuario).
/// </summary>
public interface IApplicationDbContextFactory
{
    /// <summary>
    /// Crea un DbContext. Requiere que exista configuración guardada (HasConnectionConfigured == true).
    /// </summary>
    /// <exception cref="InvalidOperationException">Si no hay conexión configurada.</exception>
    IApplicationDbContext CreateDbContext();
}
