using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.CreateContrato;

public record CreateContratoCommand(
    TipoDocumentoContrato TipoDocumento,
    RolContrato Rol,
    string Numero,
    string Objeto,
    DateTime? FechaFirma,
    DateTime? FechaEntradaVigor,
    DateTime? FechaVigencia,
    string? Duracion,
    int? MiEmpresaId,
    int? TerceroId,
    int? ContratoPadreId,
    decimal? ValorTotal,
    string? CondicionesEntrega,
    string? CostosAsociados) : IRequest<Contrato>;

public class CreateContratoCommandHandler : IRequestHandler<CreateContratoCommand, Contrato>
{
    private readonly IContratoStore _store;
    private readonly IContratoValidationService _validation;

    public CreateContratoCommandHandler(IContratoStore store, IContratoValidationService validation)
    {
        _store = store;
        _validation = validation;
    }

    public async Task<Contrato> Handle(CreateContratoCommand request, CancellationToken cancellationToken)
    {
        // Validar número único (R06)
        var resultNumero = await _validation.ValidarNumeroUnicoAsync(request.Numero, cancellationToken: cancellationToken);
        if (!resultNumero.EsValido)
            throw new InvalidOperationException(resultNumero.MensajeError);

        var contrato = new Contrato
        {
            TipoDocumento = request.TipoDocumento,
            Rol = request.Rol,
            Numero = request.Numero.Trim(),
            Objeto = request.Objeto?.Trim() ?? string.Empty,
            Estado = EstadoContrato.Borrador,
            FechaFirma = request.FechaFirma,
            FechaEntradaVigor = request.FechaEntradaVigor,
            FechaVigencia = request.FechaVigencia,
            Duracion = request.Duracion?.Trim(),
            MiEmpresaId = request.MiEmpresaId,
            TerceroId = request.TerceroId,
            ContratoPadreId = request.ContratoPadreId,
            ValorTotal = request.ValorTotal,
            CondicionesEntrega = request.CondicionesEntrega?.Trim(),
            CostosAsociados = request.CostosAsociados?.Trim()
        };

        return await _store.CreateAsync(contrato, cancellationToken);
    }
}
