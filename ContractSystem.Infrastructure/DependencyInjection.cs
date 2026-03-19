using ContractSystem.Application.Business;
using ContractSystem.Application.Auth;
using ContractSystem.Infrastructure.Business;
using ContractSystem.Application.Configuration;
using ContractSystem.Application.Nomencladores;
using ContractSystem.Application.Contratos;
using ContractSystem.Infrastructure.Auth;
using ContractSystem.Infrastructure.Configuration;
using ContractSystem.Infrastructure.Nomencladores;
using ContractSystem.Infrastructure.Contratos;
using ContractSystem.Infrastructure.Database;
using ContractSystem.Infrastructure.DatabaseSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ContractSystem.Infrastructure;

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

        // Autenticación y autorización
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddTransient<IUsuarioStore, UsuarioStore>();
        services.AddTransient<IRolStore, RolStore>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddSingleton<IAuthContext, AuthContext>();
        services.AddTransient<ISqlServerAdminAuthService, SqlServerAdminAuthService>();
        services.AddTransient<ISeedDataService, SeedDataService>();
        services.AddTransient<IBusinessInfoStore, BusinessInfoStore>();
        services.AddTransient<IUnidadMedidaStore, UnidadMedidaStore>();
        services.AddTransient<ITerceroStore, TerceroStore>();
        services.AddTransient<IProductoServicioStore, ProductoServicioStore>();
        services.AddTransient<IPlantillaDocumentoStore, PlantillaDocumentoStore>();

        // Contratos
        services.AddTransient<IContratoStore, ContratoStore>();
        services.AddTransient<IModificacionDocumentoStore, ModificacionDocumentoStore>();
        services.AddTransient<IContratoValidationService, ContratoValidationService>();
        services.AddTransient<IConfiguracionNumeracionStore, ConfiguracionNumeracionStore>();
        services.AddTransient<IDocumentoNumeracionService, DocumentoNumeracionService>();
        services.AddTransient<IAnexoStore, AnexoStore>();
        services.AddTransient<ILineaDetalleStore, LineaDetalleStore>();
        services.AddTransient<IDocumentoAdjuntoStore, DocumentoAdjuntoStore>();
        services.AddTransient<IHistorialCambioStore, HistorialCambioStore>();
        services.AddTransient<IFacturaStore, FacturaStore>();

        return services;
    }
}
