namespace ContractSystem.Application.Auth;

/// <summary>
/// Nombres de permisos de la aplicación. El rol Administrador tiene siempre todos los permisos.
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Permiso para ver la pestaña Gestión de usuarios y realizar CRUD de usuarios.
    /// </summary>
    public const string GestionarUsuarios = "GestionarUsuarios";
}
