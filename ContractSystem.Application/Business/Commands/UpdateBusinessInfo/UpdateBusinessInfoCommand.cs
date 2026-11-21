using MediatR;
using ContractSystem.Application.DatosLegales;
using ContractSystem.Application.LlenadoContratos;
using ContractSystem.Domain.Business;

namespace ContractSystem.Application.Business.Commands.UpdateBusinessInfo;

/// <summary>Plantillas de los bloques de contrato; un texto vacío vuelve al predeterminado.</summary>
public record PlantillasBloquesDto(string? Comparecencia, string? Firma);

/// <param name="DatosLegales">Si es null, los datos legales existentes no se modifican.</param>
/// <param name="Plantillas">Si es null, las plantillas existentes no se modifican.</param>
public record UpdateBusinessInfoCommand(
    string Nombre,
    byte[]? Logo,
    string Nit,
    string Direccion,
    string Telefono,
    string Email,
    string Eslogan,
    string NombreDueno,
    DatosLegalesDto? DatosLegales = null,
    PlantillasBloquesDto? Plantillas = null
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

        var actualizarDatosLegales = request.DatosLegales is not null;
        if (request.DatosLegales is { } legales)
        {
            businessInfo.CodigoReeup = DatosLegalesMapper.Limpiar(legales.CodigoReeup);
            businessInfo.RegistroMercantil = DatosLegalesMapper.Limpiar(legales.RegistroMercantil);
            businessInfo.DocumentoConstitucion = DatosLegalesMapper.Limpiar(legales.DocumentoConstitucion);
            businessInfo.Representantes = DatosLegalesMapper.CrearRepresentantes(legales);
            businessInfo.CuentasBancarias = DatosLegalesMapper.CrearCuentas(legales);
        }

        if (request.Plantillas is { } plantillas)
        {
            businessInfo.PlantillaComparecencia = NormalizarPlantilla(plantillas.Comparecencia, BloquesContrato.ComparecenciaPredeterminada);
            businessInfo.PlantillaFirma = NormalizarPlantilla(plantillas.Firma, BloquesContrato.FirmaPredeterminada);
        }

        await _store.UpdateAsync(businessInfo, actualizarDatosLegales, cancellationToken);

        return Unit.Value;
    }

    /// <summary>Vacío o igual al predeterminado se guarda como null (así se beneficia de futuras mejoras del texto).</summary>
    private static string? NormalizarPlantilla(string? texto, string predeterminado)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var normalizado = texto.Replace("\r\n", "\n").Trim();
        return normalizado == predeterminado ? null : normalizado;
    }
}
