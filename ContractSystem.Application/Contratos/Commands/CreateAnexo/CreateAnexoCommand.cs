using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.CreateAnexo;

public record CreateAnexoCommand(
    int ContratoId,
    string Nombre,
    string? CondicionesEntrega,
    string? CostosAsociados,
    int Orden) : IRequest<Anexo>;

public class CreateAnexoCommandHandler : IRequestHandler<CreateAnexoCommand, Anexo>
{
    private readonly IAnexoStore _store;

    public CreateAnexoCommandHandler(IAnexoStore store) => _store = store;

    public Task<Anexo> Handle(CreateAnexoCommand request, CancellationToken cancellationToken)
    {
        var anexo = new Anexo
        {
            ContratoId = request.ContratoId,
            Nombre = request.Nombre.Trim(),
            CondicionesEntrega = request.CondicionesEntrega?.Trim(),
            CostosAsociados = request.CostosAsociados?.Trim(),
            Orden = request.Orden
        };
        return _store.CreateAsync(anexo, cancellationToken);
    }
}
