namespace ContractSystem.Domain.Nomencladores;

/// <summary>
/// Tipo de documento al que aplica una plantilla.
/// </summary>
public enum TipoDocumentoPlantilla
{
    Marco = 0,
    Especifico = 1,
    Independiente = 2,
    Suplemento = 3,

    /// <summary>
    /// Tipo personalizado: el nombre concreto se guarda en PlantillaDocumento.TipoPersonalizado.
    /// </summary>
    Otros = 4
}
