namespace InventorySystem.Application.Configuration;

/// <summary>
/// Almacén de la configuración de conexión (persistida en archivo local).
/// La app Windows usa una ruta en AppData; la app MAUI usa su directorio de datos.
/// </summary>
public interface IConnectionConfigurationStore
{
    /// <summary>
    /// Indica si existe una conexión configurada (el usuario ya completó el setup).
    /// </summary>
    bool HasConnectionConfigured { get; }

    /// <summary>
    /// Obtiene la configuración actual o null si no hay ninguna.
    /// </summary>
    ConnectionSettings? GetSettings();

    /// <summary>
    /// Guarda la configuración de conexión tras un setup exitoso.
    /// </summary>
    void SaveSettings(ConnectionSettings settings);

    /// <summary>
    /// Borra la configuración (p. ej. para permitir reconfigurar).
    /// </summary>
    void Clear();

    /// <summary>
    /// Cuando el proveedor es SQL Server, devuelve (Servidor, Base de datos) para uso en modo administrador SQL.
    /// </summary>
    (string? Server, string? Database)? GetSqlServerConnectionInfo();
}
