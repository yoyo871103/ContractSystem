using System.Globalization;
using System.Text.RegularExpressions;
using ContractSystem.Domain.DatosLegales;

namespace ContractSystem.Application.LlenadoContratos;

/// <summary>Un dato que el usuario puede insertar con un clic.</summary>
public sealed record CampoInsertable(string Grupo, string Etiqueta, string Valor)
{
    public bool TieneValor => !string.IsNullOrWhiteSpace(Valor);
}

/// <summary>
/// Contexto con el que se rellena un bloque: la parte, el representante y la cuenta elegidos,
/// y cómo se denomina esa parte en el contrato (ej: "EL PRESTADOR").
/// </summary>
public sealed record ContextoParte(
    FichaParte Ficha,
    RepresentanteLegal? Representante,
    CuentaBancaria? Cuenta,
    string Denominacion);

/// <summary>
/// Campos insertables y bloques de texto (comparecencia y firma) con marcadores {Campo}.
/// </summary>
public static partial class BloquesContrato
{
    /// <summary>Lo que se inserta cuando falta un dato: una línea para completarlo a mano.</summary>
    public const string Hueco = "________";

    public const string ComparecenciaPredeterminada =
        "{Nombre}, con domicilio legal en {Direccion}, código REEUP {Reeup}, NIT {Nit}, " +
        "inscrita en el Registro Mercantil, {RegistroMercantil}, constituida mediante {DocumentoConstitucion}, " +
        "con cuenta bancaria No. {Cuenta} en {Moneda}, en la {Sucursal} del {Banco}, " +
        "representada en este acto por {Representante}, en su carácter de {Cargo}, " +
        "según {DocumentoNombramiento} de fecha {FechaNombramiento}, " +
        "que en lo adelante y a los efectos del presente contrato se denominará {Denominacion}.";

    public const string FirmaPredeterminada =
        "Por {Denominacion}:\n\n\n______________________________\n{Representante}\n{Cargo}\n{Nombre}";

    /// <summary>Marcadores disponibles en las plantillas, con su descripción (para la ayuda en pantalla).</summary>
    public static IReadOnlyList<(string Marcador, string Descripcion)> Marcadores { get; } = new[]
    {
        ("{Nombre}", "Nombre o razón social"),
        ("{Nit}", "NIT"),
        ("{Reeup}", "Código REEUP"),
        ("{Direccion}", "Domicilio legal"),
        ("{Telefono}", "Teléfono"),
        ("{Email}", "Correo electrónico"),
        ("{RegistroMercantil}", "Inscripción en el Registro Mercantil"),
        ("{DocumentoConstitucion}", "Documento de constitución"),
        ("{Representante}", "Nombre del representante"),
        ("{Cargo}", "Cargo del representante"),
        ("{CarneIdentidad}", "Carné de identidad del representante"),
        ("{DocumentoNombramiento}", "Documento de nombramiento"),
        ("{FechaNombramiento}", "Fecha de nombramiento"),
        ("{Cuenta}", "Número de cuenta"),
        ("{Moneda}", "Moneda de la cuenta"),
        ("{Banco}", "Banco"),
        ("{Sucursal}", "Sucursal"),
        ("{DireccionSucursal}", "Dirección de la sucursal"),
        ("{Titular}", "Titular de la cuenta"),
        ("{Denominacion}", "Cómo se denomina la parte (ej: EL PRESTADOR)"),
    };

    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-ES");

    public static string FormatearFecha(DateTime? fecha) =>
        fecha?.ToString("d 'de' MMMM 'de' yyyy", Cultura) ?? string.Empty;

