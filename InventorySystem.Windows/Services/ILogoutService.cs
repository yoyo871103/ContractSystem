using System.Windows;

namespace InventorySystem.Windows.Services;

/// <summary>
/// Servicio para cerrar sesión: oculta la ventana principal, muestra login y vuelve a mostrar la ventana si el usuario se autentica de nuevo.
/// </summary>
public interface ILogoutService
{
    /// <summary>
    /// Establece la ventana principal (debe llamarse una vez tras mostrar MainWindow).
    /// </summary>
    void SetMainWindow(Window? mainWindow);

    /// <summary>
    /// Cierra sesión: oculta la ventana principal, muestra LoginWindow; si el usuario inicia sesión de nuevo, muestra la ventana principal.
    /// </summary>
    void RequestLogout();
}
