namespace ContractSystem.Application.Auth;

/// <summary>
/// Resultado de la validación de credenciales de administrador SQL (SA o equivalente).
/// Solo aplica cuando la conexión configurada es SQL Server.
/// </summary>
public sealed record SqlAdminAuthResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static SqlAdminAuthResult Success() => new() { IsSuccess = true };

    public static SqlAdminAuthResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
