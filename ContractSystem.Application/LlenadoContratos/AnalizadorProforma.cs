using System.Text.RegularExpressions;

namespace ContractSystem.Application.LlenadoContratos;

/// <summary>Dato de una parte que se puede escribir en un hueco de la proforma.</summary>
public enum CampoContrato
{
    Nombre,
    Reeup,
    Nit,
    Direccion,
    Telefono,
    Email,
    RegistroMercantil,
    DocumentoConstitucion,
    Representante,
    Cargo,
    CarneIdentidad,
    DocumentoNombramiento,
    FechaNombramiento,
    Cuenta,
    Banco,
    Sucursal,
    DireccionSucursal,
    Titular
}

/// <summary>Lado de la relación contractual al que pertenece un hueco.</summary>
public enum LadoParte
{
    Desconocido,
    Proveedor,
    Cliente
}

/// <summary>
/// Párrafo del documento tal como lo entrega Word: posición inicial, texto (con su marca final) y,
/// si se conoce, la posición final según Word (en celdas de tabla puede no coincidir con la longitud del texto).
/// </summary>
public sealed record ParrafoDocumento(int Inicio, string Texto, bool EnTabla, int Fin = -1)
{
    public int FinReal => Fin >= 0 ? Fin : Inicio + Texto.Length;

    /// <summary>Texto sin la marca de fin de párrafo / celda (\r, \a).</summary>
    public string Contenido { get; } = Texto.TrimEnd('\r', '\n', '\a');
}

/// <summary>
/// Hueco reconocido en la proforma. <see cref="Inicio"/> es la posición absoluta en el documento;
/// si <see cref="Longitud"/> es 0 se inserta en ese punto (con <see cref="Prefijo"/>), si no se sustituye.
/// </summary>
public sealed record HuecoDetectado(
    int ParrafoIndice,
    int Inicio,
    int Longitud,
    string TextoHueco,
    string Prefijo,
    CampoContrato Campo,
    LadoParte Lado,
    string Etiqueta,
    string? Moneda,
    string Contexto);

public sealed record ResultadoAnalisis(
    IReadOnlyList<HuecoDetectado> Huecos,
    string? DenominacionProveedor,
    string? DenominacionCliente);

/// <summary>
/// Reconoce en el texto de una proforma los huecos a rellenar (líneas ____ o puntos ...., o etiquetas
/// "NIT:" sin valor) y decide a qué parte pertenecen según la sección en la que están
/// ("De una parte … EL PRESTADOR", "Por EL CLIENTE:", "Datos del cliente"…).
/// No depende de Word: recibe los párrafos y devuelve posiciones.
/// </summary>
public static partial class AnalizadorProforma
{
    private const RegexOptions Opciones = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    // ---------- Denominaciones de las partes ----------

    private static readonly string[] RolesProveedor =
    {
        "PRESTADOR", "PRESTADORA", "VENDEDOR", "VENDEDORA", "SUMINISTRADOR", "SUMINISTRADORA", "ARRENDADOR", "ARRENDADORA",
        "EJECUTOR", "EJECUTORA", "TRANSPORTISTA", "PORTEADOR", "COMISIONISTA", "CONTRATISTA", "PROVEEDOR", "PROVEEDORA",
        "DEPOSITARIO", "DEPOSITARIA", "CONSTRUCTOR", "CONSTRUCTORA", "PROYECTISTA", "ASEGURADOR", "ASEGURADORA",
        "AGENTE", "PRESTATARIO DEL SERVICIO", "PRODUCTOR", "PRODUCTORA", "EXPORTADOR", "EXPORTADORA"
    };

    private static readonly string[] RolesCliente =
    {
        "CLIENTE", "COMPRADOR", "COMPRADORA", "ARRENDATARIO", "ARRENDATARIA", "INVERSIONISTA", "COMITENTE",
        "CONTRATANTE", "BENEFICIARIO", "BENEFICIARIA", "DEPOSITANTE", "CARGADOR", "ASEGURADO", "ASEGURADA",
        "PRINCIPAL", "USUARIO", "USUARIA", "RECEPTOR", "RECEPTORA", "DESTINATARIO", "DESTINATARIA",
        "IMPORTADOR", "IMPORTADORA", "ADQUIRENTE"
    };

