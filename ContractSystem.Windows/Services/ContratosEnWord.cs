using System.IO;
using System.Text.RegularExpressions;
using ContractSystem.Application.Contratos.Queries.DescargarDocumentoAdjunto;
using MediatR;

namespace ContractSystem.Windows.Services;

/// <summary>
/// Documentos de Word que se llenan desde un contrato.
///
/// Un adjunto abierto para llenar se copia a una carpeta propia de la aplicación
/// (…\ContractSystem\Contratos en Word\adjunto-{id}\nombre.docx). La carpeta identifica el adjunto
/// de origen: así «Guardar en el contrato» sabe qué adjunto actualizar, incluso tras reiniciar la aplicación.
/// </summary>
public static class ContratosEnWord
{
    private static readonly Regex CarpetaAdjunto = new(@"^adjunto-(\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>Carpeta de trabajo para los documentos que se llenan.</summary>
    public static string CarpetaTrabajo { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ContractSystem", "Contratos en Word");

    /// <summary>Se dispara (en el hilo de la interfaz) al guardar un documento en los adjuntos de un contrato.</summary>
    public static event EventHandler<int>? AdjuntosCambiados;

    public static void NotificarAdjuntosCambiados(int contratoId) => AdjuntosCambiados?.Invoke(null, contratoId);

    public static bool EsWord(string extension) =>
        extension.Equals(".docx", StringComparison.OrdinalIgnoreCase) || extension.Equals(".doc", StringComparison.OrdinalIgnoreCase);

    /// <summary>Id del adjunto del que procede el documento (por su carpeta de trabajo), o null.</summary>
    public static int? AdjuntoDeOrigen(string rutaDocumento)
    {
        try
        {
            var carpeta = Path.GetDirectoryName(rutaDocumento);
            if (carpeta is null) return null;
            if (!string.Equals(Path.GetDirectoryName(carpeta), CarpetaTrabajo, StringComparison.OrdinalIgnoreCase)) return null;
            var m = CarpetaAdjunto.Match(Path.GetFileName(carpeta));
            return m.Success && int.TryParse(m.Groups[1].Value, out var id) ? id : null;
        }
        catch (ArgumentException)
        {
            return null; // ruta no válida (p. ej. una URL)
        }
    }

    /// <summary>
    /// Copia el adjunto a la carpeta de trabajo y lo abre en Word. Si ya estaba abierto (archivo en uso),
    /// se reabre la copia existente para no perder lo que se haya llenado.
    /// </summary>
    public static async Task<ResultadoWord> AbrirAdjuntoAsync(ISender sender, int adjuntoId, string nombreArchivo)
    {
        var ruta = Path.Combine(CarpetaTrabajo, $"adjunto-{adjuntoId}", NombreSeguro(nombreArchivo));
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
            if (!EstaEnUso(ruta))
            {
                var adjunto = await sender.Send(new DescargarDocumentoAdjuntoQuery(adjuntoId));
                if (!File.Exists(ruta) || !CopiaLocalConCambios(ruta, adjunto.Contenido, adjunto.FechaCarga))
                    await File.WriteAllBytesAsync(ruta, adjunto.Contenido);
            }
        }
        catch (Exception ex)
        {
            return ResultadoWord.Error("No se pudo preparar el documento: " + ex.Message);
        }

        return WordInterop.AbrirDocumento(ruta);
    }

    /// <summary>Lee un archivo aunque Word lo tenga abierto.</summary>
    public static async Task<byte[]> LeerAsync(string ruta)
    {
        await using var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var ms = new MemoryStream();
        await fs.CopyToAsync(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// La copia de trabajo se modificó después de la versión guardada en el contrato: se pregunta
    /// si se sigue con ella (true) o se descarta y se abre la del contrato (false).
    /// </summary>
    private static bool CopiaLocalConCambios(string ruta, byte[] contenidoContrato, DateTime fechaCargaUtc)
    {
        var modificada = File.GetLastWriteTimeUtc(ruta);
        if (modificada <= fechaCargaUtc || File.ReadAllBytes(ruta).AsSpan().SequenceEqual(contenidoContrato))
            return false;

        var respuesta = System.Windows.MessageBox.Show(
            $"Este documento se modificó en Word el {modificada.ToLocalTime():dd/MM/yyyy HH:mm} y esos cambios no se guardaron en el contrato.\n\n" +
            "Sí: seguir con esa copia (con los cambios).\nNo: descartarla y abrir la versión guardada en el contrato.",
            "Llenar en Word", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
        return respuesta == System.Windows.MessageBoxResult.Yes;
    }

    private static bool EstaEnUso(string ruta)
    {
        if (!File.Exists(ruta)) return false;
        try
        {
            using var _ = new FileStream(ruta, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (IOException)
        {
            return true;
        }
    }

    private static string NombreSeguro(string nombre)
    {
        var limpio = string.Concat(Path.GetFileName(nombre).Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return string.IsNullOrWhiteSpace(limpio) ? "documento.docx" : limpio;
    }
}
