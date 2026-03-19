using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.CrearFactura;

public record CrearFacturaCommand(
    int ContratoId,
    string Numero,
    DateTime Fecha,
    decimal ImporteTotal,
    string Descripcion) : IRequest<Factura>;

public class CrearFacturaCommandHandler : IRequestHandler<CrearFacturaCommand, Factura>
{
    private readonly IFacturaStore _facturaStore;
    private readonly IContratoStore _contratoStore;

    public CrearFacturaCommandHandler(IFacturaStore facturaStore, IContratoStore contratoStore)
    {
        _facturaStore = facturaStore;
        _contratoStore = contratoStore;
    }

    public async Task<Factura> Handle(CrearFacturaCommand request, CancellationToken cancellationToken)
    {
        var contrato = await _contratoStore.GetByIdAsync(request.ContratoId, cancellationToken: cancellationToken);
        if (contrato is null)
            throw new InvalidOperationException("El contrato no existe.");

        if (contrato.TipoDocumento == TipoDocumentoContrato.Marco)
            throw new InvalidOperationException("No se pueden asociar facturas a un Contrato Marco.");

        var factura = new Factura
        {
            ContratoId = request.ContratoId,
            Numero = request.Numero.Trim(),
            Fecha = request.Fecha,
            ImporteTotal = request.ImporteTotal,
            Descripcion = request.Descripcion.Trim()
        };

        await _facturaStore.CreateAsync(factura, cancellationToken);
        return factura;
    }
}
