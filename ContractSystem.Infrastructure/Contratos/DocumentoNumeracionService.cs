using ContractSystem.Application.Contratos;
using ContractSystem.Domain.Contratos;

namespace ContractSystem.Infrastructure.Contratos;

public sealed class DocumentoNumeracionService : IDocumentoNumeracionService
{
    private readonly IConfiguracionNumeracionStore _configuracionStore;
    private readonly IContratoStore _contratoStore;

    public DocumentoNumeracionService(
        IConfiguracionNumeracionStore configuracionStore,
        IContratoStore contratoStore)
    {
        _configuracionStore = configuracionStore;
        _contratoStore = contratoStore;
    }

    public async Task<string> GenerarNumeroAsync(
        TipoDocumentoContrato tipo,
        string? codigoTercero = null,
        CancellationToken cancellationToken = default)
    {
        var config = await _configuracionStore.GetActivaAsync(cancellationToken);
        if (config is null)
            throw new InvalidOperationException("No hay una configuración de numeración activa.");

        var anio = config.ContadorPorAnio ? (int?)DateTime.Now.Year : null;
        var siguienteNumero = await _configuracionStore.ObtenerSiguienteNumeroAsync(anio, cancellationToken);

        var numero = ResolverFormato(config.Formato, tipo, codigoTercero, siguienteNumero, config.DigitosPadding);

        // Verificar unicidad; si ya existe, intentar con el siguiente
        while (await _contratoStore.ExisteNumeroAsync(numero, cancellationToken: cancellationToken))
        {
            siguienteNumero = await _configuracionStore.ObtenerSiguienteNumeroAsync(anio, cancellationToken);
            numero = ResolverFormato(config.Formato, tipo, codigoTercero, siguienteNumero, config.DigitosPadding);
        }

        return numero;
    }

    public Task<string> VistaPrevia(
        string formato,
        int digitosPadding,
        bool contadorPorAnio,
        CancellationToken cancellationToken = default)
    {
        var numero = ResolverFormato(formato, TipoDocumentoContrato.Marco, "CLI001", 1, digitosPadding);
        return Task.FromResult(numero);
    }

    private static string ResolverFormato(
        string formato,
        TipoDocumentoContrato tipo,
        string? codigoTercero,
        int contador,
        int digitosPadding)
    {
        var now = DateTime.Now;

        var tipoTexto = tipo switch
        {
            TipoDocumentoContrato.Marco => "MARCO",
            TipoDocumentoContrato.Especifico => "ESPECIFICO",
            TipoDocumentoContrato.Independiente => "INDEP",
            TipoDocumentoContrato.Suplemento => "SUPL",
            _ => "DOC"
        };

        var resultado = formato
            .Replace("{YYYY}", now.Year.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{MM}", now.Month.ToString("D2"), StringComparison.OrdinalIgnoreCase)
            .Replace("{TIPO}", tipoTexto, StringComparison.OrdinalIgnoreCase)
            .Replace("{CODIGO_CLIENTE}", codigoTercero ?? "", StringComparison.OrdinalIgnoreCase)
            .Replace("{CONTADOR}", contador.ToString().PadLeft(digitosPadding, '0'), StringComparison.OrdinalIgnoreCase);

        return resultado;
    }
}
