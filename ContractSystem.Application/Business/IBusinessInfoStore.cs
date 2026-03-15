using ContractSystem.Domain.Business;

namespace ContractSystem.Application.Business;

public interface IBusinessInfoStore
{
    Task<BusinessInfo?> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(BusinessInfo businessInfo, CancellationToken cancellationToken = default);
}
