using System.Windows;
using System.Windows.Input;
using ContractSystem.Application.Contratos.Queries.GetAdjuntosByFactura;
using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Windows.Views.Contratos;

/// <summary>
/// Ventana de documentos adjuntos.
/// - Modo contrato: lista unificada con TODOS los adjuntos del contrato (propios, de sus anexos,
///   de sus facturas y de sus suplementos), indicando a qué documento está asociado cada uno.
///   Al adjuntar se elige el documento destino.
/// - Modo factura: solo los adjuntos de una factura concreta.
/// </summary>
public partial class DocumentosAdjuntosWindow : Window
{
    private readonly ISender _sender;
    private AdjuntosContrato? _datos;

    /// <summary>
    /// Modo contrato (vista unificada).
    /// </summary>
    public DocumentosAdjuntosWindow(ISender sender, int contratoId, string contratoNumero, bool readOnly = false,
        TipoDocumentoContrato? tipoDocumento = null)
    {
        InitializeComponent();
        _sender = sender;
        var tipo = tipoDocumento ?? TipoDocumentoContrato.Independiente;

        TxtTitulo.Text = $"Documentos Adjuntos — {contratoNumero}";
        TxtSubtitulo.Text = "Adjuntos del contrato, de sus anexos, de sus facturas y de sus suplementos. " +
                            "Doble clic para verlos sin descargarlos.";

        Panel.Inicializar(sender, readOnly, mostrarAsociacion: true);
        Panel.TextoVacio = "Este contrato no tiene documentos adjuntos.";
        Panel.Cargador = async () =>
        {
            _datos = await AdjuntosContrato.CargarAsync(_sender, contratoId, contratoNumero, tipo);
            ActualizarResumen();
            return _datos.Adjuntos;
        };
        Panel.Destinos = () => _datos?.Destinos ?? Array.Empty<AdjuntoDestino>();
        Panel.DestinoPorDefecto = () => _datos?.Destinos.FirstOrDefault();

        Loaded += async (_, _) => await Panel.RecargarAsync();
    }

    /// <summary>
    /// Modo factura: los adjuntos de una factura concreta.
    /// </summary>
    public DocumentosAdjuntosWindow(ISender sender, int contratoId, int facturaId, string facturaNumero, bool readOnly = false)
    {
        InitializeComponent();
        _sender = sender;

        TxtTitulo.Text = $"Documentos Adjuntos — Factura {facturaNumero}";
        TxtSubtitulo.Text = "Doble clic para ver el adjunto sin descargarlo.";

        var destino = new AdjuntoDestino(TipoAsociacionAdjunto.Factura, $"Factura {facturaNumero}", contratoId, FacturaId: facturaId);
        Panel.Inicializar(sender, readOnly, mostrarAsociacion: false);
        Panel.TextoVacio = "Esta factura no tiene documentos adjuntos.";
        Panel.Cargador = async () =>
        {
            var lista = await _sender.Send(new GetAdjuntosByFacturaQuery(facturaId));
            var items = lista.Select(a => new AdjuntoListItem(a, destino.Descripcion, TipoAsociacionAdjunto.Factura)).ToList();
            TxtResumen.Text = $"{items.Count} adjunto(s)";
            return items;
        };
        Panel.Destinos = () => new[] { destino };

        Loaded += async (_, _) => await Panel.RecargarAsync();
    }

    private void ActualizarResumen()
    {
        if (_datos is null) return;
        var grupos = _datos.Adjuntos
            .GroupBy(a => a.TipoAsociacion)
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()}");
        TxtResumen.Text = _datos.Adjuntos.Count == 0
            ? "Sin adjuntos"
            : $"{_datos.Adjuntos.Count} adjunto(s) — " + string.Join(" · ", grupos);
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
