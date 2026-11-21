using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos.Commands.AdjuntarDocumento;
using ContractSystem.Application.Contratos.Commands.EliminarDocumentoAdjunto;
using ContractSystem.Application.Contratos.Queries.DescargarDocumentoAdjunto;
using ContractSystem.Application.Contratos.Queries.GetContratoById;
using ContractSystem.Windows.Services;
using MediatR;
using Microsoft.Win32;

namespace ContractSystem.Windows.Views.Contratos;

/// <summary>
/// Panel reutilizable de adjuntos: lista + Adjuntar / Ver / Descargar / Eliminar.
/// El contenedor indica cómo cargar la lista (<see cref="Cargador"/>) y a qué documentos
/// se puede adjuntar (<see cref="Destinos"/>).
/// </summary>
public partial class AdjuntosPanel : UserControl
{
    private ISender? _sender;
    private bool _soloLectura;
    private IReadOnlyList<AdjuntoListItem> _todos = Array.Empty<AdjuntoListItem>();

    public AdjuntosPanel()
    {
        InitializeComponent();
        ActualizarBotones();

        // Si se guarda un documento de Word en un contrato desde el panel, la lista se actualiza sola.
        Loaded += (_, _) => ContratosEnWord.AdjuntosCambiados += OnAdjuntosCambiadosDesdeWord;
        Unloaded += (_, _) => ContratosEnWord.AdjuntosCambiados -= OnAdjuntosCambiadosDesdeWord;
    }

    private async void OnAdjuntosCambiadosDesdeWord(object? sender, int contratoId)
    {
        if (Cargador is null || !IsEnabled) return;
        await RecargarAsync();
        AdjuntosCambiados?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Carga la lista de adjuntos a mostrar.</summary>
    public Func<Task<IReadOnlyList<AdjuntoListItem>>>? Cargador { get; set; }

    /// <summary>Documentos a los que se puede adjuntar (si hay uno solo, queda fijo).</summary>
    public Func<IReadOnlyList<AdjuntoDestino>>? Destinos { get; set; }

    /// <summary>Destino preseleccionado al adjuntar (opcional).</summary>
    public Func<AdjuntoDestino?>? DestinoPorDefecto { get; set; }

    /// <summary>Texto cuando la lista está vacía.</summary>
    public string TextoVacio { get; set; } = "No hay documentos adjuntos.";

    /// <summary>Se dispara tras adjuntar o eliminar.</summary>
    public event EventHandler? AdjuntosCambiados;

    public int Cantidad => _todos.Count;

    public void Inicializar(ISender sender, bool soloLectura, bool mostrarAsociacion, string? titulo = null)
    {
        _sender = sender;
        _soloLectura = soloLectura;

        var vis = mostrarAsociacion ? Visibility.Visible : Visibility.Collapsed;
        ColTipo.Visibility = vis;
        ColAsociado.Visibility = vis;
        PnlFiltro.Visibility = vis;

        BtnAdjuntar.Visibility = soloLectura ? Visibility.Collapsed : Visibility.Visible;
        BtnEliminar.Visibility = soloLectura ? Visibility.Collapsed : Visibility.Visible;
        MnuEliminar.Visibility = soloLectura ? Visibility.Collapsed : Visibility.Visible;
        DgAdjuntos.AllowDrop = !soloLectura;

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            TxtTitulo.Text = titulo;
            TxtTitulo.Visibility = Visibility.Visible;
        }
        ActualizarBotones();
    }

