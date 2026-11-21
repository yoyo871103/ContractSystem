using ContractSystem.Domain.Business;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Domain.DatosLegales;

/// <summary>
/// Cuenta bancaria de una entidad. Pertenece a la entidad propia (<see cref="BusinessInfoId"/>)
/// o a un tercero (<see cref="TerceroId"/>), nunca a ambos.
/// </summary>
public sealed class CuentaBancaria
{
    public int Id { get; set; }

    public int? BusinessInfoId { get; set; }
    public BusinessInfo? BusinessInfo { get; set; }

    public int? TerceroId { get; set; }
    public Tercero? Tercero { get; set; }

    public string Numero { get; set; } = string.Empty;

    /// <summary>Moneda de la cuenta (CUP, MLC, USD, EUR…).</summary>
    public string Moneda { get; set; } = "CUP";

    /// <summary>Banco (ej: "BANDEC", "BPA", "BANMET").</summary>
    public string Banco { get; set; } = string.Empty;

    /// <summary>Sucursal (ej: "Sucursal 1702").</summary>
    public string? Sucursal { get; set; }

    public string? DireccionSucursal { get; set; }

    /// <summary>Titular, si difiere del nombre de la entidad.</summary>
    public string? Titular { get; set; }

    /// <summary>Cuenta que se propone por defecto para su moneda.</summary>
    public bool EsPredeterminada { get; set; }
}
