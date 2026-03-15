namespace ContractSystem.Application.Auth;

/// <summary>
/// Servicio de autenticación contra usuarios de la aplicación (hash/salt en BD).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Valida credenciales y devuelve el usuario con roles y si requiere cambio de contraseña.
    /// </summary>
    Task<AuthResult> AuthenticateAsync(string nombreUsuario, string contraseña, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia la contraseña del usuario (contraseña actual + nueva). Devuelve true si tuvo éxito.
    /// </summary>
    Task<bool> ChangePasswordAsync(int usuarioId, string contraseñaActual, string contraseñaNueva, CancellationToken cancellationToken = default);
}
