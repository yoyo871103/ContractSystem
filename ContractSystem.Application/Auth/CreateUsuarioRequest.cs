namespace ContractSystem.Application.Auth;

/// <summary>
/// Solicitud para crear un usuario (gestión por administrador).
/// </summary>
public sealed class CreateUsuarioRequest
{
    public string NombreUsuario { get; init; } = string.Empty;
    public string NombreParaMostrar { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string HashContraseña { get; init; } = string.Empty;
    public string Salt { get; init; } = string.Empty;
    public bool RequiereCambioContraseña { get; init; }
    /// <summary>Ids de roles a asignar. Si vacío, no se asigna ningún rol.</summary>
    public IReadOnlyList<int>? RolIds { get; init; }
    /// <summary>Ids de permisos directos a asignar (sin pasar por rol).</summary>
    public IReadOnlyList<int>? PermisoDirectoIds { get; init; }
}