    private static readonly Regex RolRegex = new(
        @"\b(?<art>el|la)\s+(?<rol>" + string.Join("|", RolesProveedor.Concat(RolesCliente).OrderByDescending(r => r.Length)) + @")\b",
        Opciones);

    private static LadoParte LadoDeRol(string rol) =>
        RolesProveedor.Contains(rol.ToUpperInvariant()) ? LadoParte.Proveedor : LadoParte.Cliente;

    // ---------- Secciones ----------

    private enum TipoSeccion { Comparecencia, Firma, Datos, Cuerpo }

    private sealed record Ancla(int Parrafo, int Posicion, TipoSeccion Tipo, LadoParte Lado, string? Denominacion);

    [GeneratedRegex(@"\b(?:y\s+)?de\s+(?:una|otra|la\s+otra)\s+parte\b", Opciones)]
    private static partial Regex AnclaComparecenciaRegex();

    [GeneratedRegex(@"^\s*por\s+(?:el|la)\s+\p{L}", Opciones)]
    private static partial Regex AnclaFirmaRegex();

    [GeneratedRegex(@"\bdatos\s+(?:generales\s+)?de(?:l|\s+la)\s+\p{L}", Opciones)]
    private static partial Regex AnclaDatosRegex();

    /// <summary>Frases con las que termina la comparecencia y empieza el clausulado.</summary>
    [GeneratedRegex(@"\b(?:acuerdan|convienen|reconoci[eé]ndose|suscriben\s+el\s+presente|ambas\s+partes|cl[aá]usulas)\b", Opciones)]
    private static partial Regex FinComparecenciaRegex();

    /// <summary>"…que en lo adelante se denominará EL PRESTADOR".</summary>
    [GeneratedRegex(@"(?:denominar[aá]|denominad[oa]|en\s+lo\s+(?:adelante|sucesivo)|llamar[aá]|llamad[oa]|designad[oa])", Opciones)]
    private static partial Regex IntroDenominacionRegex();

    // ---------- Huecos y etiquetas ----------

    [GeneratedRegex(@"_{3,}|\.{4,}|…{2,}|…\.{2,}", Opciones)]
    private static partial Regex HuecoRegex();

    /// <summary>Lo que puede haber entre la etiqueta y el hueco: "No.", "en CUP", ":"…</summary>
    [GeneratedRegex(@"^\s*(?:(?:no|nro|n[uú]m(?:ero)?|n[oº°])\s*\.?\s*)?(?:(?:en|de)\s+(?:cup|mlc|usd|eur|cuc|moneda\s+nacional|divisas?)\s*)?[:\-–]?\s*$", Opciones)]
    private static partial Regex HolguraRegex();

    [GeneratedRegex(@"\b(CUP|MLC|USD|EUR|CUC)\b|moneda\s+nacional", Opciones)]
    private static partial Regex MonedaRegex();

    /// <summary>Hueco justo después de "De una parte:" → nombre de la entidad.</summary>
    [GeneratedRegex(@"de\s+(?:una|otra|la\s+otra)\s+parte\s*[:,\-–]?\s*(?:(?:la|el)\s+(?:empresa|entidad|sociedad|unidad|organizaci[oó]n)\s*)?[:,]?\s*$", Opciones)]
    private static partial Regex NombreTrasAnclaRegex();

