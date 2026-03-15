using Microsoft.Extensions.Logging;

namespace ContractSystem.Infrastructure.DatabaseSetup;

/// <summary>
/// Servicio de setup que delega en las implementaciones específicas de SQL Server y SQLite.
/// La app Windows usará ambos métodos; la app MAUI solo SetupSqlite.
/// </summary>
internal sealed class CompositeDatabaseSetupService : IDatabaseSetupService
{
    private readonly SqlServerDatabaseSetupService _sqlServer;
    private readonly SqliteDatabaseSetupService _sqlite;

    public CompositeDatabaseSetupService(ILoggerFactory loggerFactory)
    {
        _sqlServer = new SqlServerDatabaseSetupService(loggerFactory.CreateLogger<SqlServerDatabaseSetupService>());
        _sqlite = new SqliteDatabaseSetupService(loggerFactory.CreateLogger<SqliteDatabaseSetupService>());
    }

    public Task<DatabaseSetupResult> TestSqlServerConnectionAsync(SqlServerSetupRequest request, CancellationToken cancellationToken = default)
        => _sqlServer.TestSqlServerConnectionAsync(request, cancellationToken);

    public Task<DatabaseSetupResult> SetupSqlServerAsync(SqlServerSetupRequest request, CancellationToken cancellationToken = default)
        => _sqlServer.SetupSqlServerAsync(request, cancellationToken);

    public Task<DatabaseSetupResult> SetupSqliteAsync(SqliteSetupRequest request, CancellationToken cancellationToken = default)
        => _sqlite.SetupSqliteAsync(request, cancellationToken);
}
