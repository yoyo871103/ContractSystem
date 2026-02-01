namespace InventorySystem.Application;

/// <summary>
/// Parámetros para configurar la conexión a SQLite.
/// La app Windows permite seleccionar ruta; la app MAUI suele usar una ruta fija en datos locales.
/// </summary>
public sealed record SqliteSetupRequest
{
    /// <summary>
    /// Ruta completa del archivo .db (existente o a crear).
    /// </summary>
    public required string DatabasePath { get; init; }

    /// <summary>
    /// Si true, crea el directorio padre si no existe.
    /// </summary>
    public bool CreateDirectoryIfNotExists { get; init; } = true;
}
