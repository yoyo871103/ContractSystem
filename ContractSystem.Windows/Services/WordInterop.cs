using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ContractSystem.Windows.Services;

/// <summary>Resultado de una operación con Word, con un mensaje listo para mostrar si falló.</summary>
public sealed record ResultadoWord(bool Ok, string? Mensaje = null, string? Documento = null)
{
    public static ResultadoWord Bien(string? documento = null) => new(true, null, documento);
    public static ResultadoWord Error(string mensaje) => new(false, mensaje);
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
