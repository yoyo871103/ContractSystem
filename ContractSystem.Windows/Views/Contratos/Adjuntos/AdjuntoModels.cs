using ContractSystem.Domain.Contratos;

namespace ContractSystem.Windows.Views.Contratos;

/// <summary>
/// Tipo de documento al que está asociado un adjunto.
/// </summary>
public enum TipoAsociacionAdjunto
{
    Contrato,
    Suplemento,
    Factura,
    Anexo
}

/// <summary>
/// Documento al que se puede asociar (o está asociado) un adjunto.
/// </summary>
public sealed record AdjuntoDestino(
    TipoAsociacionAdjunto Tipo,
    string Descripcion,
    int ContratoId,
    int? FacturaId = null,
    int? AnexoId = null)
{
    public override string ToString() => Descripcion;

    public bool Corresponde(DocumentoAdjunto d) =>
        d.ContratoId == ContratoId && d.FacturaId == FacturaId && d.AnexoId == AnexoId;
}

/// <summary>
/// Item de presentación para las listas de adjuntos (sin Contenido y con tamaño formateado).
/// </summary>
public class AdjuntoListItem
{
    public int Id { get; }
    public string NombreArchivo { get; }
    public string Extension { get; }
    public string Objetivo { get; }
    public string TamanioTexto { get; }
    public DateTime FechaCarga { get; }
    public int ContratoId { get; }
    public int? FacturaId { get; }
    public int? AnexoId { get; }

    /// <summary>Tipo de documento al que pertenece (Contrato, Suplemento, Factura, Anexo).</summary>
    public TipoAsociacionAdjunto TipoAsociacion { get; }

    /// <summary>Descripción del documento al que pertenece (ej: "Factura F-001 · Supl. S-02").</summary>
    public string AsociadoA { get; }

    /// <summary>Compatibilidad: igual que <see cref="AsociadoA"/>.</summary>
    public string ContextoTexto => AsociadoA;

    public string Icono => Extension.ToLowerInvariant() switch
    {
        ".pdf" => "",
        ".doc" or ".docx" => "",
        ".xls" or ".xlsx" => "",
        ".png" or ".jpg" or ".jpeg" or ".gif" or ".bmp" => "",
        _ => ""
    };

    public AdjuntoListItem(DocumentoAdjunto d, string asociadoA = "", TipoAsociacionAdjunto? tipo = null)
    {
        Id = d.Id;
        NombreArchivo = d.NombreArchivo;
        Extension = d.Extension;
        Objetivo = d.Objetivo;
        FechaCarga = d.FechaCarga;
        TamanioTexto = FormatearTamanio(d.TamanioBytes);
        ContratoId = d.ContratoId;
        FacturaId = d.FacturaId;
        AnexoId = d.AnexoId;
        TipoAsociacion = tipo ?? (d.FacturaId.HasValue ? TipoAsociacionAdjunto.Factura
            : d.AnexoId.HasValue ? TipoAsociacionAdjunto.Anexo
            : TipoAsociacionAdjunto.Contrato);
        AsociadoA = asociadoA;
    }

    public static string FormatearTamanio(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:N1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):N1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):N2} GB"
    };
}
