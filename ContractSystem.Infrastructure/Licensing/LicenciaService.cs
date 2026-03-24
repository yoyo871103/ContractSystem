using System.Data;
using ContractSystem.Application.Licensing;
using ContractSystem.Domain.Licensing;
using ContractSystem.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ContractSystem.Infrastructure.Licensing;

internal sealed class LicenciaService : ILicenciaService
{
    private readonly IApplicationDbContextFactory _dbFactory;

    public LicenciaService(IApplicationDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<string> GetFingerprintAsync(CancellationToken ct = default)
    {
        using var ctx = (ApplicationDbContext)_dbFactory.CreateDbContext();
        var (brokerGuid, createDate) = await GetDatabaseIdentifiersAsync(ctx, ct);
        return LicenciaKeyHelper.ComputeFingerprint(brokerGuid, createDate);
    }

    public async Task<LicenciaValidationResult> ValidarLicenciaAsync(CancellationToken ct = default)
    {
        using var ctx = (ApplicationDbContext)_dbFactory.CreateDbContext();
        var (brokerGuid, createDate) = await GetDatabaseIdentifiersAsync(ctx, ct);
        var fingerprint = LicenciaKeyHelper.ComputeFingerprint(brokerGuid, createDate);

        var licencia = await ctx.Set<LicenciaInfo>().FirstOrDefaultAsync(ct);
        if (licencia is null)
            return LicenciaValidationResult.SinLicencia(fingerprint);

        var (esValida, fechaExp) = LicenciaKeyHelper.ValidarClave(licencia.Clave, fingerprint);
        if (!esValida)
            return LicenciaValidationResult.Invalida(fingerprint);

        var diasRestantes = (int)(fechaExp.Date - DateTime.Today).TotalDays;
        if (diasRestantes < 0)
            return LicenciaValidationResult.LicenciaExpirada(fechaExp, fingerprint);

        return LicenciaValidationResult.Valida(fechaExp, diasRestantes, fingerprint);
    }

    public async Task<LicenciaValidationResult> ActivarLicenciaAsync(string clave, CancellationToken ct = default)
    {
        using var ctx = (ApplicationDbContext)_dbFactory.CreateDbContext();
        var (brokerGuid, createDate) = await GetDatabaseIdentifiersAsync(ctx, ct);
        var fingerprint = LicenciaKeyHelper.ComputeFingerprint(brokerGuid, createDate);

        var (esValida, fechaExp) = LicenciaKeyHelper.ValidarClave(clave, fingerprint);
        if (!esValida)
            return LicenciaValidationResult.Invalida(fingerprint);

        var diasRestantes = (int)(fechaExp.Date - DateTime.Today).TotalDays;
        if (diasRestantes < 0)
            return LicenciaValidationResult.LicenciaExpirada(fechaExp, fingerprint);

        // Guardar o actualizar la licencia
        var licencia = await ctx.Set<LicenciaInfo>().FirstOrDefaultAsync(ct);
        if (licencia is null)
        {
            licencia = new LicenciaInfo
            {
                Clave = clave.Trim(),
                FechaActivacion = DateTime.UtcNow,
                FechaExpiracion = fechaExp
            };
            ctx.Set<LicenciaInfo>().Add(licencia);
        }
        else
        {
            licencia.Clave = clave.Trim();
            licencia.FechaActivacion = DateTime.UtcNow;
            licencia.FechaExpiracion = fechaExp;
        }

        await ctx.SaveChangesAsync(ct);

        return LicenciaValidationResult.Valida(fechaExp, diasRestantes, fingerprint);
    }

    private static async Task<(string BrokerGuid, string CreateDate)> GetDatabaseIdentifiersAsync(
        ApplicationDbContext ctx, CancellationToken ct)
    {
        var connection = ctx.Database.GetDbConnection();

        // Solo abrir si no está ya abierta
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync(ct);

        using var cmd = connection.CreateCommand();

        // Determinar si es SQLite o SQL Server
        if (connection.GetType().Name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            // Para SQLite no hay service_broker_guid. Usar el nombre de archivo como fingerprint.
            var dataSource = connection.DataSource ?? connection.Database ?? "sqlite-default";
            return (dataSource, "sqlite");
        }

        // SQL Server: obtener service_broker_guid y create_date
        cmd.CommandText = @"
            SELECT
                CAST(service_broker_guid AS NVARCHAR(36)) AS BrokerGuid,
                CONVERT(NVARCHAR(30), create_date, 126) AS CreateDate
            FROM sys.databases
            WHERE name = DB_NAME()";

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException("No se pudo obtener la información de la base de datos.");

        var brokerGuid = reader.GetString(0);
        var createDate = reader.GetString(1);
        return (brokerGuid, createDate);
    }
}
