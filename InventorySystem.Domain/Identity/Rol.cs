namespace InventorySystem.Domain.Identity;

/// <summary>
/// Rol de la aplicación (ej. Administrador, Operador, SoloConsulta).
/// </summary>
public sealed class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Relación muchos a muchos con Usuario (tabla UsuarioRoles).
    /// </summary>
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}
