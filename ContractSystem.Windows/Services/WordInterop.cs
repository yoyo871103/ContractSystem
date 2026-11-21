using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ContractSystem.Application.LlenadoContratos;

namespace ContractSystem.Windows.Services;

/// <summary>Resultado de una operación con Word, con un mensaje listo para mostrar si falló.</summary>
public sealed record ResultadoWord(bool Ok, string? Mensaje = null, string? Documento = null)
{
    public static ResultadoWord Bien(string? documento = null) => new(true, null, documento);
    public static ResultadoWord Error(string mensaje) => new(false, mensaje);
}

/// <summary>Documento leído de Word: ruta completa (para comprobar que sigue activo), nombre y párrafos.</summary>
public sealed record LecturaDocumento(string Documento, string Nombre, IReadOnlyList<ParrafoDocumento> Parrafos);

/// <summary>Cambio a aplicar: sustituir (o insertar si Longitud = 0) en una posición, con la comprobación del párrafo.</summary>
public sealed record ReemplazoWord(int ParrafoInicio, int ParrafoFin, string ParrafoTexto, int Inicio, int Longitud, string TextoEsperado, string Prefijo, string Texto);

public sealed record ResultadoAutollenado(bool Ok, int Aplicados, int Omitidos, string? Mensaje)
{
    public static ResultadoAutollenado Error(string mensaje) => new(false, 0, 0, mensaje);
}

/// <summary>
/// Automatización del Word instalado (COM, enlace tardío): no requiere complementos en Word
/// ni referencias a ensamblados de Office.
///
/// No se guarda ninguna referencia a Word entre llamadas: si se conservara, Word quedaría
/// vivo en segundo plano aunque el usuario lo cierre. Cada operación obtiene la instancia
/// en ejecución, trabaja y libera los objetos COM.
/// </summary>
public static class WordInterop
{
    private const int WdCollapseEnd = 0;

    // HRESULT habituales cuando Word está ocupado (diálogo abierto, editando una celda, etc.).
    private const int RpcECallRejected = unchecked((int)0x80010001);
    private const int RpcEServerCallRetryLater = unchecked((int)0x8001010A);

    /// <summary>Abre el documento en Word (o lo trae al frente si ya está abierto).</summary>
    public static ResultadoWord AbrirDocumento(string ruta)
    {
        if (!File.Exists(ruta))
            return ResultadoWord.Error("No se encontró el archivo.");

        try
        {
            // Se abre como lo haría el Explorador: Word arranca con normalidad (con sus
            // complementos y la ventana del usuario) y no queda "atado" a esta aplicación.
            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            return ResultadoWord.Bien(Path.GetFileName(ruta));
        }
        catch (Exception ex)
        {
            return ResultadoWord.Error("No se pudo abrir el documento en Word: " + ex.Message);
        }
    }

