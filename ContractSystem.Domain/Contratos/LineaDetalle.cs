using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Línea de detalle (producto/servicio) de un contrato, suplemento o anexo.
/// Puede venir del catálogo (snapshot R05) o ser escrita inline.
/// </summary>
public sealed class LineaDetalle : IAuditable
{
    public int Id { get; set; }

    /// <summary>
    /// Contrato/suplemento al que pertenece (siempre requerido).
    /// </summary>
    public int ContratoId { get; set; }
    public Contrato? Contrato { get; set; }

    /// <summary>
    /// Anexo al que pertenece (siempre requerido).
    /// </summary>
    public int AnexoId { get; set; }
    public Anexo Anexo { get; set; } = null!;

    /// <summary>
    /// Tipo: Producto o Servicio.
    /// </summary>
    public TipoProductoServicio Tipo { get; set; }

    /// <summary>
    /// Nombre/concepto de la línea (snapshot del catálogo o escrita inline).
    /// </summary>
    public string Concepto { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Cantidad { get; set; }

    /// <summary>
    /// Texto de la unidad de medida (snapshot del catálogo o inline).
    /// </summary>
    public string? UnidadMedidaTexto { get; set; }

    /// <summary>
    /// Referencia informativa al catálogo de unidades. No es FK activa (snapshot R05).
    /// </summary>
    public int? UnidadMedidaId { get; set; }

    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// Importe total. Calculado como Cantidad x PrecioUnitario, pero editable manualmente.
    /// </summary>
    public decimal ImporteTotal { get; set; }

    /// <summary>
    /// Referencia informativa al producto/servicio del catálogo (snapshot R05, no FK activa).
    /// </summary>
    public int? ProductoServicioOrigenId { get; set; }

    /// <summary>
    /// True si esta línea fue copiada de un contrato original al crear un suplemento.
    /// </summary>
    public bool EsCopiaDeOriginal { get; set; }

    public int Orden { get; set; }

    // --- Auditoría (IAuditable) ---
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
}
