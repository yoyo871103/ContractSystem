namespace ContractSystem.Application.Auth;

/// <summary>
/// Usuarios y roles especiales de la aplicación (creados en el seed).
/// </summary>
public static class DefaultUsers
{
    /// <summary>
    /// Nombre de usuario del administrador por defecto.
    /// </summary>
    public const string NombreUsuarioAdmin = "admin";

    /// <summary>
    /// Nombre del rol con acceso total al sistema. Se crea en el seed y siempre
    /// recibe todos los permisos existentes.
    /// </summary>
    public const string RolAdministradorSistema = "Administrador de sistema";
}