    /// <summary>Etiquetas por campo, de la más específica a la más general.</summary>
    private static readonly (CampoContrato Campo, Regex Regex)[] Etiquetas =
    {
        (CampoContrato.DireccionSucursal, R(@"direcci[oó]n\s+de\s+la\s+(?:sucursal|agencia)|(?:sucursal|agencia)\s+sita\s+en")),
        (CampoContrato.Nombre, R(@"nombre\s+de\s+la\s+(?:entidad|empresa|sociedad|organizaci[oó]n)|raz[oó]n\s+social|denominaci[oó]n(?:\s+social)?|\bentidad|\bempresa")),
        (CampoContrato.Reeup, R(@"c[oó]digo\s+reeup|\breeup|c[oó]digo\s+de\s+(?:la\s+)?(?:entidad|empresa)")),
        (CampoContrato.Nit, R(@"\bnit\b|n[uú]mero\s+de\s+identificaci[oó]n\s+tributaria")),
        (CampoContrato.Direccion, R(@"domicilio(?:\s+(?:legal|social|fiscal))?(?:\s+(?:sito|situado|ubicado|radicado))?(?:\s+en)?|direcci[oó]n(?:\s+(?:legal|postal|social|particular))?(?:\s+en)?")),
        (CampoContrato.Telefono, R(@"tel[eé]fonos?|\btelfs?\.?|\btel\.|\bfax")),
        (CampoContrato.Email, R(@"correo(?:\s+electr[oó]nico)?|\be-?mail")),
        (CampoContrato.RegistroMercantil, R(@"registro\s+mercantil(?:\s*,)?")),
        (CampoContrato.DocumentoConstitucion, R(@"constituid[oa]\s+(?:mediante|por|seg[uú]n)(?:\s+(?:la|el))?(?:\s+(?:resoluci[oó]n|acuerdo|escritura(?:\s+p[uú]blica)?))?|(?:documento|resoluci[oó]n)\s+de\s+constituci[oó]n|cread[oa]\s+(?:mediante|por)(?:\s+(?:la|el))?(?:\s+(?:resoluci[oó]n|acuerdo))?")),
        (CampoContrato.DocumentoNombramiento, R(@"(?:documento|resoluci[oó]n)\s+de\s+nombramiento|nombrad[oa]\s+(?:mediante|por|seg[uú]n)(?:\s+(?:la|el))?(?:\s+(?:resoluci[oó]n|acuerdo))?|acreditad[oa]\s+(?:mediante|por|seg[uú]n)(?:\s+(?:la|el))?(?:\s+(?:resoluci[oó]n|acuerdo))?|(?:seg[uú]n|mediante|conforme\s+a)(?:\s+(?:la|el))?\s+(?:resoluci[oó]n|acuerdo|escritura(?:\s+p[uú]blica)?)")),
        (CampoContrato.FechaNombramiento, R(@"fecha\s+(?:de(?:l)?\s+)?nombramiento")),
        (CampoContrato.Representante, R(@"representad[oa]\s+(?:en\s+este\s+acto\s+)?por(?:\s+(?:el|la)\s+(?:compa[ñn]ero|compa[ñn]era|se[ñn]or|se[ñn]ora|c\.))?|representante(?:\s+legal)?|\bdirector(?:a)?(?:\s+general)?(?=\s*[:\-–])|\bgerente(?:\s+general)?(?=\s*[:\-–])")),
        (CampoContrato.Cargo, R(@"\bcargo|en\s+su\s+(?:car[aá]cter|condici[oó]n|calidad)\s+de|ostenta\s+el\s+cargo\s+de")),
        (CampoContrato.CarneIdentidad, R(@"carn[eé]\s+de\s+identidad|n[uú]mero\s+de\s+identidad(?:\s+permanente)?|documento\s+de\s+identidad|\bc\.\s?i\.|\bci(?=\s*[:\-–]|\s*(?:no|n[oº°])\b)")),
        (CampoContrato.Titular, R(@"titular(?:\s+de\s+la\s+cuenta)?|a\s+nombre\s+de")),
        (CampoContrato.Sucursal, R(@"\bsucursal(?:\s+bancaria)?|\bagencia(?:\s+bancaria)?")),
        (CampoContrato.Banco, R(@"\bbanco|instituci[oó]n\s+bancaria|entidad\s+bancaria")),
        (CampoContrato.Cuenta, R(@"\bcuenta(?:\s+bancaria)?(?:\s+corriente)?(?:\s+(?:en\s+)?(?:cup|mlc|usd|eur|cuc))?")),
    };

    /// <summary>"Nombre:" a secas: del representante en el bloque de firma o junto a "representante"; si no, de la entidad.</summary>
    private static readonly Regex NombreGenericoRegex = R(@"\bnombre(?:s)?(?:\s+y\s+apellidos?)?");

    /// <summary>"de fecha ____" cuando sigue a una resolución o acuerdo → fecha de nombramiento.</summary>
    private static readonly Regex DeFechaRegex = R(@"\bde\s+fecha");

    [GeneratedRegex(@"resoluci[oó]n|acuerdo|nombramiento|escritura", Opciones)]
    private static partial Regex DocumentoPrevioRegex();

