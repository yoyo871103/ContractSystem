using ContractSystem.Domain.DatosLegales;

namespace ContractSystem.Application.DatosLegales;

public record RepresentanteLegalDto(
    string Nombre,
    string Cargo,
    string? CarneIdentidad,
    string? DocumentoNombramiento,
    DateTime? FechaNombramiento,
    bool EsPredeterminado);

public record CuentaBancariaDto(
    string Numero,
    string Moneda,
    string Banco,
    string? Sucursal,
    string? DireccionSucursal,
    string? Titular,
    bool EsPredeterminada);

/// <summary>
/// Datos legales de una entidad (propia o tercero) que se usan para llenar contratos.
/// </summary>
public record DatosLegalesDto(
    string? CodigoReeup,
    string? RegistroMercantil,
    string? DocumentoConstitucion,
    IReadOnlyList<RepresentanteLegalDto> Representantes,
    IReadOnlyList<CuentaBancariaDto> Cuentas);

public static class DatosLegalesMapper
{
    /// <summary>
    /// Crea los representantes a partir de los DTO, descartando filas sin nombre y
    /// garantizando un único predeterminado (el primero si no se marcó ninguno).
    /// </summary>
    public static List<RepresentanteLegal> CrearRepresentantes(DatosLegalesDto dto)
    {
        var lista = dto.Representantes
            .Where(r => !string.IsNullOrWhiteSpace(r.Nombre))
            .Select(r => new RepresentanteLegal
            {
                Nombre = r.Nombre.Trim(),
                Cargo = r.Cargo?.Trim() ?? string.Empty,
                CarneIdentidad = Limpiar(r.CarneIdentidad),
                DocumentoNombramiento = Limpiar(r.DocumentoNombramiento),
                FechaNombramiento = r.FechaNombramiento,
                EsPredeterminado = r.EsPredeterminado
            })
            .ToList();

        var predeterminado = lista.FirstOrDefault(r => r.EsPredeterminado) ?? lista.FirstOrDefault();
        foreach (var r in lista)
            r.EsPredeterminado = ReferenceEquals(r, predeterminado);

        return lista;
    }

    /// <summary>
    /// Crea las cuentas a partir de los DTO, descartando filas sin número y
    /// garantizando una única cuenta predeterminada por moneda.
    /// </summary>
    public static List<CuentaBancaria> CrearCuentas(DatosLegalesDto dto)
    {
        var lista = dto.Cuentas
            .Where(c => !string.IsNullOrWhiteSpace(c.Numero))
            .Select(c => new CuentaBancaria
            {
                Numero = c.Numero.Trim(),
                Moneda = string.IsNullOrWhiteSpace(c.Moneda) ? "CUP" : c.Moneda.Trim().ToUpperInvariant(),
                Banco = c.Banco?.Trim() ?? string.Empty,
                Sucursal = Limpiar(c.Sucursal),
                DireccionSucursal = Limpiar(c.DireccionSucursal),
                Titular = Limpiar(c.Titular),
                EsPredeterminada = c.EsPredeterminada
            })
            .ToList();

        foreach (var grupo in lista.GroupBy(c => c.Moneda))
        {
            var predeterminada = grupo.FirstOrDefault(c => c.EsPredeterminada) ?? grupo.First();
            foreach (var c in grupo)
                c.EsPredeterminada = ReferenceEquals(c, predeterminada);
        }

        return lista;
    }

    public static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    public static DatosLegalesDto DesdeEntidad(
        string? codigoReeup,
        string? registroMercantil,
        string? documentoConstitucion,
        IEnumerable<RepresentanteLegal> representantes,
        IEnumerable<CuentaBancaria> cuentas) =>
        new(
            codigoReeup,
            registroMercantil,
            documentoConstitucion,
            representantes.Select(r => new RepresentanteLegalDto(
                r.Nombre, r.Cargo, r.CarneIdentidad, r.DocumentoNombramiento, r.FechaNombramiento, r.EsPredeterminado)).ToList(),
            cuentas.Select(c => new CuentaBancariaDto(
                c.Numero, c.Moneda, c.Banco, c.Sucursal, c.DireccionSucursal, c.Titular, c.EsPredeterminada)).ToList());
}