    /// <summary>Valores de cada marcador (sin llaves) para una parte.</summary>
    public static IReadOnlyDictionary<string, string> Valores(ContextoParte ctx)
    {
        var f = ctx.Ficha;
        var r = ctx.Representante;
        var c = ctx.Cuenta;
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Nombre"] = f.Nombre,
            ["Nit"] = f.Nit,
            ["Reeup"] = f.CodigoReeup,
            ["Direccion"] = f.Direccion,
            ["Telefono"] = f.Telefono,
            ["Email"] = f.Email,
            ["RegistroMercantil"] = f.RegistroMercantil,
            ["DocumentoConstitucion"] = f.DocumentoConstitucion,
            ["Representante"] = r?.Nombre ?? string.Empty,
            ["Cargo"] = r?.Cargo ?? string.Empty,
            ["CarneIdentidad"] = r?.CarneIdentidad ?? string.Empty,
            ["DocumentoNombramiento"] = r?.DocumentoNombramiento ?? string.Empty,
            ["FechaNombramiento"] = FormatearFecha(r?.FechaNombramiento),
            ["Cuenta"] = c?.Numero ?? string.Empty,
            ["Moneda"] = c?.Moneda ?? string.Empty,
            ["Banco"] = c?.Banco ?? string.Empty,
            ["Sucursal"] = c?.Sucursal ?? string.Empty,
            ["DireccionSucursal"] = c?.DireccionSucursal ?? string.Empty,
            ["Titular"] = c?.Titular ?? string.Empty,
            ["Denominacion"] = ctx.Denominacion,
        };
    }

    /// <summary>
    /// Sustituye los marcadores {Campo}. Un dato vacío se sustituye por <see cref="Hueco"/> para que
    /// se vea qué falta; un marcador desconocido se deja tal cual.
    /// </summary>
    public static string Renderizar(string plantilla, ContextoParte ctx)
    {
        var valores = Valores(ctx);
        return MarcadorRegex().Replace(plantilla, m =>
        {
            var clave = m.Groups[1].Value;
            if (!valores.TryGetValue(clave, out var valor))
                return m.Value;
            return string.IsNullOrWhiteSpace(valor) ? Hueco : valor.Trim();
        });
    }

    /// <summary>Datos sueltos de una parte, en el orden en que suelen pedirse en una proforma.</summary>
    public static IReadOnlyList<CampoInsertable> Campos(ContextoParte ctx)
    {
        var f = ctx.Ficha;
        var r = ctx.Representante;
        var c = ctx.Cuenta;

        var lista = new List<CampoInsertable>
        {
            new("Entidad", "Nombre", f.Nombre),
            new("Entidad", "Código REEUP", f.CodigoReeup),
            new("Entidad", "NIT", f.Nit),
            new("Entidad", "Domicilio legal", f.Direccion),
            new("Entidad", "Teléfono", f.Telefono),
            new("Entidad", "Correo", f.Email),
            new("Entidad", "Registro Mercantil", f.RegistroMercantil),
            new("Entidad", "Documento de constitución", f.DocumentoConstitucion),

            new("Representante", "Nombre", r?.Nombre ?? string.Empty),
            new("Representante", "Cargo", r?.Cargo ?? string.Empty),
            new("Representante", "Nombre y cargo", Unir(", ", r?.Nombre, r?.Cargo)),
            new("Representante", "Carné de identidad", r?.CarneIdentidad ?? string.Empty),
            new("Representante", "Documento de nombramiento", r?.DocumentoNombramiento ?? string.Empty),
            new("Representante", "Fecha de nombramiento", FormatearFecha(r?.FechaNombramiento)),

            new("Cuenta bancaria", "Número", c?.Numero ?? string.Empty),
            new("Cuenta bancaria", "Banco", c?.Banco ?? string.Empty),
            new("Cuenta bancaria", "Sucursal", c?.Sucursal ?? string.Empty),
            new("Cuenta bancaria", "Dirección de la sucursal", c?.DireccionSucursal ?? string.Empty),
            new("Cuenta bancaria", "Titular", c?.Titular ?? string.Empty),
            new("Cuenta bancaria", "Cuenta completa", CuentaCompleta(c)),
        };

        return lista;
    }

    /// <summary>"No. 0598… en CUP, Sucursal 1702 del BANDEC".</summary>
    public static string CuentaCompleta(CuentaBancaria? c)
    {
        if (c is null || string.IsNullOrWhiteSpace(c.Numero))
            return string.Empty;

        var texto = $"No. {c.Numero.Trim()}";
        if (!string.IsNullOrWhiteSpace(c.Moneda)) texto += $" en {c.Moneda.Trim()}";
        var dondeSucursal = Unir(" del ", c.Sucursal, c.Banco);
        if (!string.IsNullOrEmpty(dondeSucursal)) texto += $", {dondeSucursal}";
        return texto;
    }

    private static string Unir(string separador, params string?[] partes) =>
        string.Join(separador, partes.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()));

    [GeneratedRegex(@"\{(\w+)\}")]
    private static partial Regex MarcadorRegex();
}

/// <summary>
/// Pares de denominaciones habituales (parte que provee / parte que recibe).
/// </summary>
public sealed record ParDenominaciones(string Descripcion, string Proveedor, string Cliente)
{
    public static IReadOnlyList<ParDenominaciones> Habituales { get; } = new[]
    {
        new ParDenominaciones("Prestación de servicios", "EL PRESTADOR", "EL CLIENTE"),
        new ParDenominaciones("Compraventa", "EL VENDEDOR", "EL COMPRADOR"),
        new ParDenominaciones("Suministro", "EL SUMINISTRADOR", "EL CLIENTE"),
        new ParDenominaciones("Arrendamiento", "EL ARRENDADOR", "EL ARRENDATARIO"),
        new ParDenominaciones("Ejecución de obra", "EL EJECUTOR", "EL INVERSIONISTA"),
        new ParDenominaciones("Transportación", "EL TRANSPORTISTA", "EL CLIENTE"),
        new ParDenominaciones("Comisión", "EL COMISIONISTA", "EL COMITENTE"),
    };

    public override string ToString() => $"{Descripcion} ({Proveedor} / {Cliente})";
}
