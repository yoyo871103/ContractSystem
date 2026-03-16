using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ContractSystem.Application.Contratos;
using ContractSystem.Application.Contratos.Queries.GetAllContratos;
using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Windows.Views.Contratos;

public partial class MapaModificacionesWindow : Window
{
    private readonly ISender _sender;
    private readonly IModificacionDocumentoStore _modStore;
    private readonly Contrato _contrato;

    public MapaModificacionesWindow(ISender sender, IModificacionDocumentoStore modStore, Contrato contrato)
    {
        InitializeComponent();
        _sender = sender;
        _modStore = modStore;
        _contrato = contrato;

        TxtTitulo.Text = $"Mapa de Modificaciones — {contrato.Numero}";
        TxtSubtitulo.Text = $"{contrato.TipoDocumento} | {contrato.Objeto}";

        Loaded += async (_, _) => await CargarDiagramaAsync();
    }

    private async Task CargarDiagramaAsync()
    {
        try
        {
            // Cargar todos los contratos para resolver nombres
            var todos = await _sender.Send(new GetAllContratosQuery());
            var porId = todos.ToDictionary(c => c.Id);

            // Determinar la raíz de la jerarquía
            var raizId = _contrato.ContratoPadreId ?? _contrato.Id;
            if (!porId.ContainsKey(raizId)) raizId = _contrato.Id;
            var raiz = porId[raizId];

            // Obtener todos los hijos del padre raíz
            var familia = todos.Where(c => c.Id == raizId || c.ContratoPadreId == raizId)
                               .OrderBy(c => c.Id == raizId ? 0 : 1)
                               .ThenBy(c => c.TipoDocumento)
                               .ThenBy(c => c.Numero)
                               .ToList();

            // Cargar todas las modificaciones entre miembros de la familia
            var modificaciones = new List<(ModificacionDocumento mod, string origenNumero, string destinoNumero)>();
            foreach (var doc in familia)
            {
                var mods = await _modStore.GetModificaAAsync(doc.Id);
                foreach (var m in mods)
                {
                    var origenNumero = porId.TryGetValue(m.DocumentoOrigenId, out var orig) ? orig.Numero : $"#{m.DocumentoOrigenId}";
                    var destinoNumero = porId.TryGetValue(m.DocumentoDestinoId, out var dest) ? dest.Numero : $"#{m.DocumentoDestinoId}";
                    modificaciones.Add((m, origenNumero, destinoNumero));
                }
            }

            PnlDiagrama.Children.Clear();

            // --- Sección 1: Jerarquía de documentos ---
            var headerJerarquia = new TextBlock
            {
                Text = "Jerarquía de documentos",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("BrushAccent"),
                Margin = new Thickness(0, 0, 0, 8)
            };
            PnlDiagrama.Children.Add(headerJerarquia);

            // Dibujar el padre
            PnlDiagrama.Children.Add(CrearTarjetaDocumento(raiz, esPadre: true, esSeleccionado: raiz.Id == _contrato.Id));

            // Dibujar hijos con línea conectora
            var hijos = familia.Where(c => c.Id != raizId).ToList();
            foreach (var hijo in hijos)
            {
                var filaHijo = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 0, 0, 0) };

                // Línea conectora vertical + horizontal
                var conector = new TextBlock
                {
                    Text = "├─",
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 14,
                    Foreground = (Brush)FindResource("BrushTextInactive"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 4, 0)
                };
                filaHijo.Children.Add(conector);
                filaHijo.Children.Add(CrearTarjetaDocumento(hijo, esPadre: false, esSeleccionado: hijo.Id == _contrato.Id));
                PnlDiagrama.Children.Add(filaHijo);
            }

            // --- Sección 2: Relaciones de modificación ---
            var headerMods = new TextBlock
            {
                Text = "Relaciones de modificación",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("BrushAccent"),
                Margin = new Thickness(0, 20, 0, 8)
            };
            PnlDiagrama.Children.Add(headerMods);

