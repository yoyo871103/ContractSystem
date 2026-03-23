using ContractSystem.Domain;

namespace ContractSystem.Domain.Identity;

/// <summary>
/// Usuario de la aplicación. Las credenciales se validan contra hash/salt almacenados aquí.
/// Soporta soft delete y foto de perfil.
/// </summary>
public sealed class Usuario : ISoftDeletable
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreParaMostrar { get; set; } = string.Empty;
    public string? Email { get; set; }
    /// <summary>Foto de perfil en bytes (opcional).</summary>
    public byte[]? FotoPerfil { get; set; }
    public string HashContraseña { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;

    /// <summary>
    /// True cuando se ha asignado una contraseña temporal (p. ej. desde modo SA en SQL Server).
    /// El usuario debe cambiar la contraseña en el próximo login.
    /// </summary>
    public bool RequiereCambioContraseña { get; set; }

    public bool Activo { get; set; } = true;
    public DateTimeOffset FechaCreacion { get; set; }
    public DateTimeOffset? UltimoAcceso { get; set; }

    /// <summary>Soft delete: cuando no es null, el usuario se considera eliminado.</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public ICollection<UsuarioPermiso> UsuarioPermisos { get; set; } = new List<UsuarioPermiso>();
}
