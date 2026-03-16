namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Registro inmutable de un cambio realizado sobre un contrato o suplemento.
/// </summary>
public sealed class HistorialCambio
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.UtcNow;
    public int? UsuarioId { get; set; }
    public string? UsuarioNombre { get; set; }
    public TipoCambio TipoCambio { get; set; }

    /// <summary>
    /// Contrato o suplemento al que pertenece este registro.
    /// </summary>
    public int ContratoId { get; set; }
    public Contrato Contrato { get; set; } = null!;

    /// <summary>
    /// Descripción legible del cambio (ej: "Estado cambió de VIGENTE a RESCINDIDO").
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Valor anterior en formato JSON (para consultas).
    /// </summary>
    public string? ValorAnterior { get; set; }

    /// <summary>
    /// Valor nuevo en formato JSON.
    /// </summary>
    public string? ValorNuevo { get; set; }
}
