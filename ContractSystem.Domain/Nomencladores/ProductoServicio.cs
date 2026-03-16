namespace ContractSystem.Domain.Nomencladores;

/// <summary>
/// Producto o servicio del catálogo. Se usa como fuente para líneas de detalle (snapshot).
/// </summary>
public sealed class ProductoServicio : ISoftDeletable, IAuditable
{
    public int Id { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoProductoServicio Tipo { get; set; }

    /// <summary>
    /// Unidad de medida recomendada (del catálogo). Nullable si no aplica.
    /// </summary>
    public int? UnidadMedidaId { get; set; }
    public UnidadMedida? UnidadMedida { get; set; }

    /// <summary>
    /// Precio estimado orientativo (opcional).
    /// </summary>
    public decimal? PrecioEstimado { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt != null;

    // --- Auditoría (IAuditable) ---
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
}
