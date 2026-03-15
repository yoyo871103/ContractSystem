using ContractSystem.Application.Auth;
using Microsoft.Data.SqlClient;

namespace ContractSystem.Infrastructure.Auth;

/// <summary>
/// Valida credenciales de administrador SQL (SA o sysadmin) con una conexión temporal.
/// No persiste ni registra la contraseña.
/// </summary>
internal sealed class SqlServerAdminAuthService : ISqlServerAdminAuthService
{
    public async Task<SqlAdminAuthResult> ValidateSqlAdminAsync(string server, string user, string password, string? database, CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            UserID = user,
            Password = password,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };
        if (!string.IsNullOrWhiteSpace(database))
            builder.InitialCatalog = database;

        try
        {
            await using var conn = new SqlConnection(builder.ConnectionString);
            await conn.OpenAsync(cancellationToken);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CAST(IS_SRVROLEMEMBER('sysadmin') AS INT)";
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            var isAdmin = result is int i && i == 1;

            if (!isAdmin)
                return SqlAdminAuthResult.Failure("El usuario indicado no tiene privilegios de administrador de SQL Server.");

            return SqlAdminAuthResult.Success();
        }
        catch (Exception ex)
        {
            return SqlAdminAuthResult.Failure($"No se pudo conectar o validar: {ex.Message}");
        }
    }
}
