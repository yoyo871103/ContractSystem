namespace ContractSystem.Application.Licensing;

/// <summary>
/// Resultado de la validación de licencia.
/// </summary>
public sealed class LicenciaValidationResult
{
    public bool EsValida { get; init; }
    public bool Expirada { get; init; }
    public int DiasRestantes { get; init; }
    public DateTime? FechaExpiracion { get; init; }
    public string? Mensaje { get; init; }
    public string? Fingerprint { get; init; }

    public static LicenciaValidationResult SinLicencia(string fingerprint) => new()
    {
        EsValida = false,
        Expirada = false,
        DiasRestantes = 0,
        Fingerprint = fingerprint,
        Mensaje = "No hay licencia activada."
    };

    public static LicenciaValidationResult Invalida(string fingerprint) => new()
    {
        EsValida = false,
        Expirada = false,
        DiasRestantes = 0,
        Fingerprint = fingerprint,
        Mensaje = "La licencia no es válida para esta base de datos."
    };

    public static LicenciaValidationResult LicenciaExpirada(DateTime fechaExpiracion, string fingerprint) => new()
    {
        EsValida = false,
        Expirada = true,
        DiasRestantes = 0,
        FechaExpiracion = fechaExpiracion,
        Fingerprint = fingerprint,
        Mensaje = "La licencia ha expirado."
    };

    public static LicenciaValidationResult Valida(DateTime fechaExpiracion, int diasRestantes, string fingerprint) => new()
    {
        EsValida = true,
        Expirada = false,
        DiasRestantes = diasRestantes,
        FechaExpiracion = fechaExpiracion,
        Fingerprint = fingerprint
    };
}

/// <summary>
/// Servicio para gestión y validación de licencias.
/// </summary>
public interface ILicenciaService
{
    /// <summary>
    /// Obtiene el fingerprint de la base de datos actual (hash de service_broker_guid + create_date).
    /// </summary>
    Task<string> GetFingerprintAsync(CancellationToken ct = default);

    /// <summary>
    /// Valida la licencia actual almacenada en la BD.
    /// </summary>
    Task<LicenciaValidationResult> ValidarLicenciaAsync(CancellationToken ct = default);

    /// <summary>
    /// Activa una licencia con la clave proporcionada.
    /// </summary>
    Task<LicenciaValidationResult> ActivarLicenciaAsync(string clave, CancellationToken ct = default);
}
