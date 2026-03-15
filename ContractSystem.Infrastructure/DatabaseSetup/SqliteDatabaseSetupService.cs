using ContractSystem.Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContractSystem.Infrastructure.DatabaseSetup;

internal sealed class SqliteDatabaseSetupService : IDatabaseSetupService
{
    private readonly ILogger<SqliteDatabaseSetupService> _logger;

    public SqliteDatabaseSetupService(ILogger<SqliteDatabaseSetupService> logger)
    {
        _logger = logger;
    }

    public Task<DatabaseSetupResult> TestSqlServerConnectionAsync(SqlServerSetupRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DatabaseSetupResult.Failure("Este servicio solo maneja SQLite. Use la implementación de SQL Server."));
    }

    public Task<DatabaseSetupResult> SetupSqlServerAsync(SqlServerSetupRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DatabaseSetupResult.Failure("Este servicio solo maneja SQLite. Use la implementación de SQL Server."));
    }

    public async Task<DatabaseSetupResult> SetupSqliteAsync(SqliteSetupRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var path = request.DatabasePath;

            // Asegurar extensión .db si no la tiene
            if (!path.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                path += ".db";

            var directory = Path.GetDirectoryName(path);
            if (request.CreateDirectoryIfNotExists && !string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                return DatabaseSetupResult.Failure($"El directorio '{directory}' no existe y no se pudo crear.");

            var connectionString = $"Data Source={path}";

            // Crear/abrir la base de datos y aplicar schema inicial
            await using (var conn = new SqliteConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken);
            }

            // Usar EF para crear el schema (si hay DbContext configurado)
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connectionString)
                .Options;

            await using (var context = new ApplicationDbContext(options))
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
            }

            var settings = ConnectionSettings.ForSqlite(path);
            _logger.LogInformation("Setup SQLite completado: {Path}", path);
            return DatabaseSetupResult.Success(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en setup SQLite");
            return DatabaseSetupResult.Failure(ex.Message);
        }
    }
}