    [GeneratedRegex(@"represent|director|gerente|firmante|firma", Opciones)]
    private static partial Regex ContextoRepresentanteRegex();

    private static Regex R(string patron) => new(patron, Opciones);

    // ---------- Análisis ----------

    public static ResultadoAnalisis Analizar(IReadOnlyList<ParrafoDocumento> parrafos)
    {
        var anclas = BuscarAnclas(parrafos);
        var huecos = new List<HuecoDetectado>();

        for (var i = 0; i < parrafos.Count; i++)
        {
            var p = parrafos[i];
            var texto = p.Contenido;
            if (string.IsNullOrWhiteSpace(texto)) continue;

            var finAnterior = 0;
            foreach (Match hueco in HuecoRegex().Matches(texto))
            {
                var ventana = texto[finAnterior..hueco.Index];
                finAnterior = hueco.Index + hueco.Length;

                var etiqueta = ReconocerEtiqueta(ventana, texto, hueco.Index, parrafos, i, anclas);
                if (etiqueta is null) continue;

                var (campo, textoEtiqueta, holgura) = etiqueta.Value;
                var (lado, _) = LadoEn(anclas, parrafos, i, hueco.Index, hueco.Length);
                // La moneda puede ir en la etiqueta ("cuenta en CUP ___") o justo después ("cuenta No. ___ en CUP").
                var tras = texto[finAnterior..Math.Min(texto.Length, finAnterior + 15)];
                var moneda = Moneda(textoEtiqueta + holgura) ?? (Regex.IsMatch(tras, @"^\s*en\s", Opciones) ? Moneda(tras) : null);
                huecos.Add(new HuecoDetectado(
                    i, p.Inicio + hueco.Index, hueco.Length, hueco.Value, string.Empty,
                    campo, lado, textoEtiqueta, moneda,
                    Contexto(texto, hueco.Index, hueco.Length)));
            }

            // Etiqueta al final del párrafo sin valor ("NIT:"), o celda de tabla con la etiqueta y la siguiente vacía.
            if (finAnterior == 0 && EtiquetaSinValor(p, parrafos, i, anclas) is { } sinValor)
                huecos.Add(sinValor);
        }

        string? Denominacion(LadoParte lado) =>
            anclas.Where(a => a.Lado == lado && a.Denominacion is not null)
                  .Select(a => a.Denominacion)
                  .FirstOrDefault();

        return new ResultadoAnalisis(huecos, Denominacion(LadoParte.Proveedor), Denominacion(LadoParte.Cliente));
    }

    /// <summary>Etiqueta inmediatamente anterior al hueco (con solo "No.", "en CUP", ":"… entre medias).</summary>
    private static (CampoContrato Campo, string Etiqueta, string Holgura)? ReconocerEtiqueta(
        string ventana, string textoParrafo, int posHueco,
        IReadOnlyList<ParrafoDocumento> parrafos, int indice, List<Ancla> anclas)
    {
        // Hueco tras "De una parte:" → nombre de la entidad.
        if (NombreTrasAnclaRegex().Match(ventana) is { Success: true } trasAncla)
            return (CampoContrato.Nombre, trasAncla.Value.Trim(), string.Empty);

        (CampoContrato Campo, Match M)? mejor = null;

        foreach (var (campo, regex) in Etiquetas)
            Considerar(campo, regex);

        // "Nombre:" a secas.
        foreach (Match m in NombreGenericoRegex.Matches(ventana))
        {
            if (!HolguraValida(ventana, m)) continue;
            if (mejor is null || m.Index + m.Length > mejor.Value.M.Index + mejor.Value.M.Length)
            {
                var previo = ventana[..m.Index] + (indice > 0 ? parrafos[indice - 1].Contenido : string.Empty);
                var (_, tipo) = LadoEn(anclas, parrafos, indice, posHueco, 0);
                var esRepresentante = tipo == TipoSeccion.Firma || ContextoRepresentanteRegex().IsMatch(previo);
                mejor = (esRepresentante ? CampoContrato.Representante : CampoContrato.Nombre, m);
            }
        }

        // "de fecha ____" detrás de una resolución o acuerdo → fecha de nombramiento.
        foreach (Match m in DeFechaRegex.Matches(ventana))
        {
            if (!HolguraValida(ventana, m)) continue;
            var antes = textoParrafo[..(m.Index + (posHueco - ventana.Length))];
            var cercano = antes.Length > 100 ? antes[^100..] : antes;
            if (DocumentoPrevioRegex().IsMatch(cercano) &&
                (mejor is null || m.Index + m.Length > mejor.Value.M.Index + mejor.Value.M.Length))
                mejor = (CampoContrato.FechaNombramiento, m);
        }

        if (mejor is null) return null;
        var fin = mejor.Value.M.Index + mejor.Value.M.Length;
        return (mejor.Value.Campo, mejor.Value.M.Value.Trim(), ventana[fin..]);

        void Considerar(CampoContrato campo, Regex regex)
        {
            foreach (Match m in regex.Matches(ventana))
            {
                if (!HolguraValida(ventana, m)) continue;
                var finM = m.Index + m.Length;
                // Gana la etiqueta que termina más cerca del hueco; a igualdad, la más larga (más específica).
                if (mejor is null ||
                    finM > mejor.Value.M.Index + mejor.Value.M.Length ||
                    (finM == mejor.Value.M.Index + mejor.Value.M.Length && m.Length > mejor.Value.M.Length))
                    mejor = (campo, m);
            }
        }
    }

