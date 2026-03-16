using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.CreateTercero;

public record ContactoTerceroDto(string Nombre, string Cargo, string Email, string Telefono);

public record CreateTerceroCommand(
    string Nombre,
    string RazonSocial,
    string NifCif,
    string Direccion,
    string Telefono,
    string Email,
    TipoTercero Tipo,
    IReadOnlyList<ContactoTerceroDto> Contactos) : IRequest<Tercero>;

public class CreateTerceroCommandHandler : IRequestHandler<CreateTerceroCommand, Tercero>
{
    private readonly ITerceroStore _store;

    public CreateTerceroCommandHandler(ITerceroStore store)
    {
        _store = store;
    }

    public async Task<Tercero> Handle(CreateTerceroCommand request, CancellationToken cancellationToken)
    {
        var entity = new Tercero
        {
            Nombre = request.Nombre.Trim(),
            RazonSocial = request.RazonSocial?.Trim() ?? string.Empty,
            NifCif = request.NifCif?.Trim() ?? string.Empty,
            Direccion = request.Direccion?.Trim() ?? string.Empty,
            Telefono = request.Telefono?.Trim() ?? string.Empty,
            Email = request.Email?.Trim() ?? string.Empty,
            Tipo = request.Tipo,
            Contactos = request.Contactos.Select(c => new ContactoTercero
            {
                Nombre = c.Nombre.Trim(),
                Cargo = c.Cargo?.Trim() ?? string.Empty,
                Email = c.Email?.Trim() ?? string.Empty,
                Telefono = c.Telefono?.Trim() ?? string.Empty
            }).ToList()
        };

        return await _store.CreateAsync(entity, cancellationToken);
    }
}
