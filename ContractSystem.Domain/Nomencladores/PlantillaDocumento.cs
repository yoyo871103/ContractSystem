namespace ContractSystem.Domain.Nomencladores;

/// <summary>
/// Plantilla de documento base (Word/PDF) con cláusulas revisadas por legal.
/// El usuario descarga, edita y sube como adjunto del contrato.
/// </summary>
public sealed class PlantillaDocumento : IAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoDocumentoPlantilla TipoDocumento { get; set; }
    public RolPlantilla Rol { get; set; }
    public byte[] Archivo { get; set; } = Array.Empty<byte>();
    public string NombreArchivo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
    public bool RevisadoPorLegal { get; set; }
}
