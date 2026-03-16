namespace ContractSystem.Domain.Contratos;

/// <summary>
/// Estado del ciclo de vida de un contrato o suplemento.
/// </summary>
public enum EstadoContrato
{
    Borrador = 0,
    Vigente = 1,
    Vencido = 2,
    Rescindido = 3,
    Ejecutado = 4
}
