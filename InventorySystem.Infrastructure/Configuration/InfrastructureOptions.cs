namespace InventorySystem.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para la capa de infraestructura.
/// Cada app (Windows, MAUI) puede proporcionar su ruta de datos.
/// </summary>
public sealed class InfrastructureOptions
{
    /// <summary>
    /// Directorio base para archivos de datos (configuración, BD SQLite si aplica).
    /// - Windows: por defecto AppData\InventorySystem
    /// - MAUI: FileSystem.AppDataDirectory (configurar explícitamente)
    /// </summary>
    public string DataDirectory { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "InventorySystem");

    /// <summary>
    /// Nombre del archivo donde se guarda la configuración de conexión (dentro de DataDirectory).
    /// </summary>
    public string ConnectionConfigFileName { get; init; } = "connection.json";
}
