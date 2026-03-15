using ContractSystem.Application.Common.Models;

namespace ContractSystem.Application.Auth;

/// <summary>
/// Consulta paginada y filtrable para el listado de usuarios (gestión de usuarios).
/// </summary>
public sealed class UsuariosListQuery : FilterableQuery
{
    /// <summary>Si es true, el listado incluye también usuarios eliminados (soft delete).</summary>
    public bool IncludeDeleted { get; set; }
}
