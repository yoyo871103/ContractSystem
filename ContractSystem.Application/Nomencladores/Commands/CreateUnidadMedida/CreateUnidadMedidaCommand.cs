using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.CreateUnidadMedida;

public record CreateUnidadMedidaCommand(string NombreCorto, string Descripcion) : IRequest<UnidadMedida>;

public class CreateUnidadMedidaCommandHandler : IRequestHandler<CreateUnidadMedidaCommand, UnidadMedida>
{
    private readonly IUnidadMedidaStore _store;

    public CreateUnidadMedidaCommandHandler(IUnidadMedidaStore store)
    {
        _store = store;
    }

    public async Task<UnidadMedida> Handle(CreateUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        var entity = new UnidadMedida
        {
            NombreCorto = request.NombreCorto.Trim(),
            Descripcion = request.Descripcion?.Trim() ?? string.Empty
        };
        return await _store.CreateAsync(entity, cancellationToken);
    }
}
