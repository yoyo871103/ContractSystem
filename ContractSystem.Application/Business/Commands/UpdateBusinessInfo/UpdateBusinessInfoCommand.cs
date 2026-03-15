using MediatR;
using ContractSystem.Domain.Business;

namespace ContractSystem.Application.Business.Commands.UpdateBusinessInfo;

public record UpdateBusinessInfoCommand(
    string Nombre,
    byte[]? Logo,
    string Nit,
    string Direccion,
    string Telefono,
    string Email,
    string Eslogan,
    string NombreDueno
) : IRequest<Unit>;

public class UpdateBusinessInfoCommandHandler : IRequestHandler<UpdateBusinessInfoCommand, Unit>
{
    private readonly IBusinessInfoStore _store;

    public UpdateBusinessInfoCommandHandler(IBusinessInfoStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(UpdateBusinessInfoCommand request, CancellationToken cancellationToken)
    {
        var businessInfo = await _store.GetAsync(cancellationToken) ?? new BusinessInfo();

        businessInfo.Nombre = request.Nombre;
        businessInfo.Logo = request.Logo;
        businessInfo.Nit = request.Nit;
        businessInfo.Direccion = request.Direccion;
        businessInfo.Telefono = request.Telefono;
        businessInfo.Email = request.Email;
        businessInfo.Eslogan = request.Eslogan;
        businessInfo.NombreDueno = request.NombreDueno;

        await _store.UpdateAsync(businessInfo, cancellationToken);

        return Unit.Value;
    }
}
