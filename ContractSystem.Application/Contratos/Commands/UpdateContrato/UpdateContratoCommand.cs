using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.UpdateContrato;

public record UpdateContratoCommand(
    int Id,
    string Numero,
    string Objeto,
    RolContrato Rol,
    DateTime? FechaFirma,
    DateTime? FechaEntradaVigor,
    DateTime? FechaVigencia,
    string? Duracion,
    int? MiEmpresaId,
    int? TerceroId,
    int? ContratoPadreId,
    decimal? ValorTotal,
    string? CondicionesEntrega,
    string? CostosAsociados) : IRequest<Unit>;

public class UpdateContratoCommandHandler : IRequestHandler<UpdateContratoCommand, Unit>
{
    private readonly IContratoStore _store;
    private readonly IContratoValidationService _validation;

    public UpdateContratoCommandHandler(IContratoStore store, IContratoValidationService validation)
    {
        _store = store;
        _validation = validation;
    }

    public async Task<Unit> Handle(UpdateContratoCommand request, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(request.Id, cancellationToken: cancellationToken);
        if (entity is null)
            throw new InvalidOperationException($"Contrato con Id {request.Id} no encontrado.");

        // Validar número único excluyendo el propio (R06)
        var resultNumero = await _validation.ValidarNumeroUnicoAsync(request.Numero, request.Id, cancellationToken);
        if (!resultNumero.EsValido)
            throw new InvalidOperationException(resultNumero.MensajeError);

        entity.Numero = request.Numero.Trim();
        entity.Objeto = request.Objeto?.Trim() ?? string.Empty;
        entity.Rol = request.Rol;
        entity.FechaFirma = request.FechaFirma;
        entity.FechaEntradaVigor = request.FechaEntradaVigor;
        entity.FechaVigencia = request.FechaVigencia;
        entity.Duracion = request.Duracion?.Trim();
        entity.MiEmpresaId = request.MiEmpresaId;
        entity.TerceroId = request.TerceroId;
        entity.ContratoPadreId = request.ContratoPadreId;
        entity.ValorTotal = request.ValorTotal;
        entity.CondicionesEntrega = request.CondicionesEntrega?.Trim();
        entity.CostosAsociados = request.CostosAsociados?.Trim();

        await _store.UpdateAsync(entity, cancellationToken);
        return Unit.Value;
    }
}
