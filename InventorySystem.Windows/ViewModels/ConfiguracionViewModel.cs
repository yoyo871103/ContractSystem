using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace InventorySystem.Windows.ViewModels;

/// <summary>
/// ViewModel de la pantalla Configuración. Contiene pestañas; por ahora solo "Gestión de usuarios".
/// Accesible por el usuario administrador (y por admin SQL solo con la pestaña de usuarios).
/// </summary>
public sealed partial class ConfiguracionViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedTabIndex;

    /// <summary>
    /// ViewModel del tab "Gestión de usuarios" (listado, CRUD y asignación de roles).
    /// </summary>
    public GestionUsuariosViewModel GestionUsuarios { get; }

    /// <summary>
    /// ViewModel del tab "Datos del Negocio".
    /// </summary>
    public BusinessInfoViewModel BusinessInfo { get; }

    public ConfiguracionViewModel(IServiceProvider services)
    {
        GestionUsuarios = services.GetRequiredService<GestionUsuariosViewModel>();
        BusinessInfo = services.GetRequiredService<BusinessInfoViewModel>();
    }
}
