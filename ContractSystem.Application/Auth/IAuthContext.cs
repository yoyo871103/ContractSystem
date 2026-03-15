namespace ContractSystem.Application.Auth;

/// <summary>
/// Contexto del usuario actual en la sesión (en memoria; se rellena tras login).
/// Se usa en toda la app para menú, permisos y redirección al cambio de contraseña.
/// </summary>
public interface IAuthContext
{
    /// <summary>
    /// True si hay un usuario de aplicación autenticado (login normal).
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// True si la sesión es solo "administrador SQL" (solo gestión de usuarios).
    /// </summary>
    bool IsSqlAdminOnly { get; }

    int? UsuarioId { get; }
    string? NombreUsuario { get; }
    string? NombreParaMostrar { get; }
    string? Email { get; }
    byte[]? FotoPerfil { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permisos { get; }
    bool EsAdministrador { get; }

    /// <summary>
    /// Indica si el usuario tiene el permiso indicado (o es Administrador, que tiene todos).
    /// </summary>
    bool TienePermiso(string nombrePermiso);
    bool RequiereCambioContraseña { get; }

    /// <summary>
    /// Establece la sesión como usuario de aplicación (tras login normal).
    /// </summary>
    void SetUser(AuthUserInfo user);

    /// <summary>
    /// Actualiza los datos de perfil del usuario actual en la sesión (tras editar perfil).
    /// </summary>
    void UpdateProfile(string nombreParaMostrar, string? email, byte[]? fotoPerfil);

    /// <summary>
    /// Establece la sesión como solo administrador SQL (tras login con SA).
    /// </summary>
    void SetSqlAdminOnly();

    /// <summary>
    /// Marca que el usuario ha cambiado la contraseña (RequiereCambioContraseña pasa a false).
    /// </summary>
    void ClearRequiresPasswordChange();

    /// <summary>
    /// Cierra la sesión (logout).
    /// </summary>
    void Clear();
}
