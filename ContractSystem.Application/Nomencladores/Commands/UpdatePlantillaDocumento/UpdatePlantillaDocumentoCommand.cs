using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.UpdatePlantillaDocumento;

public record UpdatePlantillaDocumentoCommand(
    int Id,
    string Nombre,
    TipoDocumentoPlantilla TipoDocumento,
    RolPlantilla Rol,
    byte[]? NuevoArchivo,
    string? NuevoNombreArchivo,
    bool RevisadoPorLegal) : IRequest<Unit>;

public class UpdatePlantillaDocumentoCommandHandler : IRequestHandler<UpdatePlantillaDocumentoCommand, Unit>
{
    private readonly IPlantillaDocumentoStore _store;

    public UpdatePlantillaDocumentoCommandHandler(IPlantillaDocumentoStore store)
    {
        _store = store;
    }

    public async Task<Unit> Handle(UpdatePlantillaDocumentoCommand request, CancellationToken cancellationToken)
    {
        var entity = await _store.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            throw new InvalidOperationException($"Plantilla con Id {request.Id} no encontrada.");

        entity.Nombre = request.Nombre.Trim();
        entity.TipoDocumento = request.TipoDocumento;
        entity.Rol = request.Rol;
        entity.RevisadoPorLegal = request.RevisadoPorLegal;

        if (request.NuevoArchivo is not null && request.NuevoNombreArchivo is not null)
        {
            entity.Archivo = request.NuevoArchivo;
            entity.NombreArchivo = request.NuevoNombreArchivo;
        }

        await _store.UpdateAsync(entity, cancellationToken);
        return Unit.Value;
    }
}
