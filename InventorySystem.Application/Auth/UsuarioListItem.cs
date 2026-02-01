namespace InventorySystem.Application.Auth;

/// <summary>
/// Resumen de un usuario para listados (gestión de usuarios).
/// </summary>
public sealed record UsuarioListItem
{
    public int Id { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
    public string NombreParaMostrar { get; init; } = string.Empty;
    public bool Activo { get; init; }
    public bool RequiereCambioContraseña { get; init; }
    /// <summary>Nombres de roles asignados (ej. "Administrador, Operador").</summary>
    public string RolesDisplay { get; init; } = string.Empty;

    /// <summary>True si el usuario está marcado como eliminado (soft delete).</summary>
    public bool IsDeleted { get; init; }

    /// <summary>True si el usuario tiene el rol Administrador.</summary>
    public bool IsAdministrador { get; init; }

    /// <summary>True si es el usuario admin por defecto de la app (no se puede eliminar ni deshabilitar; solo él puede editar a otros Administradores).</summary>
    public bool IsDefaultAdmin { get; init; }
}
