using InventorySystem.Application.Auth;
using InventorySystem.Domain.Identity;

namespace InventorySystem.Infrastructure.Auth;

internal sealed class AuthService : IAuthService
{
    private readonly IUsuarioStore _usuarioStore;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISeedDataService _seedDataService;

    public AuthService(IUsuarioStore usuarioStore, IPasswordHasher passwordHasher, ISeedDataService seedDataService)
    {
        _usuarioStore = usuarioStore;
        _passwordHasher = passwordHasher;
        _seedDataService = seedDataService;
    }

    public async Task<AuthResult> AuthenticateAsync(string nombreUsuario, string contraseña, CancellationToken cancellationToken = default)
    {
        await _seedDataService.EnsureSeedAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(nombreUsuario))
            return AuthResult.Failure("Usuario o contraseña incorrectos.");

        var usuario = await _usuarioStore.GetByNombreUsuarioAsync(nombreUsuario.Trim(), cancellationToken);
        if (usuario is null)
            return AuthResult.Failure("Usuario o contraseña incorrectos.");

        if (!_passwordHasher.VerifyPassword(contraseña, usuario.HashContraseña, usuario.Salt))
            return AuthResult.Failure("Usuario o contraseña incorrectos.");

        await _usuarioStore.UpdateUltimoAccesoAsync(usuario.Id, cancellationToken);

        var roles = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList();
        var esAdmin = roles.Contains("Administrador", StringComparer.OrdinalIgnoreCase);
        var permisos = usuario.UsuarioRoles
            .SelectMany(ur => ur.Rol.RolPermisos.Select(rp => rp.Permiso.Nombre))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var userInfo = new AuthUserInfo
        {
            UsuarioId = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            NombreParaMostrar = usuario.NombreParaMostrar,
            Email = usuario.Email,
            FotoPerfil = usuario.FotoPerfil,
            Roles = roles,
            Permisos = permisos,
            RequiereCambioContraseña = usuario.RequiereCambioContraseña,
            EsAdministrador = esAdmin
        };

        return AuthResult.Success(userInfo);
    }

    public async Task<bool> ChangePasswordAsync(int usuarioId, string contraseñaActual, string contraseñaNueva, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioStore.GetByIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return false;

        if (!_passwordHasher.VerifyPassword(contraseñaActual, usuario.HashContraseña, usuario.Salt))
            return false;

        var (hash, salt) = _passwordHasher.HashPassword(contraseñaNueva);
        await _usuarioStore.UpdatePasswordAsync(usuarioId, hash, salt, requiereCambioContraseña: false, cancellationToken);
        return true;
    }
}
