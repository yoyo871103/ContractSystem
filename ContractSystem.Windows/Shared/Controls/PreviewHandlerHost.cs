using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace ContractSystem.Windows.Shared.Controls;

/// <summary>
/// Hospeda un "preview handler" de Windows (los mismos que usa el panel de vista previa del
/// Explorador) para mostrar embebidos documentos Word, Excel, etc. sin abrir la aplicación.
/// Requiere que el handler correspondiente esté instalado (Office lo registra para .doc/.docx/.xls/.xlsx).
/// </summary>
public sealed class PreviewHandlerHost : HwndHost
{
    private const string PreviewHandlerShellEx = "{8895b1c6-b41f-4c1c-a562-0d564250836f}";
    private const uint STGM_READ = 0x00000000;

    private IntPtr _hwnd;
    private object? _handlerInstance;
    private IPreviewHandler? _handler;
    private IStream? _stream;

    /// <summary>
    /// Devuelve el CLSID del preview handler registrado para la extensión, o null si no hay ninguno.
    /// </summary>
    public static Guid? BuscarHandler(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return null;
        if (!extension.StartsWith('.')) extension = "." + extension;

        // 1) HKCR\.ext\shellex\{8895...}
        var clsid = LeerDefault($@"{extension}\shellex\{PreviewHandlerShellEx}");

        // 2) HKCR\<ProgID>\shellex\{8895...}
        if (clsid is null)
        {
            var progId = LeerDefault(extension);
            if (!string.IsNullOrEmpty(progId))
                clsid = LeerDefault($@"{progId}\shellex\{PreviewHandlerShellEx}");
        }

        // 3) HKCR\SystemFileAssociations\.ext\shellex\{8895...}
        clsid ??= LeerDefault($@"SystemFileAssociations\{extension}\shellex\{PreviewHandlerShellEx}");

        return Guid.TryParse(clsid, out var g) ? g : null;
    }

    private static string? LeerDefault(string subKey)
    {
        try
        {
            using var key = Registry.ClassesRoot.OpenSubKey(subKey);
            return key?.GetValue(null) as string;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Carga el archivo con el handler indicado. Lanza excepción si el handler no puede inicializarse.
    /// Debe llamarse con el control ya cargado (tiene HWND).
    /// </summary>
    public void Abrir(string filePath, Guid clsid)
    {
        Descargar();

        var tipo = Type.GetTypeFromCLSID(clsid, throwOnError: true)!;
        _handlerInstance = Activator.CreateInstance(tipo)
                           ?? throw new InvalidOperationException("No se pudo crear el visor de vista previa.");

        if (_handlerInstance is IInitializeWithFile initFile)
        {
            initFile.Initialize(filePath, STGM_READ);
        }
        else if (_handlerInstance is IInitializeWithStream initStream)
        {
            var hr = SHCreateStreamOnFileEx(filePath, STGM_READ, 0, false, null, out var stream);
            if (hr != 0) Marshal.ThrowExceptionForHR(hr);
            _stream = stream;
            initStream.Initialize(stream, STGM_READ);
        }
        else
        {
            throw new NotSupportedException("El visor de vista previa no admite inicialización por archivo ni por stream.");
        }

        _handler = (IPreviewHandler)_handlerInstance;
        var rect = RectCliente();
        _handler.SetWindow(_hwnd, ref rect);
        _handler.DoPreview();
    }

    public void Descargar()
    {
        if (_handler is not null)
        {
            try { _handler.Unload(); } catch { /* ignorar */ }
        }
        if (_stream is not null)
        {
            try { Marshal.FinalReleaseComObject(_stream); } catch { }
            _stream = null;
        }
        if (_handlerInstance is not null && Marshal.IsComObject(_handlerInstance))
        {
            try { Marshal.FinalReleaseComObject(_handlerInstance); } catch { }
        }
        _handlerInstance = null;
        _handler = null;
    }

    protected override HandleRef BuildWindowCore(HandleRef hwndParent)
    {
        _hwnd = CreateWindowEx(0, "static", "",
            WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
            0, 0, 100, 100, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        return new HandleRef(this, _hwnd);
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
        Descargar();
        DestroyWindow(hwnd.Handle);
        _hwnd = IntPtr.Zero;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (_handler is null) return;
        var rect = RectCliente();
        try { _handler.SetRect(ref rect); } catch { /* ignorar */ }
    }

    private RECT RectCliente()
    {
        // Convertir tamaño WPF (DIPs) a píxeles físicos.
        var source = PresentationSource.FromVisual(this);
        var m = source?.CompositionTarget?.TransformToDevice ?? System.Windows.Media.Matrix.Identity;
        return new RECT
        {
            left = 0,
            top = 0,
            right = (int)Math.Ceiling(ActualWidth * m.M11),
            bottom = (int)Math.Ceiling(ActualHeight * m.M22)
        };
    }

    // ---------- Interop ----------

    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;
    private const int WS_CLIPCHILDREN = 0x02000000;
    private const int WS_CLIPSIBLINGS = 0x04000000;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style,
        int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hwnd);

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SHCreateStreamOnFileEx(string pszFile, uint grfMode, uint dwAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool fCreate, IStream? pstmTemplate, out IStream ppstm);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left, top, right, bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    }

    [ComImport, Guid("8895b1c6-b41f-4c1c-a562-0d564250836f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPreviewHandler
    {
        void SetWindow(IntPtr hwnd, ref RECT rect);
        void SetRect(ref RECT rect);
        void DoPreview();
        void Unload();
        void SetFocus();
        void QueryFocus(out IntPtr phwnd);
        [PreserveSig] uint TranslateAccelerator(ref MSG pmsg);
    }

    [ComImport, Guid("b7d14566-0509-4cce-a71f-0a554233bd9b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IInitializeWithFile
    {
        void Initialize([MarshalAs(UnmanagedType.LPWStr)] string pszFilePath, uint grfMode);
    }

    [ComImport, Guid("b824b49d-22ac-4161-ac8a-9916e8fa3f7f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IInitializeWithStream
    {
        void Initialize(IStream pstm, uint grfMode);
    }
}
