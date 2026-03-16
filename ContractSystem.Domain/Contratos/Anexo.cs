namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Sección interna de un contrato o suplemento (ej: "Anexo Técnico", "Anexo Económico").
/// No es un documento independiente, sino una forma de organizar información dentro del documento padre.
/// </summary>
public sealed class Anexo : IAuditable
{
    public int Id { get; set; }

    /// <summary>
    /// Contrato o suplemento al que pertenece este anexo.
    /// </summary>
    public int ContratoId { get; set; }
    public Contrato? Contrato { get; set; }

    public string Nombre { get; set; } = string.Empty;
    public string? CondicionesEntrega { get; set; }
    public string? CostosAsociados { get; set; }

    /// <summary>
    /// Orden de aparición dentro del documento.
    /// </summary>
    public int Orden { get; set; }

    /// <summary>
    /// Líneas de detalle propias de este anexo.
    /// </summary>
    public ICollection<LineaDetalle> Lineas { get; set; } = new List<LineaDetalle>();

    // --- Auditoría (IAuditable) ---
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
}