    private static bool HolguraValida(string ventana, Match m) =>
        HolguraRegex().IsMatch(ventana[(m.Index + m.Length)..]);

    /// <summary>
    /// Párrafo cuyo final es una etiqueta con dos puntos y sin valor ("NIT:"), o celda de tabla que solo
    /// contiene la etiqueta y cuya celda siguiente está vacía.
    /// </summary>
    private static HuecoDetectado? EtiquetaSinValor(ParrafoDocumento p, IReadOnlyList<ParrafoDocumento> parrafos, int i, List<Ancla> anclas)
    {
        var texto = p.Contenido;
        var siguiente = i + 1 < parrafos.Count ? parrafos[i + 1] : null;
        var celdaSiguienteVacia = p.EnTabla && siguiente is { EnTabla: true } && string.IsNullOrWhiteSpace(siguiente.Contenido);

        var etiqueta = ReconocerEtiqueta(texto, texto, texto.Length, parrafos, i, anclas);
        if (etiqueta is null) return null;
        var (campo, textoEtiqueta, holgura) = etiqueta.Value;

        // Debe ser "etiqueta:" al final del párrafo, o la celda entera ser la etiqueta (con la siguiente vacía).
        var soloEtiqueta = texto.Trim().Length <= textoEtiqueta.Length + holgura.Trim().Length + 1;
        if (celdaSiguienteVacia && soloEtiqueta)
        {
            var (ladoCelda, _) = LadoEn(anclas, parrafos, i, texto.Length, 0);
            return new HuecoDetectado(i + 1, siguiente!.Inicio, 0, string.Empty, string.Empty, campo, ladoCelda,
                textoEtiqueta, Moneda(textoEtiqueta + holgura), Contexto(texto, texto.Length, 0) + " | [celda vacía]");
        }

        if (!holgura.Contains(':')) return null;

        var (lado, _) = LadoEn(anclas, parrafos, i, texto.Length, 0);
        var prefijo = texto.EndsWith(' ') || texto.EndsWith('\t') ? string.Empty : " ";
        return new HuecoDetectado(i, p.Inicio + texto.Length, 0, string.Empty, prefijo, campo, lado,
            textoEtiqueta, Moneda(textoEtiqueta + holgura), Contexto(texto, texto.Length, 0));
    }

