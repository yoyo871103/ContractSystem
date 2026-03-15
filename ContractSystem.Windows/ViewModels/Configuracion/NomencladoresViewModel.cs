using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel del tab "Nomencladores" dentro de Configuración.
/// Contiene sub-pestañas para cada tipo de nomenclador (Unidades de medida, etc.).
/// </summary>
public sealed partial class NomencladoresViewModel : ObservableObject
{
    [ObservableProperty]
    private int _selectedTabIndex;

    /// <summary>
    /// ViewModel del sub-tab "Unidades de medida".
    /// </summary>
    public UnidadMedidaViewModel UnidadMedida { get; }

    public NomencladoresViewModel(IServiceProvider services)
    {
        UnidadMedida = services.GetRequiredService<UnidadMedidaViewModel>();
    }
}
