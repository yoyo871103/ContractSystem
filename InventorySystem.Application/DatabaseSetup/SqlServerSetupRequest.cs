namespace InventorySystem.Application.DatabaseSetup;

/// <summary>
/// Parámetros para configurar la conexión a SQL Server.
/// Se usa SA para el setup; la aplicación crea un usuario dedicado con credenciales
/// generadas automáticamente que solo conoce la app (nunca expuestas al usuario).
/// </summary>
public sealed record SqlServerSetupRequest
{
    /// <summary>
    /// Dirección del servidor (p. ej. "localhost", "192.168.1.10", ".\SQLEXPRESS").
    /// </summary>
    public required string Server { get; init; }

    /// <summary>
    /// Contraseña del usuario SA (solo para el setup, nunca se almacena).
    /// </summary>
    public required string SaPassword { get; init; }

    /// <summary>
    /// Nombre de la base de datos (existente o a crear).
    /// </summary>
    public required string DatabaseName { get; init; }

    /// <summary>
    /// Indica si se debe crear la base de datos si no existe.
    /// </summary>
    public bool CreateDatabaseIfNotExists { get; init; } = true;
}
