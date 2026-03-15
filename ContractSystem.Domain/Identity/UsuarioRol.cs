namespace ContractSystem.Domain.Identity;

/// <summary>
/// Tabla de unión muchos a muchos entre Usuario y Rol.
/// </summary>
public sealed class UsuarioRol
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!;
}
