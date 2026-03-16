using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Application.Nomencladores.Commands.CreatePlantillaDocumento;

public record CreatePlantillaDocumentoCommand(
    string Nombre,
    TipoDocumentoPlantilla TipoDocumento,
    RolPlantilla Rol,
    byte[] Archivo,
    string NombreArchivo,
    bool RevisadoPorLegal) : IRequest<PlantillaDocumento>;

public class CreatePlantillaDocumentoCommandHandler : IRequestHandler<CreatePlantillaDocumentoCommand, PlantillaDocumento>
{
    private readonly IPlantillaDocumentoStore _store;

    public CreatePlantillaDocumentoCommandHandler(IPlantillaDocumentoStore store)
    {
        _store = store;
    }

    public async Task<PlantillaDocumento> Handle(CreatePlantillaDocumentoCommand request, CancellationToken cancellationToken)
    {
        var entity = new PlantillaDocumento
        {
            Nombre = request.Nombre.Trim(),
            TipoDocumento = request.TipoDocumento,
            Rol = request.Rol,
            Archivo = request.Archivo,
            NombreArchivo = request.NombreArchivo,
            FechaCreacion = DateTime.UtcNow,
            RevisadoPorLegal = request.RevisadoPorLegal
        };
        return await _store.CreateAsync(entity, cancellationToken);
    }
}
