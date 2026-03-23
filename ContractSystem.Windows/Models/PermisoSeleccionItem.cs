using CommunityToolkit.Mvvm.ComponentModel;

namespace ContractSystem.Windows.Models;

/// <summary>
/// Permiso con flag de selección para asignación en formularios de rol/usuario.
/// </summary>
public sealed partial class PermisoSeleccionItem : ObservableObject
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public string? Categoria { get; init; }

    [ObservableProperty]
    private bool _isSelected;
}
