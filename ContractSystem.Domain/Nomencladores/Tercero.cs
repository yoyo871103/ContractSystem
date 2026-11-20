using ContractSystem.Domain.DatosLegales;

namespace ContractSystem.Domain.Nomencladores;

/// <summary>
/// Tercero: cliente, proveedor o ambos. Catálogo de contrapartes.
/// </summary>
public sealed class Tercero : ISoftDeletable, IAuditable
{
    public int Id { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string NifCif { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public TipoTercero Tipo { get; set; }
    /// <summary>
    /// Ubicación física del expediente del tercero (uso del área legal).
    /// </summary>
    public string? UbicacionExpediente { get; set; }

    // --- Datos legales (para llenar contratos) ---

    /// <summary>Código REEUP (Registro Estatal de Empresas y Unidades Presupuestadas).</summary>
    public string? CodigoReeup { get; set; }

    /// <summary>Inscripción en el Registro Mercantil (ej: "Tomo 5, Folio 23, Hoja 1234").</summary>
    public string? RegistroMercantil { get; set; }

    /// <summary>Documento de constitución (ej: "Resolución No. 45/2010 del Ministerio de …").</summary>
    public string? DocumentoConstitucion { get; set; }

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

    public ICollection<RepresentanteLegal> Representantes { get; set; } = new List<RepresentanteLegal>();
    public ICollection<CuentaBancaria> CuentasBancarias { get; set; } = new List<CuentaBancaria>();
}