    private static List<Ancla> BuscarAnclas(IReadOnlyList<ParrafoDocumento> parrafos)
    {
        var anclas = new List<Ancla>();
        for (var i = 0; i < parrafos.Count; i++)
        {
            var texto = parrafos[i].Contenido;
            if (string.IsNullOrWhiteSpace(texto)) continue;

            if (AnclaFirmaRegex().IsMatch(texto) && RolRegex.Match(texto) is { Success: true, Index: < 12 } rolFirma)
            {
                anclas.Add(new Ancla(i, 0, TipoSeccion.Firma, LadoDeRol(rolFirma.Groups["rol"].Value), Denominacion(rolFirma)));
                continue;
            }

            if ((AnclaDatosRegex().IsMatch(texto) || EsTituloDeParte(texto)) && RolLibreRegex.Match(texto) is { Success: true } rolDatos)
            {
                var conArticulo = RolRegex.Match(texto);
                anclas.Add(new Ancla(i, 0, TipoSeccion.Datos, LadoDeRol(rolDatos.Groups["rol"].Value),
                    Denominacion(conArticulo.Success ? conArticulo : rolDatos)));
                continue;
            }

            foreach (Match m in AnclaComparecenciaRegex().Matches(texto))
                anclas.Add(new Ancla(i, m.Index, TipoSeccion.Comparecencia, LadoParte.Desconocido, null));

            if (FinComparecenciaRegex().Match(texto) is { Success: true } fin)
                anclas.Add(new Ancla(i, fin.Index, TipoSeccion.Cuerpo, LadoParte.Desconocido, null));
        }

        anclas.Sort((a, b) => a.Parrafo != b.Parrafo ? a.Parrafo.CompareTo(b.Parrafo) : a.Posicion.CompareTo(b.Posicion));

        // Lado de cada comparecencia: la denominación que aparece en su tramo (preferentemente tras "en lo adelante…").
        for (var k = 0; k < anclas.Count; k++)
        {
            if (anclas[k].Tipo != TipoSeccion.Comparecencia) continue;
            var tramo = TextoEntre(parrafos, anclas[k], k + 1 < anclas.Count ? anclas[k + 1] : null);
            var roles = RolRegex.Matches(tramo).Cast<Match>().ToList();
            if (roles.Count == 0) continue;

            var intro = IntroDenominacionRegex().Matches(tramo).Cast<Match>().LastOrDefault();
            var elegido = intro is null
                ? roles[^1]
                : roles.FirstOrDefault(r => r.Index > intro.Index) ?? roles[^1];
            anclas[k] = anclas[k] with { Lado = LadoDeRol(elegido.Groups["rol"].Value), Denominacion = Denominacion(elegido) };
        }

        return anclas;
    }

    /// <summary>Título corto que solo nombra a una parte: "EL PRESTADOR:", "CLIENTE".</summary>
    private static bool EsTituloDeParte(string texto) => TituloParteRegex.IsMatch(texto);

    /// <summary>Denominación sin exigir artículo ("Datos del cliente").</summary>
    private static readonly Regex RolLibreRegex = new(
        @"\b(?<rol>" + string.Join("|", RolesProveedor.Concat(RolesCliente).OrderByDescending(r => r.Length)) + @")\b",
        Opciones);

    private static readonly Regex TituloParteRegex = new(
        @"^\s*(?:(?:el|la)\s+)?(?:" + string.Join("|", RolesProveedor.Concat(RolesCliente).OrderByDescending(r => r.Length)) + @")\s*[:.]?\s*$",
        Opciones);

    private static string Denominacion(Match rol)
    {
        var art = rol.Groups["art"].Success ? rol.Groups["art"].Value.ToUpperInvariant() : (rol.Groups["rol"].Value.EndsWith("A", StringComparison.OrdinalIgnoreCase) ? "LA" : "EL");
        return $"{art} {rol.Groups["rol"].Value.ToUpperInvariant()}";
    }

    private static string TextoEntre(IReadOnlyList<ParrafoDocumento> parrafos, Ancla desde, Ancla? hasta)
    {
        var partes = new List<string>();
        var ultimo = hasta?.Parrafo ?? parrafos.Count - 1;
        for (var i = desde.Parrafo; i <= ultimo && i < parrafos.Count; i++)
        {
            var t = parrafos[i].Contenido;
            var ini = i == desde.Parrafo ? desde.Posicion : 0;
            var fin = hasta is not null && i == hasta.Parrafo ? hasta.Posicion : t.Length;
            if (fin > ini) partes.Add(t[ini..fin]);
        }
        return string.Join("\n", partes);
    }

