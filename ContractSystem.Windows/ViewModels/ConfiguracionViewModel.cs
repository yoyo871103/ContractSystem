using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel de la pantalla Configuración. Contiene pestañas; por ahora solo "Gestión de usuarios".
/// Accesible por el usuario administrador (y por admin SQL solo con la pestaña de usuarios).
/// </summary>
public sealed partial class ConfiguracionViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedTabIndex;

    public GestionUsuariosViewModel GestionUsuarios { get; }
    public GestionRolesViewModel GestionRoles { get; }
    public BusinessInfoViewModel BusinessInfo { get; }
    public NomencladoresViewModel Nomencladores { get; }
    public NumeracionConfigViewModel NumeracionConfig { get; }

    public ConfiguracionViewModel(IServiceProvider services)
    {
        GestionUsuarios = services.GetRequiredService<GestionUsuariosViewModel>();
        GestionRoles = services.GetRequiredService<GestionRolesViewModel>();
        BusinessInfo = services.GetRequiredService<BusinessInfoViewModel>();
        Nomencladores = services.GetRequiredService<NomencladoresViewModel>();
        NumeracionConfig = services.GetRequiredService<NumeracionConfigViewModel>();
    }
}
