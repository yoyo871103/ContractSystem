using ContractSystem.Domain.Business;

namespace ContractSystem.Application.Business;

public interface IBusinessInfoStore
{
    /// <summary>Obtiene los datos del negocio, incluidos representantes y cuentas bancarias.</summary>
    Task<BusinessInfo?> GetAsync(CancellationToken cancellationToken = default);

    /// <param name="actualizarDatosLegales">
    /// Si es true, reemplaza REEUP, registro, constitución, representantes y cuentas por los de <paramref name="businessInfo"/>.
    /// </param>
    Task UpdateAsync(BusinessInfo businessInfo, bool actualizarDatosLegales = false, CancellationToken cancellationToken = default);
}