    /// <summary>
    /// Lado y tipo de sección de una posición: el de la última ancla anterior. En el clausulado (o fuera de
    /// cualquier sección) se usa la denominación más cercana en el mismo párrafo ("cuenta No. ____ de EL PRESTADOR").
    /// </summary>
    private static (LadoParte Lado, TipoSeccion Tipo) LadoEn(List<Ancla> anclas, IReadOnlyList<ParrafoDocumento> parrafos, int parrafo, int posicion, int longitud)
    {
        var ancla = anclas.LastOrDefault(a => a.Parrafo < parrafo || (a.Parrafo == parrafo && a.Posicion <= posicion));
        if (ancla is not null && ancla.Tipo != TipoSeccion.Cuerpo && ancla.Lado != LadoParte.Desconocido)
            return (ancla.Lado, ancla.Tipo);

        var texto = parrafos[parrafo].Contenido;
        Match? masCercano = null;
        var mejorDistancia = int.MaxValue;
        foreach (Match m in RolRegex.Matches(texto))
        {
            var distancia = m.Index >= posicion + longitud ? m.Index - (posicion + longitud) : posicion - (m.Index + m.Length);
            if (distancia >= 0 && distancia < mejorDistancia && distancia <= 150)
            {
                masCercano = m;
                mejorDistancia = distancia;
            }
        }

        return (masCercano is null ? LadoParte.Desconocido : LadoDeRol(masCercano.Groups["rol"].Value), ancla?.Tipo ?? TipoSeccion.Cuerpo);
    }

    private static string? Moneda(string texto)
    {
        var m = MonedaRegex().Match(texto);
        if (!m.Success) return null;
        return m.Groups[1].Success ? m.Groups[1].Value.ToUpperInvariant() : "CUP";
    }

    private static string Contexto(string texto, int pos, int longitud)
    {
        var ini = Math.Max(0, pos - 45);
        var fin = Math.Min(texto.Length, pos + longitud + 30);
        var antes = texto[ini..pos].Replace('\t', ' ');
        var despues = texto[(pos + longitud)..fin].Replace('\t', ' ');
        return $"{(ini > 0 ? "…" : "")}{antes}[____]{despues}{(fin < texto.Length ? "…" : "")}";
    }

    // ---------- Valores ----------

    /// <summary>
    /// Valor a escribir en un hueco con los datos de la parte. Si la etiqueta ya trae el tipo de documento
    /// ("Resolución No. ____", "del Banco ____", "Sucursal ____"), se evita repetirlo.
    /// </summary>
    public static string Valor(HuecoDetectado hueco, ContextoParte ctx)
    {
        if (hueco.Moneda is not null && hueco.Campo is CampoContrato.Cuenta or CampoContrato.Banco or CampoContrato.Sucursal
                or CampoContrato.DireccionSucursal or CampoContrato.Titular)
            ctx = ctx with { Cuenta = ctx.Ficha.CuentaPara(hueco.Moneda) };

        var clave = hueco.Campo switch
        {
            CampoContrato.Reeup => "Reeup",
            CampoContrato.Cuenta => "Cuenta",
            _ => hueco.Campo.ToString()
        };
        var valor = BloquesContrato.Valores(ctx).TryGetValue(clave, out var v) ? v.Trim() : string.Empty;

        // Solo donde la etiqueta suele anticipar el tipo de dato ("del Banco ___", "según Resolución No. ___").
        // En nombres no: "la empresa ___" + "Empresa de Telecomunicaciones…" debe quedar completo.
        return hueco.Campo is CampoContrato.Banco or CampoContrato.Sucursal
                or CampoContrato.DocumentoNombramiento or CampoContrato.DocumentoConstitucion
            ? QuitarRepeticion(hueco.Etiqueta, valor)
            : valor;
    }

    /// <summary>Si el valor empieza por la última palabra de la etiqueta (y un "No."), la quita.</summary>
    private static string QuitarRepeticion(string etiqueta, string valor)
    {
        if (string.IsNullOrEmpty(valor)) return valor;
        var ultima = Regex.Match(etiqueta, @"(\p{L}+)\W*$");
        if (!ultima.Success || ultima.Groups[1].Value.Length < 4) return valor;

        var palabra = Regex.Escape(ultima.Groups[1].Value);
        var sinRepetir = Regex.Replace(valor, $@"^{palabra}\b\s*(?:(?:no|nro|n[uú]m(?:ero)?|n[oº°])\s*\.?\s*)?", string.Empty, Opciones);
        return string.IsNullOrWhiteSpace(sinRepetir) ? valor : sinRepetir;
    }
}
