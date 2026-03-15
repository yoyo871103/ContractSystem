using System.Windows;
using ContractSystem.Windows.Shared.Dialogs;

namespace ContractSystem.Windows.Services;

/// <summary>
/// Implementación que usa ViewDialogWindow para mostrar contenido en modal.
/// Usa Application.Current.MainWindow como owner cuando está disponible.
/// </summary>
public sealed class ViewDialogService : IViewDialogService
{
    public void ShowInDialog(object content, string title)
    {
        var owner = System.Windows.Application.Current?.MainWindow;
        var dialog = new ViewDialogWindow
        {
            Owner = owner,
            Title = title,
            DialogContent = content
        };
        dialog.ShowDialog();
    }
}
