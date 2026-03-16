namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Configuración del formato de numeración automática para documentos contractuales.
/// Variables soportadas: {YYYY}, {MM}, {CODIGO_CLIENTE}, {TIPO}, {CONTADOR}
/// Ejemplo: "CON-{TIPO}-{YYYY}-{CONTADOR:4}" → "CON-MARCO-2026-0001"
/// </summary>
public class ConfiguracionNumeracion
{
    public int Id { get; set; }

    /// <summary>
    /// Formato con variables. Ej: "CON-{TIPO}-{YYYY}-{CONTADOR}"
    /// </summary>
    public string Formato { get; set; } = "CON-{TIPO}-{YYYY}-{CONTADOR}";

    /// <summary>
    /// Dígitos de relleno para el contador (ej: 4 → 0001).
    /// </summary>
    public int DigitosPadding { get; set; } = 4;

    /// <summary>
    /// Si true, el contador se reinicia cada año. Si false, es global.
    /// </summary>
    public bool ContadorPorAnio { get; set; } = true;

    /// <summary>
    /// Solo puede haber una configuración activa.
    /// </summary>
    public bool Activa { get; set; } = true;

    public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
}
