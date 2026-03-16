using ContractSystem.Application.Auth;
using Microsoft.EntityFrameworkCore;

namespace ContractSystem.Infrastructure.Database;

internal sealed class ApplicationDbContextFactory : IApplicationDbContextFactory
{
    private readonly IConnectionConfigurationStore _configStore;
    private readonly IAuthContext _authContext;
    private static bool _migrated;
    private static readonly object _migrateLock = new();
    private static readonly string _logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ContractSystem", "migration.log");

    public ApplicationDbContextFactory(IConnectionConfigurationStore configStore, IAuthContext authContext)
    {
        _configStore = configStore;
        _authContext = authContext;
    }

    public IApplicationDbContext CreateDbContext()
    {
        var settings = _configStore.GetSettings()
            ?? throw new InvalidOperationException(
                "No hay conexión configurada. Complete el asistente de configuración de base de datos.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = settings.GetConnectionString();

        _ = settings.Provider switch
        {
            DatabaseProvider.SqlServer => optionsBuilder.UseSqlServer(connectionString),
            DatabaseProvider.Sqlite => optionsBuilder.UseSqlite(connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(settings.Provider))
        };

        var context = new ApplicationDbContext(optionsBuilder.Options, _authContext);

        if (!_migrated)
        {
            lock (_migrateLock)
            {
                if (!_migrated)
                {
                    try
                    {
                        ApplyMigrationsWithLog(context);
                        _migrated = true;
                    }
                    catch
                    {
                        // _migrated stays false so next call retries
                    }
                }
            }
        }

        return context;
    }

    private static void ApplyMigrationsWithLog(ApplicationDbContext context)
    {
        var lines = new List<string> { $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] === Inicio migración ===" };
        try
        {
            var pending = context.Database.GetPendingMigrations().ToList();
            lines.Add($"Migraciones pendientes ({pending.Count}):");
            foreach (var m in pending) lines.Add($"  - {m}");

            if (pending.Count > 0)
            {
                lines.Add("Ejecutando Migrate()...");
                context.Database.Migrate();
                lines.Add("Migrate() completado OK.");
            }
            else
            {
                lines.Add("No hay migraciones pendientes.");
            }
        }
        catch (Exception ex)
        {
            lines.Add($"ERROR: {ex.Message}");
            lines.Add($"INNER: {ex.InnerException?.Message}");
            try { File.AppendAllLines(_logPath, lines); } catch { }
            throw; // Re-throw so _migrated stays false and retries next time
        }

        try { File.AppendAllLines(_logPath, lines); } catch { }
    }

    public static void ResetMigrationState() => _migrated = false;
}
