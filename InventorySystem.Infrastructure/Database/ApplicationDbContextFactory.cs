using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Infrastructure.Database;

internal sealed class ApplicationDbContextFactory : IApplicationDbContextFactory
{
    private readonly IConnectionConfigurationStore _configStore;

    public ApplicationDbContextFactory(IConnectionConfigurationStore configStore)
    {
        _configStore = configStore;
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

        var context = new ApplicationDbContext(optionsBuilder.Options);
        context.Database.Migrate();
        return context;
    }
}
