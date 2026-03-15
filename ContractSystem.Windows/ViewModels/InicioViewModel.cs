using CommunityToolkit.Mvvm.ComponentModel;

namespace ContractSystem.Windows.ViewModels;

/// <summary>
/// ViewModel de la pantalla de bienvenida/inicio.
/// </summary>
public sealed partial class InicioViewModel : ObservableObject
{
    [ObservableProperty]
    private string _mensajeBienvenida = "Bienvenido al Sistema de Contratos";

    [ObservableProperty]
    private string _mensajeSecundario = "Selecciona una opción del menú para comenzar";
}
