namespace ContractSystem.Application.Auth;

/// <summary>
/// Servicio para validar credenciales de administrador SQL (SA o sysadmin).
/// Solo tiene sentido cuando el proveedor de BD es SQL Server.
/// No guarda la contraseña; usa conexión temporal solo para comprobar.
/// </summary>
public interface ISqlServerAdminAuthService
{
    /// <summary>
    /// Abre una conexión temporal con las credenciales indicadas y comprueba si el login es administrador
    /// (p. ej. IS_SRVROLEMEMBER('sysadmin')). No persiste credenciales.
    /// </summary>
    Task<SqlAdminAuthResult> ValidateSqlAdminAsync(string server, string user, string password, string? database, CancellationToken cancellationToken = default);
}
