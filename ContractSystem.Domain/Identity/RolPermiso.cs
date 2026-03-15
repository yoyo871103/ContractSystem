namespace ContractSystem.Domain.Identity;

/// <summary>
/// Tabla de unión muchos a muchos entre Rol y Permiso.
/// </summary>
public sealed class RolPermiso
{
    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public int PermisoId { get; set; }
    public Permiso Permiso { get; set; } = null!;
}
