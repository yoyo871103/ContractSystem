using CommunityToolkit.Mvvm.ComponentModel;

namespace InventorySystem.Windows.Models;

/// <summary>
/// Rol con flag de selección para asignación en formularios de usuario.
/// </summary>
public sealed partial class RolSeleccionItem : ObservableObject
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
