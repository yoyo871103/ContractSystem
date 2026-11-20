using ContractSystem.Domain.Business;
using ContractSystem.Domain.DatosLegales;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Application.LlenadoContratos;

/// <summary>
/// Datos de una de las partes de un contrato (la entidad propia o un tercero), listos para insertar.
/// </summary>
public sealed record FichaParte(
    string Nombre,
    string Nit,
    string CodigoReeup,
    string Direccion,
    string Telefono,
    string Email,
    string RegistroMercantil,
    string DocumentoConstitucion,
    IReadOnlyList<RepresentanteLegal> Representantes,
    IReadOnlyList<CuentaBancaria> Cuentas)
{
    public static FichaParte DesdeEntidad(BusinessInfo b) => new(
        b.Nombre,
        b.Nit,
        b.CodigoReeup ?? string.Empty,
        b.Direccion,
        b.Telefono,
        b.Email,
        b.RegistroMercantil ?? string.Empty,
        b.DocumentoConstitucion ?? string.Empty,
        Ordenar(b.Representantes),
        Ordenar(b.CuentasBancarias));

    /// <summary>Para el tercero se usa la razón social si existe (es el nombre legal); si no, el nombre.</summary>
    public static FichaParte DesdeTercero(Tercero t) => new(
        string.IsNullOrWhiteSpace(t.RazonSocial) ? t.Nombre : t.RazonSocial,
        t.NifCif,
        t.CodigoReeup ?? string.Empty,
        t.Direccion,
        t.Telefono,
        t.Email,
        t.RegistroMercantil ?? string.Empty,
        t.DocumentoConstitucion ?? string.Empty,
        Ordenar(t.Representantes),
        Ordenar(t.CuentasBancarias));

    public RepresentanteLegal? RepresentantePredeterminado =>
        Representantes.FirstOrDefault(r => r.EsPredeterminado) ?? Representantes.FirstOrDefault();

    /// <summary>
    /// Cuenta propuesta para una moneda: la predeterminada de esa moneda, cualquiera de esa moneda,
    /// o en su defecto la primera cuenta disponible.
    /// </summary>
    public CuentaBancaria? CuentaPara(string? moneda)
    {
        var deMoneda = Cuentas.Where(c => string.Equals(c.Moneda, moneda, StringComparison.OrdinalIgnoreCase)).ToList();
        return deMoneda.FirstOrDefault(c => c.EsPredeterminada) ?? deMoneda.FirstOrDefault() ?? Cuentas.FirstOrDefault();
    }

    private static IReadOnlyList<RepresentanteLegal> Ordenar(IEnumerable<RepresentanteLegal> lista) =>
        lista.OrderByDescending(r => r.EsPredeterminado).ThenBy(r => r.Nombre).ToList();

    private static IReadOnlyList<CuentaBancaria> Ordenar(IEnumerable<CuentaBancaria> lista) =>
        lista.OrderBy(c => c.Moneda).ThenByDescending(c => c.EsPredeterminada).ToList();
}
