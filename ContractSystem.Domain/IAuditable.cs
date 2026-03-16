namespace ContractSystem.Domain;

/// <summary>
/// Marca entidades que registran quién las creó y modificó.
/// Los campos se rellenan automáticamente en SaveChanges.
/// </summary>
public interface IAuditable
{
    DateTime FechaCreacion { get; set; }
    string? CreadoPor { get; set; }
    DateTime? FechaModificacion { get; set; }
    string? ModificadoPor { get; set; }
}
