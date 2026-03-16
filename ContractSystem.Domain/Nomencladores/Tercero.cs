namespace ContractSystem.Domain.Nomencladores;

/// <summary>
/// Tercero: cliente, proveedor o ambos. Catálogo de contrapartes.
/// </summary>
public sealed class Tercero : ISoftDeletable, IAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string NifCif { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoTercero Tipo { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public bool IsDeleted => DeletedAt != null;

    // --- Auditoría (IAuditable) ---
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }

    /// <summary>
    /// Contactos asociados al tercero.
    /// </summary>
    public ICollection<ContactoTercero> Contactos { get; set; } = new List<ContactoTercero>();
}
