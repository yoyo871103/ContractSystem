using MediatR;
using ContractSystem.Domain.Business;

namespace ContractSystem.Application.Business.Queries.GetBusinessInfo;

public record GetBusinessInfoQuery : IRequest<BusinessInfo?>;

public class GetBusinessInfoQueryHandler : IRequestHandler<GetBusinessInfoQuery, BusinessInfo?>
{
    private readonly IBusinessInfoStore _store;

    public GetBusinessInfoQueryHandler(IBusinessInfoStore store)
    {
        _store = store;
    }

    public async Task<BusinessInfo?> Handle(GetBusinessInfoQuery request, CancellationToken cancellationToken)
    {
        return await _store.GetAsync(cancellationToken);
    }
}
