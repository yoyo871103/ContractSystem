namespace InventorySystem.Windows.Services;

/// <summary>
/// Servicio de navegación entre vistas. Permite cambiar el ViewModel actual en el área de contenido.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// ViewModel actualmente mostrado.
    /// </summary>
    object? CurrentViewModel { get; }

    /// <summary>
    /// Se dispara cuando cambia CurrentViewModel.
    /// </summary>
    event EventHandler? Navigated;

    /// <summary>
    /// Navega al ViewModel especificado.
    /// </summary>
    void NavigateTo(object? viewModel);

    /// <summary>
    /// Navega a la pantalla de bienvenida (Inicio).
    /// </summary>
    void NavigateToInicio();
}
