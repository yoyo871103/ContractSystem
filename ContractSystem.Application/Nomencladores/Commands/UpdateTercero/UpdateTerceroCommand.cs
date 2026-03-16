using ContractSystem.Domain.Nomencladores;
using ContractSystem.Application.Nomencladores.Commands.CreateTercero;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.UpdateTercero;

public record UpdateTerceroCommand(
    int Id,
    string? Codigo,
    string Nombre,
    string RazonSocial,
    string NifCif,
    string Direccion,
    string Telefono,
    string Email,
    TipoTercero Tipo,
    string? UbicacionExpediente,
    IReadOnlyList<ContactoTerceroDto> Contactos) : IRequest<Unit>;

public class UpdateTerceroCommandHandler : IRequestHandler<UpdateTerceroCommand, Unit>
{
    private readonly ITerceroStore _store;

    public UpdateTerceroCommandHandler(ITerceroStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(UpdateTerceroCommand request, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(request.Id, includeContactos: true, cancellationToken);
        if (entity is null)
            throw new InvalidOperationException($"Tercero con Id {request.Id} no encontrado.");

        entity.Codigo = request.Codigo?.Trim();
        entity.Nombre = request.Nombre.Trim();
        entity.RazonSocial = request.RazonSocial?.Trim() ?? string.Empty;
        entity.NifCif = request.NifCif?.Trim() ?? string.Empty;
        entity.Direccion = request.Direccion?.Trim() ?? string.Empty;
        entity.Telefono = request.Telefono?.Trim() ?? string.Empty;
        entity.Email = request.Email?.Trim() ?? string.Empty;
        entity.Tipo = request.Tipo;
        entity.UbicacionExpediente = request.UbicacionExpediente?.Trim();

        // Reemplazar contactos
        entity.Contactos.Clear();
        foreach (var c in request.Contactos)
        {
            entity.Contactos.Add(new ContactoTercero
            {
                TerceroId = entity.Id,
                Nombre = c.Nombre.Trim(),
                Cargo = c.Cargo?.Trim() ?? string.Empty,
                Email = c.Email?.Trim() ?? string.Empty,
                Telefono = c.Telefono?.Trim() ?? string.Empty
            });
        }

        await _store.UpdateAsync(entity, cancellationToken);
        return Unit.Value;
    }
}
