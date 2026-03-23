namespace ContractSystem.Domain.Identity;

/// <summary>
/// Permiso de la aplicación (ej. Contratos.Ver). Se asigna a roles y/o directamente a usuarios.
/// </summary>
public sealed class Permiso
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Categoria { get; set; }

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
    public ICollection<UsuarioPermiso> UsuarioPermisos { get; set; } = new List<UsuarioPermiso>();
}
