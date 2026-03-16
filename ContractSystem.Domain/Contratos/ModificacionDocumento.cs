namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Relación M:N de modificación entre documentos.
/// Registro de que un documento (origen/suplemento) modifica a otro (destino/contrato o suplemento).
/// Cada relación incluye una descripción textual de qué cambia específicamente.
/// </summary>
public sealed class ModificacionDocumento
{
    public int Id { get; set; }

    /// <summary>
    /// Documento que modifica (el suplemento o documento origen).
    /// </summary>
    public int DocumentoOrigenId { get; set; }
    public Contrato? DocumentoOrigen { get; set; }

    /// <summary>
    /// Documento que es modificado (el contrato o suplemento destino).
    /// </summary>
    public int DocumentoDestinoId { get; set; }
    public Contrato? DocumentoDestino { get; set; }

    /// <summary>
    /// Descripción textual de qué modifica específicamente.
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de registro de la relación.
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
