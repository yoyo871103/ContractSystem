using ContractSystem.Domain.DatosLegales;

namespace ContractSystem.Domain.Business;

public class BusinessInfo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public byte[]? Logo { get; set; }
    public string Nit { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Eslogan { get; set; } = string.Empty;
    public string NombreDueno { get; set; } = string.Empty;

    // --- Datos legales (para llenar contratos) ---

    /// <summary>Código REEUP (Registro Estatal de Empresas y Unidades Presupuestadas).</summary>
    public string? CodigoReeup { get; set; }

    /// <summary>Inscripción en el Registro Mercantil (ej: "Tomo 5, Folio 23, Hoja 1234").</summary>
    public string? RegistroMercantil { get; set; }

    /// <summary>Documento de constitución (ej: "Resolución No. 45/2010 del Ministerio de …").</summary>
    public string? DocumentoConstitucion { get; set; }

    // --- Textos para contratos (null = usar el texto predeterminado) ---

    /// <summary>Plantilla del bloque de comparecencia, con marcadores {Campo}.</summary>
    public string? PlantillaComparecencia { get; set; }

    /// <summary>Plantilla del bloque de firma, con marcadores {Campo}.</summary>
    public string? PlantillaFirma { get; set; }

    public ICollection<RepresentanteLegal> Representantes { get; set; } = new List<RepresentanteLegal>();
    public ICollection<CuentaBancaria> CuentasBancarias { get; set; } = new List<CuentaBancaria>();
}
