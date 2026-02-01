namespace InventorySystem.Application.Auth;

/// <summary>
/// Usuarios especiales de la aplicación (p. ej. el admin por defecto creado en el seed).
/// </summary>
public static class DefaultUsers
{
    /// <summary>
    /// Nombre de usuario del administrador por defecto. No se puede eliminar ni deshabilitar; solo él puede editar a otros Administradores desde Gestión de usuarios.
    /// </summary>
    public const string NombreUsuarioAdmin = "admin";
}
