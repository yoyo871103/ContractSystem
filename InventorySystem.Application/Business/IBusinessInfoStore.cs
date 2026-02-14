using InventorySystem.Domain.Business;

namespace InventorySystem.Application.Business;

public interface IBusinessInfoStore
{
    Task<BusinessInfo?> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(BusinessInfo businessInfo, CancellationToken cancellationToken = default);
}
