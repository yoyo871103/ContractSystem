namespace ContractSystem.Application.Configuration;

/// <summary>
/// Servicio para probar conexión, listar bases de datos y crear bases de datos
/// usando credenciales de un usuario de SQL Server (no SA).
/// </summary>
public interface ISqlServerConnectionService
{
    /// <summary>
    /// Prueba la conexión al servidor con el usuario indicado (opcionalmente a una base de datos).
    /// </summary>
    Task<bool> TestConnectionAsync(string server, string user, string password, string? database, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la lista de bases de datos del servidor a las que el usuario tiene acceso.
    /// </summary>
    Task<IReadOnlyList<string>> ListDatabasesAsync(string server, string user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea una nueva base de datos en el servidor (el usuario debe tener permiso para crearla).
    /// </summary>
    Task<bool> CreateDatabaseAsync(string server, string user, string password, string databaseName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Construye la cadena de conexión para el usuario y base de datos indicados.
    /// </summary>
    string BuildConnectionString(string server, string user, string password, string database);
}
