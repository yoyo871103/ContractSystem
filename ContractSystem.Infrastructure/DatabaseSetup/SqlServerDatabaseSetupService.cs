using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ContractSystem.Infrastructure.DatabaseSetup;

internal sealed class SqlServerDatabaseSetupService : IDatabaseSetupService
{
    /// <summary>
    /// Prefijo del usuario que crea la app. El nombre completo incluye un identificador único.
    /// Solo la aplicación conoce estas credenciales; el usuario nunca las ve.
    /// </summary>
    private const string AppUserPrefix = "InvApp_";

    private readonly ILogger<SqlServerDatabaseSetupService> _logger;

    public SqlServerDatabaseSetupService(ILogger<SqlServerDatabaseSetupService> logger)
    {
        _logger = logger;
    }

    public async Task<DatabaseSetupResult> TestSqlServerConnectionAsync(SqlServerSetupRequest request, CancellationToken cancellationToken = default)
    {
        var saConnectionString = BuildSaConnectionString(request.Server, request.SaPassword);
        try
        {
            await using var conn = new SqlConnection(saConnectionString);
            await conn.OpenAsync(cancellationToken);
            _logger.LogInformation("Conexión SA a SQL Server exitosa: {Server}", request.Server);
            return DatabaseSetupResult.Success(); // Solo validamos conexión, no devolvemos credenciales SA
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al conectar con SA a {Server}", request.Server);
            return DatabaseSetupResult.Failure($"No se pudo conectar al servidor: {ex.Message}");
        }
    }

    public async Task<DatabaseSetupResult> SetupSqlServerAsync(SqlServerSetupRequest request, CancellationToken cancellationToken = default)
    {
        var saConnectionString = BuildSaConnectionString(request.Server, request.SaPassword);

        try
        {
            await using var conn = new SqlConnection(saConnectionString);
            await conn.OpenAsync(cancellationToken);

            // 1. Crear base de datos si no existe
            if (request.CreateDatabaseIfNotExists)
            {
                await CreateDatabaseIfNotExistsAsync(conn, request.DatabaseName, cancellationToken);
            }
            else
            {
                if (!await DatabaseExistsAsync(conn, request.DatabaseName, cancellationToken))
                    return DatabaseSetupResult.Failure($"La base de datos '{request.DatabaseName}' no existe.");
            }

            // 2. Generar credenciales que solo conocerá la aplicación (imposibles de adivinar)
            var appUserName = GenerateAppUserName();
            var appUserPassword = GenerateStrongPassword();

            // 3. Crear login (en master)
            await CreateLoginIfNotExistsAsync(conn, appUserName, appUserPassword, cancellationToken);

            // 4. Crear usuario en la base de datos y asignar roles
            await CreateUserAndGrantRolesAsync(conn, request.DatabaseName, appUserName, cancellationToken);

            var appConnectionString = BuildAppConnectionString(request.Server, request.DatabaseName, appUserName, appUserPassword);

            // 5. Verificar que la conexión con el usuario de app funciona
            await using var testConn = new SqlConnection(appConnectionString);
            await testConn.OpenAsync(cancellationToken);
            await testConn.CloseAsync();

            var settings = ConnectionSettings.ForSqlServer(appConnectionString);
            _logger.LogInformation("Setup SQL Server completado: {Database} con usuario interno {User}", request.DatabaseName, appUserName);
            return DatabaseSetupResult.Success(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en setup SQL Server");
            return DatabaseSetupResult.Failure(ex.Message);
        }
    }

    public Task<DatabaseSetupResult> SetupSqliteAsync(SqliteSetupRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DatabaseSetupResult.Failure("Este servicio solo maneja SQL Server. Use la implementación de SQLite."));
    }

