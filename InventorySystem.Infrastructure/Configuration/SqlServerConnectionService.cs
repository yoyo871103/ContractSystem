using InventorySystem.Application.Configuration;
using Microsoft.Data.SqlClient;

namespace InventorySystem.Infrastructure.Configuration;

internal sealed class SqlServerConnectionService : ISqlServerConnectionService
{
    public async Task<bool> TestConnectionAsync(string server, string user, string password, string? database, CancellationToken cancellationToken = default)
    {
        var connectionString = BuildConnectionString(server, user, password, database ?? "master");
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> ListDatabasesAsync(string server, string user, string password, CancellationToken cancellationToken = default)
    {
        var connectionString = BuildConnectionString(server, user, password, "master");
        var list = new List<string>();
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sys.databases WHERE state = 0 ORDER BY name";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString(0);
            list.Add(name);
        }
        return list;
    }

    public async Task<bool> CreateDatabaseAsync(string server, string user, string password, string databaseName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            return false;
        var connectionString = BuildConnectionString(server, user, password, "master");
        var safeName = databaseName.Trim().Replace("]", "]]");
        if (safeName.IndexOf('[') < 0)
            safeName = "[" + safeName + "]";
        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE {safeName}";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string BuildConnectionString(string server, string user, string password, string database)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            UserID = user,
            Password = password,
            InitialCatalog = database,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };
        return builder.ConnectionString;
    }
}
