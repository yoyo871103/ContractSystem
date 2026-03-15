using ContractSystem.Domain;

namespace ContractSystem.Domain.Nomencladores;

/// <summary>
/// Unidad de medida (kg, litros, unidades, etc.). Nomenclador.
/// </summary>
public sealed class UnidadMedida : ISoftDeletable
{
    public int Id { get; set; }
    public string NombreCorto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Indica si la unidad está eliminada (soft delete).
    /// </summary>
    public bool IsDeleted => DeletedAt != null;
}