    /// <summary>Nombre del documento activo en Word, o null si Word no está abierto o no tiene documentos.</summary>
    public static string? DocumentoActivo()
    {
        try
        {
            return ConWord(word =>
            {
                if ((int)word.Documents.Count == 0) return null;
                return (string?)word.ActiveDocument.Name;
            });
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Inserta el texto donde está el cursor de Word (si hay texto seleccionado, lo sustituye),
    /// deja el cursor justo después y devuelve el foco a Word para que el usuario siga escribiendo.
    /// El texto toma el formato del punto de inserción. Saltos de línea "\n" pasan a ser párrafos.
    /// </summary>
    public static ResultadoWord InsertarEnCursor(string texto)
    {
        try
        {
            return ConWord(word =>
            {
                if ((int)word.Documents.Count == 0)
                    return ResultadoWord.Error("Word no tiene ningún documento abierto. Abra la proforma y coloque el cursor donde quiera insertar.");

                dynamic seleccion = word.Selection;
                dynamic rango = seleccion.Range;
                rango.Text = texto.Replace("\r\n", "\n").Replace('\n', '\r');
                rango.Collapse(WdCollapseEnd);
                rango.Select();

                string documento = word.ActiveDocument.Name;
                TraerAlFrente(word);
                return ResultadoWord.Bien(documento);
            }) ?? ResultadoWord.Error("Word no está abierto. Abra la proforma en Word y coloque el cursor donde quiera insertar.");
        }
        catch (COMException ex) when (ex.HResult is RpcECallRejected or RpcEServerCallRetryLater)
        {
            return ResultadoWord.Error("Word está ocupado (puede haber un cuadro de diálogo abierto o una celda en edición). Ciérrelo e inténtelo de nuevo.");
        }
        catch (COMException ex)
        {
            return ResultadoWord.Error("Word no permitió insertar el texto (¿documento protegido o de solo lectura?): " + ex.Message);
        }
        catch (Exception ex)
        {
            return ResultadoWord.Error("No se pudo insertar en Word: " + ex.Message);
        }
    }

    // ---------- Autollenado ----------

    private const int WdWithInTable = 12;
    private const int WdYellow = 7;
    private const int WdNoHighlight = 0;
    private const string PrefijoMarcador = "_CSAuto";
    private const string PrefijoMarcadorVisible = "CSAuto_";

    /// <summary>
    /// Lee los párrafos del documento activo (posición, texto y si están en una tabla).
    /// Devuelve el error a mostrar si Word no está abierto, no tiene documentos o está ocupado.
    /// </summary>
    public static (LecturaDocumento? Lectura, string? Error) LeerDocumentoActivo()
    {
        try
        {
            var resultado = ConWord<(LecturaDocumento?, string?)>(word =>
            {
                if ((int)word.Documents.Count == 0)
                    return (null, "Word no tiene ningún documento abierto. Abra la proforma primero.");

                dynamic doc = word.ActiveDocument;
                var parrafos = new List<ParrafoDocumento>();
                // Recorrido con Next(): el acceso por índice a Paragraphs es lento en documentos largos.
                dynamic? p = doc.Paragraphs.Count > 0 ? doc.Paragraphs.First : null;
                while (p is not null)
                {
                    dynamic rango = p.Range;
                    parrafos.Add(new ParrafoDocumento((int)rango.Start, (string)rango.Text, (bool)rango.Information(WdWithInTable), (int)rango.End));
                    p = p.Next();
                }

                return (new LecturaDocumento((string)doc.FullName, (string)doc.Name, parrafos), null);
            });

            return resultado == default
                ? (null, "Word no está abierto. Abra la proforma en Word primero.")
                : resultado;
        }
        catch (COMException ex) when (ex.HResult is RpcECallRejected or RpcEServerCallRetryLater)
        {
            return (null, "Word está ocupado (puede haber un cuadro de diálogo abierto). Ciérrelo e inténtelo de nuevo.");
        }
        catch (Exception ex)
        {
            return (null, "No se pudo leer el documento de Word: " + ex.Message);
        }
    }

    /// <summary>
    /// Aplica los reemplazos en el documento (si sigue siendo el activo), resaltando en amarillo lo escrito.
    /// Antes de tocar nada comprueba que cada párrafo sigue igual que cuando se analizó; los que cambiaron
    /// se omiten. Todo queda en un único paso de deshacer ("Autollenar contrato").
    /// </summary>
    public static ResultadoAutollenado AplicarAutollenado(string documento, IReadOnlyList<ReemplazoWord> reemplazos)
    {
        try
        {
            return ConWord(word =>
            {
                if ((int)word.Documents.Count == 0 || !string.Equals((string)word.ActiveDocument.FullName, documento, StringComparison.OrdinalIgnoreCase))
                    return ResultadoAutollenado.Error("El documento activo en Word ya no es el que se analizó. Actívelo y vuelva a pulsar «Autollenar».");

                dynamic doc = word.ActiveDocument;

                // 1) Verificar antes de modificar (las posiciones son las del análisis).
                var parrafosValidos = new HashSet<int>();
                foreach (var r0 in reemplazos.GroupBy(r => r.ParrafoInicio).Select(g => g.First()))
                {
                    string actual = doc.Range(r0.ParrafoInicio, r0.ParrafoFin).Text;
                    if (actual == r0.ParrafoTexto) parrafosValidos.Add(r0.ParrafoInicio);
                }

                var validos = reemplazos.Where(r => parrafosValidos.Contains(r.ParrafoInicio)).ToList();
                var omitidos = reemplazos.Count - validos.Count;
                var aplicados = 0;
                var lote = Random.Shared.Next(1000, 9999);

                IniciarDeshacer(word, "Autollenar contrato");
                try
                {
                    // 2) De atrás hacia delante, para que las posiciones anteriores no se desplacen.
                    foreach (var r in validos.OrderByDescending(r => r.Inicio))
                    {
                        dynamic rango = doc.Range(r.Inicio, r.Inicio + r.Longitud);
                        if (r.Longitud > 0 && (string)rango.Text != r.TextoEsperado)
                        {
                            omitidos++;
                            continue;
                        }

                        rango.Text = r.Prefijo + r.Texto;
                        dynamic resaltar = doc.Range((int)rango.Start + r.Prefijo.Length, (int)rango.End);
                        resaltar.HighlightColorIndex = WdYellow;
                        MarcarInsercion(doc, resaltar, $"{lote}_{aplicados}");
                        aplicados++;
                    }
                }
                finally
                {
                    TerminarDeshacer(word);
                }

                TraerAlFrente(word);
                return new ResultadoAutollenado(true, aplicados, omitidos, null);
            }) ?? ResultadoAutollenado.Error("Word no está abierto.");
        }
        catch (COMException ex) when (ex.HResult is RpcECallRejected or RpcEServerCallRetryLater)
        {
            return ResultadoAutollenado.Error("Word está ocupado (puede haber un cuadro de diálogo abierto). Ciérrelo e inténtelo de nuevo.");
        }
        catch (Exception ex)
        {
            return ResultadoAutollenado.Error("No se pudo completar el autollenado: " + ex.Message);
        }
    }

    /// <summary>Quita el resaltado amarillo de lo escrito por el autollenado (y solo de eso).</summary>
    public static ResultadoWord QuitarResaltado()
    {
        try
        {
            return ConWord(word =>
            {
                if ((int)word.Documents.Count == 0)
                    return ResultadoWord.Error("Word no tiene ningún documento abierto.");

                dynamic doc = word.ActiveDocument;
                dynamic marcadores = doc.Bookmarks;
                bool mostrabaOcultos = marcadores.ShowHidden;
                marcadores.ShowHidden = true;
                var quitados = 0;
                try
                {
                    for (int i = (int)marcadores.Count; i >= 1; i--)
                    {
                        dynamic m = marcadores[i];
                        string nombre = m.Name;
                        if (!nombre.StartsWith(PrefijoMarcador, StringComparison.Ordinal) &&
                            !nombre.StartsWith(PrefijoMarcadorVisible, StringComparison.Ordinal))
                            continue;
                        m.Range.HighlightColorIndex = WdNoHighlight;
                        m.Delete();
                        quitados++;
                    }
                }
                finally
                {
                    marcadores.ShowHidden = mostrabaOcultos;
                }

                return quitados > 0
                    ? ResultadoWord.Bien((string)doc.Name)
                    : ResultadoWord.Error("No hay nada resaltado por el autollenado en este documento.");
            }) ?? ResultadoWord.Error("Word no está abierto.");
        }
        catch (Exception ex)
        {
            return ResultadoWord.Error("No se pudo quitar el resaltado: " + ex.Message);
        }
    }

    /// <summary>
    /// Marcador oculto (nombre con "_") sobre lo escrito, para poder quitar luego solo ese resaltado
    /// sin tocar otros resaltados del documento. Si la versión de Word no admite ocultos, se usa uno visible.
    /// </summary>
    private static void MarcarInsercion(dynamic doc, dynamic rango, string sufijo)
    {
        try { doc.Bookmarks.Add(PrefijoMarcador + sufijo, rango); }
        catch
        {
            try { doc.Bookmarks.Add(PrefijoMarcadorVisible + sufijo, rango); } catch { /* sin marcador: solo se pierde "Quitar resaltado" para este dato */ }
        }
    }

    private static void IniciarDeshacer(dynamic word, string nombre)
    {
        try { word.UndoRecord.StartCustomRecord(nombre); } catch { /* Word 2007: cada cambio se deshace por separado */ }
    }

    private static void TerminarDeshacer(dynamic word)
    {
        try { word.UndoRecord.EndCustomRecord(); } catch { }
    }

    /// <summary>
    /// Ejecuta la acción con la instancia de Word en ejecución. Devuelve default si Word no está abierto.
    /// </summary>
    private static T? ConWord<T>(Func<dynamic, T> accion)
    {
        object? word = ObtenerWordEnEjecucion();
        if (word is null) return default;

        try
        {
            return accion(word);
        }
        finally
        {
            // ReleaseComObject (no FinalRelease): si otra parte del proceso tuviera la misma instancia,
            // FinalRelease la invalidaría también para ella.
            Marshal.ReleaseComObject(word);
            // Los objetos intermedios (Selection, Range…) se liberan al recolectar.
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private static object? ObtenerWordEnEjecucion()
    {
        if (CLSIDFromProgID("Word.Application", out var clsid) != 0)
            return null; // Word no está instalado

        var hr = GetActiveObject(ref clsid, IntPtr.Zero, out var instancia);
        return hr == 0 ? instancia : null; // MK_E_UNAVAILABLE: Word no está abierto
    }

    private static void TraerAlFrente(dynamic word)
    {
        try
        {
            word.ActiveWindow.Activate();
            var hwnd = new IntPtr((int)word.ActiveWindow.Hwnd);
            if (hwnd != IntPtr.Zero)
            {
                if (IsIconic(hwnd)) ShowWindow(hwnd, SwRestore);
                SetForegroundWindow(hwnd);
            }
        }
        catch
        {
            // Versiones antiguas de Word sin ActiveWindow.Hwnd: el usuario hace clic en Word.
        }
    }

    /// <summary>Rectángulo (en píxeles físicos) de la ventana activa de Word, si está abierto.</summary>
    public static (IntPtr Hwnd, int Left, int Top, int Right, int Bottom)? VentanaWord()
    {
        try
        {
            return ConWord<(IntPtr, int, int, int, int)?>(word =>
            {
                if ((int)word.Windows.Count == 0) return null;
                var hwnd = new IntPtr((int)word.ActiveWindow.Hwnd);
                return GetWindowRect(hwnd, out var r) ? (hwnd, r.Left, r.Top, r.Right, r.Bottom) : null;
            });
        }
        catch
        {
            return null;
        }
    }

    private const int SwRestore = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left, Top, Right, Bottom; }

    [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
    private static extern int CLSIDFromProgID(string progId, out Guid clsid);

    [DllImport("oleaut32.dll")]
    private static extern int GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);
}
