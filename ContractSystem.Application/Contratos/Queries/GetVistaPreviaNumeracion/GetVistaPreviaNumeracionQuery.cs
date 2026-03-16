using MediatR;

namespace ContractSystem.Application.Contratos.Queries.GetVistaPreviaNumeracion;

public record GetVistaPreviaNumeracionQuery(
    string Formato,
    int DigitosPadding,
    bool ContadorPorAnio) : IRequest<string>;

public class GetVistaPreviaNumeracionQueryHandler : IRequestHandler<GetVistaPreviaNumeracionQuery, string>
{
    private readonly IDocumentoNumeracionService _numeracionService;

    public GetVistaPreviaNumeracionQueryHandler(IDocumentoNumeracionService numeracionService)
    {
        _numeracionService = numeracionService;
    }

    public Task<string> Handle(GetVistaPreviaNumeracionQuery request, CancellationToken cancellationToken)
    {
        return _numeracionService.VistaPrevia(request.Formato, request.DigitosPadding, request.ContadorPorAnio, cancellationToken);
    }
}
