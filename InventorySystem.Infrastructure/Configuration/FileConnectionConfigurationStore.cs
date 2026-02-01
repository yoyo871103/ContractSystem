using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace InventorySystem.Infrastructure.Configuration;

internal sealed class FileConnectionConfigurationStore : IConnectionConfigurationStore
{
    /// <summary>
    /// Entropía adicional para el cifrado DPAPI (opcional, mejora aislamiento).
    /// Solo esta aplicación conoce esta entropía.
    /// </summary>
    private static readonly byte[] EncryptionEntropy = Encoding.UTF8.GetBytes("InventorySystem.ConnStore.v1");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly string _configFilePath;
    private readonly object _lock = new();

    public FileConnectionConfigurationStore(IOptions<InfrastructureOptions> options)
    {
        var dataDir = options.Value.DataDirectory;
        var fileName = options.Value.ConnectionConfigFileName;
        Directory.CreateDirectory(dataDir);
        _configFilePath = Path.Combine(dataDir, fileName);
    }

    public bool HasConnectionConfigured
    {
        get
        {
            lock (_lock)
            {
                return File.Exists(_configFilePath) && File.ReadAllText(_configFilePath).Trim().Length > 0;
            }
        }
    }

    public ConnectionSettings? GetSettings()
    {
        lock (_lock)
        {
            if (!File.Exists(_configFilePath))
                return null;

            var json = File.ReadAllText(_configFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var dto = JsonSerializer.Deserialize<ConnectionSettingsDto>(json, JsonOptions);
            return dto?.ToConnectionSettings(DecryptConnectionString);
        }
    }

    public void SaveSettings(ConnectionSettings settings)
    {
        lock (_lock)
        {
            var dto = ConnectionSettingsDto.From(settings, EncryptConnectionString);
            var json = JsonSerializer.Serialize(dto, JsonOptions);
            File.WriteAllText(_configFilePath, json);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            if (File.Exists(_configFilePath))
                File.Delete(_configFilePath);
        }
    }

    public (string? Server, string? Database)? GetSqlServerConnectionInfo()
    {
        var settings = GetSettings();
        if (settings?.Provider != DatabaseProvider.SqlServer || string.IsNullOrEmpty(settings.SqlServerConnectionString))
            return null;
        try
        {
            var builder = new SqlConnectionStringBuilder(settings.SqlServerConnectionString);
            return (builder.DataSource, string.IsNullOrEmpty(builder.InitialCatalog) ? null : builder.InitialCatalog);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Cifra la cadena de conexión con DPAPI. Solo el usuario de Windows actual en esta máquina puede descifrarla.
    /// </summary>
    private static string? EncryptConnectionString(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return null;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encrypted = ProtectedData.Protect(plainBytes, EncryptionEntropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Descifra la cadena de conexión protegida con DPAPI.
    /// </summary>
    private static string? DecryptConnectionString(string? encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64))
            return null;

        try
        {
            var encrypted = Convert.FromBase64String(encryptedBase64);
            var decrypted = ProtectedData.Unprotect(encrypted, EncryptionEntropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch
        {
            // Datos corruptos o cifrados por otro usuario/máquina
            return null;
        }
    }

    private sealed class ConnectionSettingsDto
    {
        public string Provider { get; set; } = "";
        /// <summary>
        /// Cadena de conexión cifrada con DPAPI para SQL Server (contiene credenciales).
        /// </summary>
        public string? SqlServerConnectionString { get; set; }
        public string? SqliteDatabasePath { get; set; }

        public static ConnectionSettingsDto From(ConnectionSettings settings, Func<string?, string?> encrypt)
        {
            return new ConnectionSettingsDto
            {
                Provider = settings.Provider.ToString(),
                SqlServerConnectionString = encrypt(settings.SqlServerConnectionString),
                SqliteDatabasePath = settings.SqliteDatabasePath // SQLite path no contiene credenciales sensibles
            };
        }

        public ConnectionSettings ToConnectionSettings(Func<string?, string?> decrypt)
        {
            var provider = Enum.Parse<DatabaseProvider>(Provider);
            return provider switch
            {
                DatabaseProvider.SqlServer => ConnectionSettings.ForSqlServer(
                    decrypt(SqlServerConnectionString) ?? throw new InvalidOperationException(
                        "No se pudo descifrar la configuración de SQL Server. Verifique que usa el mismo usuario de Windows.")),
                DatabaseProvider.Sqlite => ConnectionSettings.ForSqlite(SqliteDatabasePath!),
                _ => throw new ArgumentOutOfRangeException(nameof(Provider))
            };
        }
    }
}
