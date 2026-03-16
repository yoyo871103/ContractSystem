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

    /// <summary>
    /// ViewModel del sub-tab "Terceros (Clientes/Proveedores)".
    /// </summary>
    public TerceroViewModel Terceros { get; }

    /// <summary>
    /// ViewModel del sub-tab "Productos/Servicios".
    /// </summary>
    public ProductoServicioViewModel ProductosServicios { get; }

    /// <summary>
    /// ViewModel del sub-tab "Plantillas de Documentos".
    /// </summary>
    public PlantillaDocumentoViewModel PlantillasDocumento { get; }

    public NomencladoresViewModel(IServiceProvider services)
    {
        UnidadMedida = services.GetRequiredService<UnidadMedidaViewModel>();
        Terceros = services.GetRequiredService<TerceroViewModel>();
        ProductosServicios = services.GetRequiredService<ProductoServicioViewModel>();
        PlantillasDocumento = services.GetRequiredService<PlantillaDocumentoViewModel>();
    }
}