            if (modificaciones.Count == 0)
            {
                PnlDiagrama.Children.Add(new TextBlock
                {
                    Text = "No hay relaciones de modificación registradas.",
                    FontSize = 12,
                    Foreground = (Brush)FindResource("BrushTextInactive"),
                    Margin = new Thickness(0, 0, 0, 8)
                });
            }
            else
            {
                foreach (var (mod, origenNum, destinoNum) in modificaciones)
                {
                    PnlDiagrama.Children.Add(CrearFilaModificacion(mod, origenNum, destinoNum, porId));
                }
            }
        }
        catch (Exception ex)
        {
            PnlDiagrama.Children.Clear();
            PnlDiagrama.Children.Add(new TextBlock
            {
                Text = "Error al cargar: " + ex.Message,
                Foreground = Brushes.Red,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap
            });
        }
    }

    private Border CrearTarjetaDocumento(Contrato doc, bool esPadre, bool esSeleccionado)
    {
        var colorFondo = esPadre ? "#E3F2FD" : "#E8F5E9";
        var colorBorde = esPadre ? "#0078D4" : "#28A745";
        if (esSeleccionado) colorBorde = "#E67E00";

        var tipoIcon = doc.TipoDocumento switch
        {
            TipoDocumentoContrato.Marco => "\uE8A5",
            TipoDocumentoContrato.Especifico => "\uE7C3",
            TipoDocumentoContrato.Independiente => "\uE729",
            TipoDocumentoContrato.Suplemento => "\uE70F",
            _ => "\uE7C3"
        };

        var estadoColor = doc.Estado switch
        {
            EstadoContrato.Borrador => "#888888",
            EstadoContrato.Vigente => "#28A745",
            EstadoContrato.Vencido => "#FFC107",
            EstadoContrato.Rescindido => "#DC3545",
            EstadoContrato.Ejecutado => "#17A2B8",
            _ => "#333333"
        };

        var sp = new StackPanel { Orientation = Orientation.Horizontal };

        sp.Children.Add(new TextBlock
        {
            Text = tipoIcon,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 16,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorBorde)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });

        var info = new StackPanel();
        info.Children.Add(new TextBlock
        {
            Text = $"{doc.Numero}  [{doc.TipoDocumento}]",
            FontSize = 12,
            FontWeight = FontWeights.SemiBold
        });

        var detalle = new StackPanel { Orientation = Orientation.Horizontal };
        detalle.Children.Add(new TextBlock
        {
            Text = doc.Estado.ToString(),
            FontSize = 10,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(estadoColor)),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 8, 0)
        });
        if (doc.Tercero != null)
        {
            detalle.Children.Add(new TextBlock
            {
                Text = doc.Tercero.Nombre,
                FontSize = 10,
                Foreground = (Brush)FindResource("BrushTextInactive")
            });
        }
        info.Children.Add(detalle);

        var objeto = doc.Objeto.Length > 80 ? doc.Objeto[..80] + "..." : doc.Objeto;
        info.Children.Add(new TextBlock
        {
            Text = objeto,
            FontSize = 10,
            Foreground = (Brush)FindResource("BrushTextInactive"),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 500
        });

        sp.Children.Add(info);

        return new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorFondo)),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorBorde)),
            BorderThickness = esSeleccionado ? new Thickness(2) : new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 2, 0, 2),
            Child = sp
        };
    }

    private UIElement CrearFilaModificacion(ModificacionDocumento mod, string origenNum, string destinoNum,
        Dictionary<int, Contrato> porId)
    {
        var fila = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8E1")),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE082")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 2, 0, 2)
        };

        var sp = new StackPanel();

        // Línea principal: Origen → Destino
        var lineaPrincipal = new StackPanel { Orientation = Orientation.Horizontal };

        // Origen
        var origenTipo = porId.TryGetValue(mod.DocumentoOrigenId, out var orig)
            ? orig.TipoDocumento.ToString() : "";
        lineaPrincipal.Children.Add(new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(6, 2, 6, 2),
            Child = new TextBlock
            {
                Text = $"{origenNum} [{origenTipo}]",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            }
        });

        // Flecha
        lineaPrincipal.Children.Add(new TextBlock
        {
            Text = "  ──→  ",
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E67E00")),
            VerticalAlignment = VerticalAlignment.Center
        });

        lineaPrincipal.Children.Add(new TextBlock
        {
            Text = "modifica a",
            FontSize = 10,
            Foreground = (Brush)FindResource("BrushTextInactive"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });

        // Destino
        var destinoTipo = porId.TryGetValue(mod.DocumentoDestinoId, out var dest)
            ? dest.TipoDocumento.ToString() : "";
        lineaPrincipal.Children.Add(new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(6, 2, 6, 2),
            Child = new TextBlock
            {
                Text = $"{destinoNum} [{destinoTipo}]",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            }
        });

        sp.Children.Add(lineaPrincipal);

        // Descripción de la modificación
        if (!string.IsNullOrWhiteSpace(mod.Descripcion))
        {
            sp.Children.Add(new TextBlock
            {
                Text = mod.Descripcion,
                FontSize = 11,
                Foreground = (Brush)FindResource("BrushTextInactive"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0),
                MaxWidth = 600
            });
        }

        // Fecha
        sp.Children.Add(new TextBlock
        {
            Text = $"Registrada: {mod.FechaRegistro:dd/MM/yyyy}",
            FontSize = 10,
            Foreground = (Brush)FindResource("BrushTextInactive"),
            Margin = new Thickness(0, 2, 0, 0)
        });

        fila.Child = sp;
        return fila;
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else
            DragMove();
    }
}
