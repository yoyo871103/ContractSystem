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
    bool RevisadoPorLegal,
    string? TipoPersonalizado = null) : IRequest<Unit>;

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

        var tipoPersonalizado = PlantillaValidacion.Validar(
            request.Nombre, request.TipoDocumento, request.TipoPersonalizado,
            request.NuevoArchivo is not null ? request.NuevoNombreArchivo : null);

        entity.Nombre = request.Nombre.Trim();
        entity.TipoDocumento = request.TipoDocumento;
        entity.TipoPersonalizado = tipoPersonalizado;
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
