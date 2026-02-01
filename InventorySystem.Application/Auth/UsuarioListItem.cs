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
}
