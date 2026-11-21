using ContractSystem.Domain.Business;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Domain.DatosLegales;

/// <summary>
/// Persona que representa legalmente a una entidad al firmar contratos (director, gerente, apoderado…).
/// Pertenece a la entidad propia (<see cref="BusinessInfoId"/>) o a un tercero (<see cref="TerceroId"/>), nunca a ambos.
/// </summary>
public sealed class RepresentanteLegal
{
    public int Id { get; set; }

    public int? BusinessInfoId { get; set; }
    public BusinessInfo? BusinessInfo { get; set; }

    public int? TerceroId { get; set; }
    public Tercero? Tercero { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Cargo con el que comparece (ej: "Director General").</summary>
    public string Cargo { get; set; } = string.Empty;

    /// <summary>Número del carné de identidad.</summary>
    public string? CarneIdentidad { get; set; }

    /// <summary>Documento que acredita la representación (ej: "Resolución No. 15/2024 del Ministro de …").</summary>
    public string? DocumentoNombramiento { get; set; }

    public DateTime? FechaNombramiento { get; set; }

    /// <summary>Representante que se propone por defecto al llenar contratos.</summary>
    public bool EsPredeterminado { get; set; }
}
