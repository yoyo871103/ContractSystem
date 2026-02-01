namespace InventorySystem.Application.DatabaseSetup;

/// <summary>
/// Resultado de una operación de setup de base de datos.
/// </summary>
public sealed record DatabaseSetupResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public ConnectionSettings? Settings { get; init; }

    public static DatabaseSetupResult Success(ConnectionSettings settings) => new()
    {
        IsSuccess = true,
        Settings = settings
    };

    /// <summary>
    /// Resultado exitoso sin settings (p. ej. test de conexión SA).
    /// </summary>
    public static DatabaseSetupResult Success() => new()
    {
        IsSuccess = true
    };

    public static DatabaseSetupResult Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}
