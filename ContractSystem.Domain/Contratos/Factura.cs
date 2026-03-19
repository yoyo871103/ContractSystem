namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Factura asociada a un contrato (Específico, Independiente o Suplemento).
/// No se permite asociar facturas a Contratos Marco.
/// </summary>
public class Factura : IAuditable
{
    public int Id { get; set; }

    /// <summary>
    /// Número de la factura.
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de emisión de la factura.
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Importe total de la factura.
    /// </summary>
    public decimal ImporteTotal { get; set; }

    /// <summary>
    /// Descripción o concepto de la factura.
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    // --- FK al contrato ---
    public int ContratoId { get; set; }
    public Contrato Contrato { get; set; } = null!;

    // --- Auditoría (IAuditable) ---
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
}
