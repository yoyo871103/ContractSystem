using InventorySystem.Application.Common.Models;
using InventorySystem.Domain.Identity;

namespace InventorySystem.Application.Auth;

/// <summary>
/// Acceso a usuarios de la aplicación para autenticación (por nombre de usuario, con roles).
/// Todas las operaciones son asíncronas. Los listados son paginados para optimizar consultas.
/// </summary>
public interface IUsuarioStore
{
    /// <summary>
    /// Obtiene un usuario por nombre de usuario, incluyendo roles, o null si no existe.
    /// </summary>
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario por id, incluyendo roles, o null si no existe.
    /// </summary>
    Task<Usuario?> GetByIdAsync(int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza la fecha de último acceso del usuario.
    /// </summary>
    Task UpdateUltimoAccesoAsync(int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza hash, salt y flag RequiereCambioContraseña del usuario.
    /// </summary>
    Task UpdatePasswordAsync(int usuarioId, string hash, string salt, bool requiereCambioContraseña, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista usuarios no eliminados de forma paginada (gestión de usuarios en modo admin SQL).
    /// Consulta optimizada: solo se cargan los registros de la página solicitada.
    /// </summary>
    Task<PagedList<UsuarioListItem>> ListPagedAsync(FilterableQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza nombre para mostrar, email y foto de perfil del usuario.
    /// </summary>
    Task UpdateProfileAsync(int usuarioId, string nombreParaMostrar, string? email, byte[]? fotoPerfil, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca el usuario como eliminado (soft delete).
    /// </summary>
    Task SetDeletedAsync(int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Quita la marca de eliminado del usuario (reactivación).
    /// </summary>
    Task SetUndeletedAsync(int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea un usuario con contraseña y roles (gestión por administrador).
    /// </summary>
    Task<Usuario?> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un usuario por id para edición (incluye roles). No filtra por Activo.
    /// </summary>
    Task<UsuarioEditDto?> GetByIdForEditAsync(int usuarioId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza nombre, email, activo y roles del usuario.
    /// </summary>
    Task UpdateUsuarioAsync(int usuarioId, string nombreParaMostrar, string? email, bool activo, IReadOnlyList<int> rolIds, CancellationToken cancellationToken = default);
}
