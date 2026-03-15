namespace ContractSystem.Application.DatabaseSetup;

/// <summary>
/// Servicio que realiza el setup inicial de la base de datos.
/// - SQL Server: valida SA, crea BD si no existe, crea usuario de aplicación.
/// - SQLite: crea el archivo .db y la estructura si no existe.
/// La app MAUI solo usará SetupSqlite.
/// </summary>
public interface IDatabaseSetupService
{
    /// <summary>
    /// Prueba la conexión con SA sin modificar nada.
    /// </summary>
    Task<DatabaseSetupResult> TestSqlServerConnectionAsync(SqlServerSetupRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta el setup completo de SQL Server: crea BD si corresponde, usuario de aplicación,
    /// y devuelve ConnectionSettings para guardar.
    /// </summary>
    Task<DatabaseSetupResult> SetupSqlServerAsync(SqlServerSetupRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta el setup de SQLite: crea directorio si corresponde, archivo .db y estructura inicial.
    /// </summary>
    Task<DatabaseSetupResult> SetupSqliteAsync(SqliteSetupRequest request, CancellationToken cancellationToken = default);
}
