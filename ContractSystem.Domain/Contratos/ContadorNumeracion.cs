namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Lleva la secuencia del contador de numeración.
/// Puede haber un registro por año (si ContadorPorAnio=true) o uno solo global.
/// </summary>
public class ContadorNumeracion
{
    public int Id { get; set; }

    /// <summary>
    /// Año al que corresponde este contador. Null si es contador global.
    /// </summary>
    public int? Anio { get; set; }

    /// <summary>
    /// Último número secuencial utilizado.
    /// </summary>
    public int UltimoNumero { get; set; }
}
