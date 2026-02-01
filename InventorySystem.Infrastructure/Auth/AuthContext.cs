using InventorySystem.Application.Auth;

namespace InventorySystem.Infrastructure.Auth;

/// <summary>
/// Contexto del usuario actual en memoria. Se rellena tras login y se usa en toda la app.
/// </summary>
internal sealed class AuthContext : IAuthContext
{
    private AuthUserInfo? _user;
    private bool _sqlAdminOnly;

    public bool IsAuthenticated => _user is not null && !_sqlAdminOnly;
    public bool IsSqlAdminOnly => _sqlAdminOnly;

    public int? UsuarioId => _user?.UsuarioId;
    public string? NombreUsuario => _user?.NombreUsuario;
    public string? NombreParaMostrar => _user?.NombreParaMostrar;
    public string? Email => _user?.Email;
    public byte[]? FotoPerfil => _user?.FotoPerfil;
    public IReadOnlyList<string> Roles => _user?.Roles ?? Array.Empty<string>();
    public IReadOnlyList<string> Permisos => _user?.Permisos ?? Array.Empty<string>();
    public bool EsAdministrador => _user?.EsAdministrador ?? false;

    public bool TienePermiso(string nombrePermiso) =>
        EsAdministrador || (Permisos?.Contains(nombrePermiso, StringComparer.OrdinalIgnoreCase) ?? false);
    public bool RequiereCambioContraseña => _user?.RequiereCambioContraseña ?? false;

    public void SetUser(AuthUserInfo user)
    {
        _user = user;
        _sqlAdminOnly = false;
    }

    public void UpdateProfile(string nombreParaMostrar, string? email, byte[]? fotoPerfil)
    {
        if (_user is null) return;
        _user = _user with
        {
            NombreParaMostrar = nombreParaMostrar,
            Email = email,
            FotoPerfil = fotoPerfil
        };
    }

    public void SetSqlAdminOnly()
    {
        _user = null;
        _sqlAdminOnly = true;
    }

    public void ClearRequiresPasswordChange()
    {
        if (_user is null)
            return;
        _user = _user with { RequiereCambioContraseña = false };
    }

    public void Clear()
    {
        _user = null;
        _sqlAdminOnly = false;
    }
}
