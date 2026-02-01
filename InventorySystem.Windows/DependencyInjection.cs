using InventorySystem.Windows.Services;
using InventorySystem.Windows.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Windows;

/// <summary>
/// Registro de servicios y ViewModels de la capa de presentación (MVVM).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddWindows(this IServiceCollection services)
    {
        // Servicios de UI
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IViewDialogService, ViewDialogService>();
        services.AddSingleton<ILogoutService, LogoutService>();

        // ViewModels (Transient para que cada navegación obtenga una nueva instancia)
        services.AddTransient<MainViewModel>();
        services.AddTransient<InicioViewModel>();
        services.AddTransient<VentasViewModel>();
        services.AddTransient<GestionUsuariosViewModel>();
        services.AddTransient<ConfiguracionViewModel>();

        return services;
    }
}
