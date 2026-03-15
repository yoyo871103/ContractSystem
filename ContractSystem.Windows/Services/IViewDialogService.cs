namespace ContractSystem.Windows.Services;

/// <summary>
/// Servicio para mostrar vistas en ventanas de diálogo modales.
/// </summary>
public interface IViewDialogService
{
    /// <summary>
    /// Muestra el contenido en una ventana de diálogo modal.
    /// </summary>
    /// <param name="content">Contenido a mostrar (p. ej. UserControl con DataContext ya asignado).</param>
    /// <param name="title">Título de la ventana.</param>
    void ShowInDialog(object content, string title);
}
