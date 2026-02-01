namespace InventorySystem.Domain;

/// <summary>
/// Configuración de conexión persistida tras el setup inicial.
/// Contiene la información necesaria para conectarse con el usuario de aplicación
/// (no las credenciales SA, que solo se usan durante el setup).
/// </summary>
public sealed record ConnectionSettings
{
    public DatabaseProvider Provider { get; init; }

    /// <summary>
    /// Cadena de conexión para SQL Server (con usuario de aplicación creado durante el setup).
    /// </summary>
    public string? SqlServerConnectionString { get; init; }

    /// <summary>
    /// Ruta del archivo .db para SQLite.
    /// </summary>
    public string? SqliteDatabasePath { get; init; }

    public string GetConnectionString()
    {
        return Provider switch
        {
            DatabaseProvider.SqlServer => SqlServerConnectionString ?? throw new InvalidOperationException("Connection string not configured for SQL Server."),
            DatabaseProvider.Sqlite => $"Data Source={SqliteDatabasePath ?? throw new InvalidOperationException("Database path not configured for SQLite.")}",
            _ => throw new ArgumentOutOfRangeException(nameof(Provider))
        };
    }

    public static ConnectionSettings ForSqlServer(string connectionString) => new()
    {
        Provider = DatabaseProvider.SqlServer,
        SqlServerConnectionString = connectionString
    };

    public static ConnectionSettings ForSqlite(string databasePath) => new()
    {
        Provider = DatabaseProvider.Sqlite,
        SqliteDatabasePath = databasePath
    };
}
