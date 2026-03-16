namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Archivo adjunto asociado a un contrato o suplemento.
/// </summary>
public class DocumentoAdjunto : IAuditable
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Objetivo { get; set; } = string.Empty;
    public byte[] Contenido { get; set; } = Array.Empty<byte>();
    public long TamanioBytes { get; set; }
    public DateTime FechaCarga { get; set; }
    public int? UsuarioCargaId { get; set; }

    // --- Auditoría (IAuditable) ---
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string? ModificadoPor { get; set; }

    // FK al contrato (tabla única, incluye suplementos)
    public int ContratoId { get; set; }
    public Contrato Contrato { get; set; } = null!;

    /// <summary>
    /// Extensiones permitidas para adjuntos.
    /// </summary>
    public static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".gif",
        ".doc", ".docx", ".xls", ".xlsx", ".txt"
    };

    /// <summary>
    /// Extensiones explícitamente prohibidas (ejecutables).
    /// </summary>
    public static readonly HashSet<string> ExtensionesProhibidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".sh", ".cmd", ".com", ".msi", ".ps1", ".vbs", ".js", ".wsf"
    };
}