    private static string BuildSaConnectionString(string server, string password)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            UserID = "sa",
            Password = password,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };
        return builder.ConnectionString;
    }

    private static string BuildAppConnectionString(string server, string database, string user, string password)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            UserID = user,
            Password = password,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };
        return builder.ConnectionString;
    }

    private static async Task<bool> DatabaseExistsAsync(SqlConnection conn, string databaseName, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM sys.databases WHERE name = @name";
        cmd.Parameters.AddWithValue("@name", databaseName);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null;
    }

    private static async Task CreateDatabaseIfNotExistsAsync(SqlConnection conn, string databaseName, CancellationToken ct)
    {
        if (await DatabaseExistsAsync(conn, databaseName, ct))
            return;

        var safeName = SanitizeIdentifier(databaseName);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"CREATE DATABASE [{safeName}]";
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task CreateLoginIfNotExistsAsync(SqlConnection conn, string loginName, string password, CancellationToken ct)
    {
        var safeLogin = SanitizeIdentifier(loginName);
        var escapedPassword = password.Replace("'", "''");

        await using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM sys.server_principals WHERE name = @name";
        checkCmd.Parameters.AddWithValue("@name", loginName);
        if (await checkCmd.ExecuteScalarAsync(ct) != null)
            return; // Ya existe

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"CREATE LOGIN [{safeLogin}] WITH PASSWORD = '{escapedPassword}'";
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task CreateUserAndGrantRolesAsync(SqlConnection conn, string databaseName, string userName, CancellationToken ct)
    {
        var safeDb = SanitizeIdentifier(databaseName);
        var safeUser = SanitizeIdentifier(userName);

        await using var useCmd = conn.CreateCommand();
        useCmd.CommandText = $"USE [{safeDb}]";
        await useCmd.ExecuteNonQueryAsync(ct);

        await using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT 1 FROM sys.database_principals WHERE name = @name";
        checkCmd.Parameters.AddWithValue("@name", userName);
        if (await checkCmd.ExecuteScalarAsync(ct) != null)
            return; // Usuario ya existe en esta BD

        await using (var createUserCmd = conn.CreateCommand())
        {
            createUserCmd.CommandText = $"CREATE USER [{safeUser}] FOR LOGIN [{safeUser}]";
            await createUserCmd.ExecuteNonQueryAsync(ct);
        }

        foreach (var role in new[] { "db_datareader", "db_datawriter", "db_ddladmin" })
        {
            await using var roleCmd = conn.CreateCommand();
            roleCmd.CommandText = $"ALTER ROLE [{role}] ADD MEMBER [{safeUser}]";
            await roleCmd.ExecuteNonQueryAsync(ct);
        }
    }

    private static string SanitizeIdentifier(string value)
    {
        return Regex.Replace(value, @"[\[\]\s]", "");
    }

    /// <summary>
    /// Genera un nombre de usuario único que solo la app conoce.
    /// Formato: InvApp_xxxxxxxx (8 caracteres aleatorios).
    /// </summary>
    private static string GenerateAppUserName()
    {
        var suffix = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
        return $"{AppUserPrefix}{suffix}";
    }

    /// <summary>
    /// Genera una contraseña criptográficamente segura de 32 caracteres
    /// que cumple los requisitos de complejidad de SQL Server.
    /// </summary>
    private static string GenerateStrongPassword()
    {
        const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string Lower = "abcdefghjkmnpqrstuvwxyz";
        const string Digits = "23456789";
        const string Symbols = "!@#$%&*";
        const int Length = 32;

        var password = new char[Length];
        var bytes = RandomNumberGenerator.GetBytes(Length);

        // Garantizar al menos uno de cada tipo (requisito SQL Server)
        password[0] = Upper[bytes[0] % Upper.Length];
        password[1] = Lower[bytes[1] % Lower.Length];
        password[2] = Digits[bytes[2] % Digits.Length];
        password[3] = Symbols[bytes[3] % Symbols.Length];

        var allChars = Upper + Lower + Digits + Symbols;
        for (var i = 4; i < Length; i++)
        {
            password[i] = allChars[bytes[i] % allChars.Length];
        }

        // Mezclar para que los primeros 4 no sean predecibles
        for (var i = Length - 1; i > 0; i--)
        {
            var j = bytes[i % 4] % (i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
