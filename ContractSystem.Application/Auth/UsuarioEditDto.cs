namespace ContractSystem.Application.Auth;

/// <summary>
/// Datos de un usuario para edición (gestión de usuarios por administrador).
/// </summary>
public sealed class UsuarioEditDto
{
    public int Id { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
    public string NombreParaMostrar { get; init; } = string.Empty;
    public string? Email { get; init; }
    public bool Activo { get; init; }
    /// <summary>Ids de roles asignados al usuario.</summary>
    public IReadOnlyList<int> RolIds { get; init; } = Array.Empty<int>();
}
