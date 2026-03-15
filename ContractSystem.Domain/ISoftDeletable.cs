namespace ContractSystem.Domain;

/// <summary>
/// Marca entidades que admiten borrado lógico (soft delete).
/// Cuando <see cref="DeletedAt"/> no es null, la entidad se considera eliminada y no debe mostrarse en consultas normales.
/// Otras entidades futuras pueden implementar esta interfaz para el mismo comportamiento.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Fecha y hora en que se marcó la entidad como eliminada, o null si sigue activa.
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }
}
