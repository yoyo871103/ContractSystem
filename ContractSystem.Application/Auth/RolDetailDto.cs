namespace ContractSystem.Application.Auth;

/// <summary>
/// Detalle de un rol para edición (incluye permisos asignados).
/// </summary>
public sealed record RolDetailDto(
    int Id,
    string Nombre,
    string? Descripcion,
    IReadOnlyList<int> PermisoIds);

/// <summary>
/// Rol para listado con info adicional.
/// </summary>
public sealed record RolListItem(
    int Id,
    string Nombre,
    string? Descripcion,
    int CantidadPermisos,
    int CantidadUsuarios,
    bool EsSistema);

/// <summary>
/// Permiso para asignación en UI.
/// </summary>
public sealed record PermisoItem(
    int Id,
    string Nombre,
    string? Descripcion,
    string? Categoria);
