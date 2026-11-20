namespace ContractSystem.Domain.Nomencladores;

/// <summary>
/// Plantilla de documento base (Word, PDF, Excel, imagen o texto) con cláusulas revisadas por legal.
/// El usuario la consulta, descarga, edita y sube como adjunto del contrato.
/// </summary>
public sealed class PlantillaDocumento : IAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoDocumentoPlantilla TipoDocumento { get; set; }

    /// <summary>
    /// Nombre del tipo cuando TipoDocumento == Otros (ej: "Acta de entrega"). Reutilizable entre plantillas.
    /// </summary>
    public string? TipoPersonalizado { get; set; }

    /// <summary>
    /// Texto a mostrar para el tipo (el personalizado si es "Otros").
    /// </summary>
    public string TipoDescripcion =>
        TipoDocumento == TipoDocumentoPlantilla.Otros && !string.IsNullOrWhiteSpace(TipoPersonalizado)
            ? TipoPersonalizado!
            : TipoDocumento switch
            {
                TipoDocumentoPlantilla.Especifico => "Específico",
                _ => TipoDocumento.ToString()
            };
    public RolPlantilla Rol { get; set; }
    public byte[] Archivo { get; set; } = Array.Empty<byte>();
    public string NombreArchivo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }
    public bool RevisadoPorLegal { get; set; }

    /// <summary>
    /// Extensiones admitidas para el archivo de la plantilla: Word, PDF, Excel, imágenes y texto.
    /// </summary>
    public static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".doc", ".docx", ".pdf", ".xls", ".xlsx",
        ".png", ".jpg", ".jpeg", ".gif", ".bmp",
        ".txt"
    };
}
