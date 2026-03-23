using ContractSystem.Domain.Business;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Documento contractual: puede ser Marco, Específico, Independiente o Suplemento.
/// Todos comparten la misma estructura base; el campo TipoDocumento discrimina el comportamiento.
/// Modelados en una sola tabla para simplificar las relaciones M:N de modificación.
/// </summary>
public sealed class Contrato : ISoftDeletable, IAuditable
{
    public int Id { get; set; }

    /// <summary>
    /// Número del documento. Generado automáticamente pero editable por el usuario.
    /// Debe ser único (R06).
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Texto libre que explica el propósito del documento.
    /// </summary>
    public string Objeto { get; set; } = string.Empty;

    public TipoDocumentoContrato TipoDocumento { get; set; }
    public RolContrato Rol { get; set; }
    public EstadoContrato Estado { get; set; } = EstadoContrato.Borrador;

    // --- Fechas ---
    public DateTime? FechaFirma { get; set; }
    public DateTime? FechaEntradaVigor { get; set; }
    public DateTime? FechaVigencia { get; set; }

    /// <summary>
    /// Duración en texto libre (ej: "12 meses", "Indefinido").
    /// </summary>
    public string? Duracion { get; set; }

    // --- Ejecución ---
    public bool Ejecutado { get; set; }
    public DateTime? FechaEjecucion { get; set; }

    // --- Partes involucradas ---

    /// <summary>
    /// Mi empresa (selección del catálogo BusinessInfo).
    /// </summary>
    public int? MiEmpresaId { get; set; }
    public BusinessInfo? MiEmpresa { get; set; }

    /// <summary>
    /// Contraparte: cliente o proveedor (del catálogo Terceros).
    /// </summary>
    public int? TerceroId { get; set; }
    public Tercero? Tercero { get; set; }

    // --- Jerarquía ---

    /// <summary>
    /// Para Específicos: el Marco padre.
    /// Para Suplementos: el contrato padre (Marco, Específico o Independiente).
    /// Null para Marco e Independiente.
    /// </summary>
    public int? ContratoPadreId { get; set; }
    public Contrato? ContratoPadre { get; set; }

    /// <summary>
    /// Documentos hijos (Específicos de un Marco, o Suplementos de cualquier contrato).
    /// </summary>
    public ICollection<Contrato> Hijos { get; set; } = new List<Contrato>();

    // --- Valores económicos (opcionales) ---
    public decimal? ValorTotal { get; set; }
    public string? CondicionesEntrega { get; set; }
    public string? CostosAsociados { get; set; }

    // --- Solo para Suplementos ---

    /// <summary>
    /// Indica si el suplemento modifica condiciones generales del Marco (R09).
    /// Solo aplica cuando TipoDocumento == Suplemento y el padre es Marco.
    /// </summary>
    public bool EsModificacionGenerales { get; set; }

    // --- Relaciones M:N de modificación ---

    /// <summary>
    /// Documentos que este documento modifica ("modifica a").
    /// </summary>
    public ICollection<ModificacionDocumento> ModificaA { get; set; } = new List<ModificacionDocumento>();

    /// <summary>
    /// Documentos que modifican a este documento ("modificado por").
    /// </summary>
    public ICollection<ModificacionDocumento> ModificadoPor { get; set; } = new List<ModificacionDocumento>();

    // --- Anexos (secciones internas) ---
    public ICollection<Anexo> Anexos { get; set; } = new List<Anexo>();

    // --- Documentos adjuntos ---
    public ICollection<DocumentoAdjunto> DocumentosAdjuntos { get; set; } = new List<DocumentoAdjunto>();

    // --- Facturas ---
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    // --- Auditoría (IAuditable) ---
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }

    /// <summary>
    /// Usuario que realizó la última modificación (IAuditable).
    /// Nota: se usa propiedad explícita porque "ModificadoPor" es la nav de ModificacionDocumento.
    /// </summary>
    string? IAuditable.ModificadoPor
    {
        get => ModificadoPorUsuario;
        set => ModificadoPorUsuario = value;
    }
    public string? ModificadoPorUsuario { get; set; }

    public int? UsuarioCreacionId { get; set; }

    // --- Soft delete ---
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt != null;
}
