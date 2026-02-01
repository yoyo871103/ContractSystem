using InventorySystem.Application.Configuration;
using InventorySystem.Infrastructure.Configuration;
using InventorySystem.Infrastructure.Database;
using InventorySystem.Infrastructure.DatabaseSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InventorySystem.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de infraestructura.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configureOptions">Opcional: configurar DataDirectory para MAUI u otras plataformas.</param>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<InfrastructureOptions>? configureOptions = null)
    {
        services.AddApplication();

        services.AddLogging(builder =>
        {
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddOptions<InfrastructureOptions>()
            .Configure(options =>
            {
                configureOptions?.Invoke(options);
            });

        services.AddSingleton<IConnectionConfigurationStore, FileConnectionConfigurationStore>();
        services.AddSingleton<ISqlServerConnectionService, SqlServerConnectionService>();
        services.AddSingleton<IDatabaseSetupService, CompositeDatabaseSetupService>();
        services.AddTransient<IApplicationDbContextFactory, ApplicationDbContextFactory>();

        return services;
    }
}