    public string Titulo
    {
        set
        {
            TxtTitulo.Text = value;
            TxtTitulo.Visibility = string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>Habilita o deshabilita el panel (ej: no hay factura/anexo seleccionado).</summary>
    public void SetDisponible(bool disponible, string? textoNoDisponible = null)
    {
        IsEnabled = disponible;
        if (!disponible)
        {
            _todos = Array.Empty<AdjuntoListItem>();
            DgAdjuntos.ItemsSource = null;
            TxtVacio.Text = textoNoDisponible ?? TextoVacio;
            TxtVacio.Visibility = Visibility.Visible;
        }
    }

    public async Task RecargarAsync()
    {
        if (Cargador is null) return;
        try
        {
            _todos = await Cargador();
            ConfigurarFiltro();
            AplicarFiltro();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar adjuntos: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ---------- Filtro por tipo de documento ----------

    private const string FiltroTodos = "Todos los documentos";

    private void ConfigurarFiltro()
    {
        if (PnlFiltro.Visibility != Visibility.Visible) return;

        var anterior = CmbFiltro.SelectedItem as string;
        var opciones = new List<string> { FiltroTodos };
        opciones.AddRange(_todos.Select(t => t.TipoAsociacion).Distinct().OrderBy(t => t).Select(NombreFiltro));

        CmbFiltro.SelectionChanged -= CmbFiltro_SelectionChanged;
        CmbFiltro.ItemsSource = opciones;
        CmbFiltro.SelectedItem = anterior is not null && opciones.Contains(anterior) ? anterior : FiltroTodos;
        CmbFiltro.SelectionChanged += CmbFiltro_SelectionChanged;
    }

    private static string NombreFiltro(TipoAsociacionAdjunto t) => t switch
    {
        TipoAsociacionAdjunto.Contrato => "Solo del contrato",
        TipoAsociacionAdjunto.Suplemento => "Solo de suplementos",
        TipoAsociacionAdjunto.Factura => "Solo de facturas",
        TipoAsociacionAdjunto.Anexo => "Solo de anexos",
        _ => t.ToString()
    };

    private void AplicarFiltro()
    {
        var filtro = CmbFiltro.SelectedItem as string;
        IEnumerable<AdjuntoListItem> lista = _todos;
        if (PnlFiltro.Visibility == Visibility.Visible && filtro is not null && filtro != FiltroTodos)
            lista = _todos.Where(t => NombreFiltro(t.TipoAsociacion) == filtro);

        var seleccionadoId = (DgAdjuntos.SelectedItem as AdjuntoListItem)?.Id;
        var items = lista.ToList();
        DgAdjuntos.ItemsSource = items;
        if (seleccionadoId is int id)
            DgAdjuntos.SelectedItem = items.FirstOrDefault(i => i.Id == id);

        TxtVacio.Text = _soloLectura ? TextoVacio : TextoVacio + "\nPulse \"Adjuntar\" o arrastre archivos aquí.";
        TxtVacio.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ActualizarBotones();
    }

    private void CmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicarFiltro();

    // ---------- Acciones ----------

    private AdjuntoListItem? Seleccionado => DgAdjuntos.SelectedItem as AdjuntoListItem;

    private void ActualizarBotones()
    {
        var haySel = Seleccionado is not null;
        BtnVer.IsEnabled = haySel;
        BtnDescargar.IsEnabled = haySel;
        BtnEliminar.IsEnabled = haySel && !_soloLectura;
        var esWord = Seleccionado is { } sel && ContratosEnWord.EsWord(sel.Extension);
        BtnLlenarWord.IsEnabled = esWord;
        MnuLlenarWord.Visibility = esWord ? Visibility.Visible : Visibility.Collapsed;
    }

    private void DgAdjuntos_SelectionChanged(object sender, SelectionChangedEventArgs e) => ActualizarBotones();

    private async void BtnAdjuntar_Click(object sender, RoutedEventArgs e) => await AdjuntarAsync(null);

    private async Task AdjuntarAsync(IEnumerable<string>? archivos)
    {
        if (_soloLectura || _sender is null) return;

        var destinos = Destinos?.Invoke() ?? Array.Empty<AdjuntoDestino>();
        if (destinos.Count == 0)
        {
            MessageBox.Show("No hay ningún documento al que asociar el adjunto.", "Adjuntos",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dlg = new AdjuntarArchivoDialog(destinos, DestinoPorDefecto?.Invoke(), archivos)
        {
            Owner = Window.GetWindow(this)
        };
        if (dlg.ShowDialog() != true || dlg.Destino is null) return;

        var errores = new List<string>();
        var ok = 0;
        foreach (var path in dlg.Archivos)
        {
            try
            {
                var contenido = await File.ReadAllBytesAsync(path);
                await _sender.Send(new AdjuntarDocumentoCommand(
                    dlg.Destino.ContratoId,
                    Path.GetFileName(path),
                    dlg.Objetivo,
                    contenido,
                    dlg.Destino.FacturaId,
                    dlg.Destino.AnexoId));
                ok++;
            }
            catch (Exception ex)
            {
                errores.Add($"{Path.GetFileName(path)}: {ex.Message}");
            }
        }

        await RecargarAsync();
        if (ok > 0) AdjuntosCambiados?.Invoke(this, EventArgs.Empty);

        if (errores.Count > 0)
            MessageBox.Show("No se pudieron adjuntar algunos archivos:\n\n" + string.Join("\n", errores),
                "Adjuntos", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void BtnVer_Click(object sender, RoutedEventArgs e)
    {
        if (Seleccionado is { } item) AbrirVisor(item);
    }

    private void DgAdjuntos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is not AdjuntoListItem) return;
        e.Handled = true;
        if (Seleccionado is { } item) AbrirVisor(item);
    }

    private void AbrirVisor(AdjuntoListItem item)
    {
        if (_sender is null) return;
        var visor = new VisorAdjuntoWindow(_sender, item.Id, item.NombreArchivo, item.Extension)
        {
            Owner = Window.GetWindow(this)
        };
        visor.ShowDialog();
    }

    /// <summary>Abre el adjunto de Word para llenarlo, con el panel ya ajustado a su contrato.</summary>
    private async void BtnLlenarWord_Click(object sender, RoutedEventArgs e)
    {
        if (_sender is null || Seleccionado is not { } item || !ContratosEnWord.EsWord(item.Extension)) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var resultado = await ContratosEnWord.AbrirAdjuntoAsync(_sender, item.Id, item.NombreArchivo);
            if (!resultado.Ok)
            {
                MessageBox.Show(resultado.Mensaje, "Llenar en Word", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var contrato = await _sender.Send(new GetContratoByIdQuery(item.ContratoId));
            LlenadoContratos.PanelWordWindow.Mostrar(contrato);
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo abrir el documento: " + ex.Message, "Llenar en Word",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private async void BtnDescargar_Click(object sender, RoutedEventArgs e)
    {
        if (_sender is null || Seleccionado is not { } item) return;

        var saveDialog = new SaveFileDialog
        {
            Title = "Descargar archivo adjunto",
            FileName = item.NombreArchivo,
            Filter = $"Archivo (*{item.Extension})|*{item.Extension}|Todos los archivos (*.*)|*.*"
        };
        if (saveDialog.ShowDialog(Window.GetWindow(this)) != true) return;

        try
        {
            var adjunto = await _sender.Send(new DescargarDocumentoAdjuntoQuery(item.Id));
            await File.WriteAllBytesAsync(saveDialog.FileName, adjunto.Contenido);
            MessageBox.Show("Archivo guardado correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
    {
        if (_soloLectura || _sender is null || Seleccionado is not { } item) return;

        var descripcion = string.IsNullOrEmpty(item.AsociadoA) ? "" : $"\n(asociado a {item.AsociadoA})";
        var result = MessageBox.Show(
            $"¿Eliminar el adjunto '{item.NombreArchivo}'?{descripcion}",
            "Confirmar eliminación",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            await _sender.Send(new EliminarDocumentoAdjuntoCommand(item.Id));
            await RecargarAsync();
            AdjuntosCambiados?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al eliminar: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ---------- Arrastrar y soltar ----------

    private void DgAdjuntos_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = !_soloLectura && e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private async void DgAdjuntos_Drop(object sender, DragEventArgs e)
    {
        if (_soloLectura || e.Data.GetData(DataFormats.FileDrop) is not string[] archivos || archivos.Length == 0)
            return;
        await AdjuntarAsync(archivos);
    }
}
