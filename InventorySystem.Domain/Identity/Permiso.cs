namespace InventorySystem.Domain.Identity;

/// <summary>
/// Permiso de la aplicación (ej. GestionarUsuarios). Se asigna a roles; el usuario los obtiene a través de sus roles.
/// </summary>
public sealed class Permiso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
