using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.ActualizarFactura;

public record ActualizarFacturaCommand(
    int Id,
    string Numero,
    DateTime Fecha,
    decimal ImporteTotal,
    string Descripcion) : IRequest;

public class ActualizarFacturaCommandHandler : IRequestHandler<ActualizarFacturaCommand>
{
    private readonly IFacturaStore _store;

    public ActualizarFacturaCommandHandler(IFacturaStore store) => _store = store;

    public async Task Handle(ActualizarFacturaCommand request, CancellationToken cancellationToken)
    {
        var factura = await _store.GetByIdAsync(request.Id, cancellationToken);
        if (factura is null)
            throw new InvalidOperationException("La factura no existe.");

        factura.Numero = request.Numero.Trim();
        factura.Fecha = request.Fecha;
        factura.ImporteTotal = request.ImporteTotal;
        factura.Descripcion = request.Descripcion.Trim();

        await _store.UpdateAsync(factura, cancellationToken);
    }
}
