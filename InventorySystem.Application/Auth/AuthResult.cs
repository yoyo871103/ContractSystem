namespace InventorySystem.Application.Auth;

/// <summary>
/// Resultado de la autenticación contra usuarios de la aplicación.
/// </summary>
public sealed record AuthResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public AuthUserInfo? User { get; init; }

    public static AuthResult Success(AuthUserInfo user) => new()
    {
        IsSuccess = true,
        User = user
    };

    public static AuthResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Datos del usuario autenticado para uso en la app.
/// </summary>
public sealed record AuthUserInfo
{
    public int UsuarioId { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
    public string NombreParaMostrar { get; init; } = string.Empty;
    public string? Email { get; init; }
    /// <summary>Foto de perfil en bytes (opcional).</summary>
    public byte[]? FotoPerfil { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public bool RequiereCambioContraseña { get; init; }
    public bool EsAdministrador { get; init; }
}
