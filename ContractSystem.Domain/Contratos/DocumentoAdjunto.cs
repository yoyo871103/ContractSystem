namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Archivo adjunto asociado a un contrato, suplemento, factura o anexo.
/// Siempre cuelga de un Contrato (ContratoId). Si además se asocia a una
/// factura o a un anexo concreto, FacturaId / AnexoId apuntan a la fila correspondiente
/// (como máximo uno de los dos).
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
    /// Si está informado, el adjunto pertenece a una factura concreta del contrato.
    /// Si es null, el adjunto es del contrato (o suplemento) en general.
    /// </summary>
    public int? FacturaId { get; set; }
    public Factura? Factura { get; set; }

    /// <summary>
    /// Si está informado, el adjunto pertenece a un anexo concreto del contrato.
    /// </summary>
    public int? AnexoId { get; set; }
    public Anexo? Anexo { get; set; }

    /// <summary>
    /// Extensiones permitidas para adjuntos.
    /// </summary>
    public static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".bmp",
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
