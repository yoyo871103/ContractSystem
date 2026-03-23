namespace ContractSystem.Domain.Identity;

/// <summary>
/// Tabla de unión muchos a muchos entre Usuario y Permiso (permisos directos, sin pasar por rol).
/// </summary>
public sealed class UsuarioPermiso
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int PermisoId { get; set; }
    public Permiso Permiso { get; set; } = null!;
}
