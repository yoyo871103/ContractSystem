using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Application.Contratos.Commands.CreateSuplemento;

public record ModificacionDto(int DocumentoDestinoId, string Descripcion);

public record CreateSuplementoCommand(
    int ContratoPadreId,
    RolContrato Rol,
    string Numero,
    string Objeto,
    bool EsModificacionGenerales,
    DateTime? FechaFirma,
    DateTime? FechaEntradaVigor,
    DateTime? FechaVigencia,
    string? Duracion,
    int? MiEmpresaId,
    int? TerceroId,
    decimal? ValorTotal,
    string? CondicionesEntrega,
    string? CostosAsociados,
    IReadOnlyList<ModificacionDto> Modificaciones) : IRequest<Contrato>;

public class CreateSuplementoCommandHandler : IRequestHandler<CreateSuplementoCommand, Contrato>
{
    private readonly IContratoStore _contratoStore;
    private readonly IModificacionDocumentoStore _modificacionStore;
    private readonly IContratoValidationService _validation;

    public CreateSuplementoCommandHandler(
        IContratoStore contratoStore,
        IModificacionDocumentoStore modificacionStore,
        IContratoValidationService validation)
    {
        _contratoStore = contratoStore;
        _modificacionStore = modificacionStore;
        _validation = validation;
    }

    public async Task<Contrato> Handle(CreateSuplementoCommand request, CancellationToken cancellationToken)
    {
        // Obtener el contrato padre
        var padre = await _contratoStore.GetByIdAsync(request.ContratoPadreId, cancellationToken: cancellationToken);
        if (padre is null)
            throw new InvalidOperationException("El contrato padre no existe.");

        // Validar R09: suplementos de Marco solo para modificaciones generales
        if (padre.TipoDocumento == TipoDocumentoContrato.Marco)
        {
            var r09 = _validation.ValidarSuplementoMarco(padre.TipoDocumento, request.EsModificacionGenerales);
            if (!r09.EsValido)
                throw new InvalidOperationException(r09.MensajeError);
        }

        // Validar número único (R06)
        var resultNumero = await _validation.ValidarNumeroUnicoAsync(request.Numero, cancellationToken: cancellationToken);
        if (!resultNumero.EsValido)
            throw new InvalidOperationException(resultNumero.MensajeError);

        // Crear el suplemento (es un Contrato con TipoDocumento=Suplemento)
        var suplemento = new Contrato
        {
            TipoDocumento = TipoDocumentoContrato.Suplemento,
            Rol = request.Rol,
            Numero = request.Numero.Trim(),
            Objeto = request.Objeto?.Trim() ?? string.Empty,
            Estado = EstadoContrato.Borrador,
            EsModificacionGenerales = request.EsModificacionGenerales,
            ContratoPadreId = request.ContratoPadreId,
            FechaFirma = request.FechaFirma,
            FechaEntradaVigor = request.FechaEntradaVigor,
            FechaVigencia = request.FechaVigencia,
            Duracion = request.Duracion?.Trim(),
            MiEmpresaId = request.MiEmpresaId,
            TerceroId = request.TerceroId,
            ValorTotal = request.ValorTotal,
            CondicionesEntrega = request.CondicionesEntrega?.Trim(),
            CostosAsociados = request.CostosAsociados?.Trim()
        };

        var creado = await _contratoStore.CreateAsync(suplemento, cancellationToken);

        // Crear relaciones "modifica a"
        foreach (var mod in request.Modificaciones)
        {
            // Validar R03: fechas
            var r03 = await _validation.ValidarFechasModificacionAsync(creado.Id, mod.DocumentoDestinoId, cancellationToken);
            if (!r03.EsValido)
                throw new InvalidOperationException(r03.MensajeError);

            // Validar R04: ciclos
            var r04 = await _validation.ValidarSinCiclosAsync(creado.Id, mod.DocumentoDestinoId, cancellationToken);
            if (!r04.EsValido)
                throw new InvalidOperationException(r04.MensajeError);

            await _modificacionStore.CreateAsync(new ModificacionDocumento
            {
                DocumentoOrigenId = creado.Id,
                DocumentoDestinoId = mod.DocumentoDestinoId,
                Descripcion = mod.Descripcion?.Trim() ?? string.Empty
            }, cancellationToken);
        }

        return creado;
    }
}
