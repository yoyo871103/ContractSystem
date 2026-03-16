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

    // Layout constants for the visual diagram
    private const double CardWidth = 260;
    private const double CardHeight = 80;
    private const double HGap = 60;
    private const double VGap = 100;
    private const double PadLeft = 40;
    private const double PadTop = 40;

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
            var todos = await _sender.Send(new GetAllContratosQuery());
            var porId = todos.ToDictionary(c => c.Id);

            var raizId = _contrato.ContratoPadreId ?? _contrato.Id;
            if (!porId.ContainsKey(raizId)) raizId = _contrato.Id;
            var raiz = porId[raizId];

            var familia = todos.Where(c => c.Id == raizId || c.ContratoPadreId == raizId)
                               .OrderBy(c => c.Id == raizId ? 0 : 1)
                               .ThenBy(c => c.TipoDocumento == TipoDocumentoContrato.Suplemento ? 1 : 0)
                               .ThenBy(c => c.Numero)
                               .ToList();

            // Collect all modifications
            var modificaciones = new List<ModificacionDocumento>();
            foreach (var doc in familia)
            {
                var mods = await _modStore.GetModificaAAsync(doc.Id);
                modificaciones.AddRange(mods);
            }

            // --- Tab 1: Visual Canvas Diagram ---
            DibujarDiagramaVisual(raiz, familia, modificaciones, porId);

            // --- Tab 2: List view ---
            DibujarListaRelaciones(raiz, familia, modificaciones, porId);
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

    #region Visual Canvas Diagram

    private void DibujarDiagramaVisual(Contrato raiz, List<Contrato> familia,
        List<ModificacionDocumento> modificaciones, Dictionary<int, Contrato> porId)
    {
        CnvDiagrama.Children.Clear();

        // Separate children: contratos (especificos/independientes) and suplementos
        var hijos = familia.Where(c => c.Id != raiz.Id).ToList();
        var contratos = hijos.Where(c => c.TipoDocumento != TipoDocumentoContrato.Suplemento).ToList();
        var suplementos = hijos.Where(c => c.TipoDocumento == TipoDocumentoContrato.Suplemento).ToList();

        // Layout: Row 0 = root, Row 1 = contratos hijos, Row 2 = suplementos
        // Each row's items are centered horizontally
        var cardPositions = new Dictionary<int, (double x, double y)>();

        // Root position
        double rootX = PadLeft;
        double rootY = PadTop;

        // Calculate total width needed
        int maxCols = Math.Max(1, Math.Max(contratos.Count, suplementos.Count));
        double totalWidth = maxCols * (CardWidth + HGap) - HGap;

        // Center root
        rootX = PadLeft + (totalWidth - CardWidth) / 2;
        if (rootX < PadLeft) rootX = PadLeft;
        cardPositions[raiz.Id] = (rootX, rootY);

        // Row 1: contratos hijos
        double row1Y = rootY + CardHeight + VGap;
        LayoutRow(contratos, row1Y, totalWidth, cardPositions);

        // Row 2: suplementos
        double row2Y = row1Y + (contratos.Count > 0 ? CardHeight + VGap : 0);
        if (contratos.Count == 0 && suplementos.Count > 0)
            row2Y = row1Y;
        LayoutRow(suplementos, row2Y, totalWidth, cardPositions);

        // Set canvas size
        double maxX = cardPositions.Values.Max(p => p.x) + CardWidth + PadLeft;
        double maxY = cardPositions.Values.Max(p => p.y) + CardHeight + PadTop + 40;
        CnvDiagrama.Width = Math.Max(700, maxX);
        CnvDiagrama.Height = Math.Max(400, maxY);

        // Draw hierarchy arrows (parent → children) as thin gray lines
        foreach (var hijo in hijos)
        {
            if (!cardPositions.ContainsKey(hijo.Id)) continue;
            var (px, py) = cardPositions[raiz.Id];
            var (cx, cy) = cardPositions[hijo.Id];
            DibujarFlechaJerarquia(px + CardWidth / 2, py + CardHeight,
                                    cx + CardWidth / 2, cy,
                                    "#BBBBBB");
        }

        // Draw modification arrows (source → target) as colored curved arrows
        foreach (var mod in modificaciones)
        {
            if (!cardPositions.ContainsKey(mod.DocumentoOrigenId) ||
                !cardPositions.ContainsKey(mod.DocumentoDestinoId)) continue;

            var (ox, oy) = cardPositions[mod.DocumentoOrigenId];
            var (dx, dy) = cardPositions[mod.DocumentoDestinoId];

            DibujarFlechaModificacion(ox, oy, dx, dy, "#0078D4", mod.Descripcion);
        }

        // Draw cards on top of arrows
        foreach (var doc in familia)
        {
            if (!cardPositions.ContainsKey(doc.Id)) continue;
            var (x, y) = cardPositions[doc.Id];
            var card = CrearTarjetaVisual(doc, doc.Id == raiz.Id, doc.Id == _contrato.Id);
            Canvas.SetLeft(card, x);
            Canvas.SetTop(card, y);
            CnvDiagrama.Children.Add(card);
        }
    }

    private void LayoutRow(List<Contrato> items, double y, double totalWidth,
        Dictionary<int, (double x, double y)> positions)
    {
        if (items.Count == 0) return;

        double rowWidth = items.Count * (CardWidth + HGap) - HGap;
        double startX = PadLeft + (totalWidth - rowWidth) / 2;
        if (startX < PadLeft) startX = PadLeft;

        for (int i = 0; i < items.Count; i++)
        {
            double x = startX + i * (CardWidth + HGap);
            positions[items[i].Id] = (x, y);
        }
    }

    private void DibujarFlechaJerarquia(double x1, double y1, double x2, double y2, string colorHex)
    {
        var color = (Color)ColorConverter.ConvertFromString(colorHex);
        var brush = new SolidColorBrush(color);

        var line = new Line
        {
            X1 = x1, Y1 = y1,
            X2 = x2, Y2 = y2,
            Stroke = brush,
            StrokeThickness = 1.5,
            StrokeDashArray = new DoubleCollection { 4, 3 }
        };
        CnvDiagrama.Children.Add(line);

        // Small arrow head
        DibujarCabezaFlecha(x2, y2, x2 - x1, y2 - y1, brush, 8);
    }

    private void DibujarFlechaModificacion(double ox, double oy, double dx, double dy,
        string colorHex, string? descripcion)
    {
        var color = (Color)ColorConverter.ConvertFromString(colorHex);
        var brush = new SolidColorBrush(color);

        // Determine connection points based on relative positions
        double startX, startY, endX, endY;

        // Source: determine best exit point
        // Target: determine best entry point
        double oCenterX = ox + CardWidth / 2;
        double oCenterY = oy + CardHeight / 2;
        double dCenterX = dx + CardWidth / 2;
        double dCenterY = dy + CardHeight / 2;

        double diffX = dCenterX - oCenterX;
        double diffY = dCenterY - oCenterY;

        // If same row or close, use side connections
        if (Math.Abs(diffY) < CardHeight)
        {
            // Horizontal: right side of source → left side of target (or vice versa)
            if (diffX > 0)
            {
                startX = ox + CardWidth;
                startY = oCenterY;
                endX = dx;
                endY = dCenterY;
            }
            else
            {
                startX = ox;
                startY = oCenterY;
                endX = dx + CardWidth;
                endY = dCenterY;
            }
        }
        else if (diffY > 0)
        {
            // Target is below: bottom of source → top of target
            startX = oCenterX + 30; // offset to not overlap hierarchy arrow
            startY = oy + CardHeight;
            endX = dCenterX + 30;
            endY = dy;
        }
        else
        {
            // Target is above: top of source → bottom of target
            startX = oCenterX + 30;
            startY = oy;
            endX = dCenterX + 30;
            endY = dy + CardHeight;
        }

        // Draw Bezier curve
        var midY = (startY + endY) / 2;
        var ctrl1 = new Point(startX, midY);
        var ctrl2 = new Point(endX, midY);

        // For same-row connections, use a wider curve
        if (Math.Abs(diffY) < CardHeight)
        {
            double curveOffset = Math.Min(80, Math.Abs(diffX) * 0.4);
            ctrl1 = new Point(startX + (diffX > 0 ? curveOffset : -curveOffset), startY - curveOffset);
            ctrl2 = new Point(endX + (diffX > 0 ? -curveOffset : curveOffset), endY - curveOffset);
        }

        var pathFigure = new PathFigure { StartPoint = new Point(startX, startY) };
        pathFigure.Segments.Add(new BezierSegment(ctrl1, ctrl2, new Point(endX, endY), true));

        var pathGeometry = new PathGeometry();
        pathGeometry.Figures.Add(pathFigure);

        var path = new Path
        {
            Data = pathGeometry,
            Stroke = brush,
            StrokeThickness = 2.5,
            Fill = Brushes.Transparent
        };
        CnvDiagrama.Children.Add(path);

        // Arrow head at end
        double tangentX = endX - ctrl2.X;
        double tangentY = endY - ctrl2.Y;
        DibujarCabezaFlecha(endX, endY, tangentX, tangentY, brush, 12);

        // Label "modifica" near midpoint
        if (!string.IsNullOrWhiteSpace(descripcion))
        {
            double labelX = (startX + endX) / 2;
            double labelY = (startY + endY) / 2 - 14;
            var label = new TextBlock
            {
                Text = descripcion.Length > 30 ? descripcion[..30] + "..." : descripcion,
                FontSize = 9,
                Foreground = brush,
                FontStyle = FontStyles.Italic,
                Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255))
            };
            Canvas.SetLeft(label, labelX - 40);
            Canvas.SetTop(label, labelY);
            CnvDiagrama.Children.Add(label);
        }
    }

    private void DibujarCabezaFlecha(double tipX, double tipY, double dirX, double dirY,
        Brush brush, double size)
    {
        double len = Math.Sqrt(dirX * dirX + dirY * dirY);
        if (len < 0.001) return;
        double nx = dirX / len;
        double ny = dirY / len;

        double perpX = -ny;
        double perpY = nx;

        var p1 = new Point(tipX - nx * size + perpX * size * 0.4, tipY - ny * size + perpY * size * 0.4);
        var p2 = new Point(tipX - nx * size - perpX * size * 0.4, tipY - ny * size - perpY * size * 0.4);
        var tip = new Point(tipX, tipY);

        var polygon = new Polygon
        {
            Points = new PointCollection { tip, p1, p2 },
            Fill = brush,
            Stroke = brush,
            StrokeThickness = 1
        };
        CnvDiagrama.Children.Add(polygon);
    }

    private Border CrearTarjetaVisual(Contrato doc, bool esPadre, bool esSeleccionado)
    {
        string colorFondo, colorBorde;
        if (doc.TipoDocumento == TipoDocumentoContrato.Suplemento)
        {
            colorFondo = "#FFF8E1";
            colorBorde = "#E67E00";
        }
        else if (esPadre)
        {
            colorFondo = "#E3F2FD";
            colorBorde = "#0078D4";
        }
        else
        {
            colorFondo = "#E8F5E9";
            colorBorde = "#28A745";
        }

        var estadoColor = doc.Estado switch
        {
            EstadoContrato.Borrador => "#888888",
            EstadoContrato.Vigente => "#28A745",
            EstadoContrato.Vencido => "#FFC107",
            EstadoContrato.Rescindido => "#DC3545",
            EstadoContrato.Ejecutado => "#17A2B8",
            _ => "#333333"
        };

        var tipoIcon = doc.TipoDocumento switch
        {
            TipoDocumentoContrato.Marco => "\uE8A5",
            TipoDocumentoContrato.Especifico => "\uE7C3",
            TipoDocumentoContrato.Independiente => "\uE729",
            TipoDocumentoContrato.Suplemento => "\uE70F",
            _ => "\uE7C3"
        };

        var sp = new StackPanel { Orientation = Orientation.Horizontal };

        sp.Children.Add(new TextBlock
        {
            Text = tipoIcon,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 22,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorBorde)),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        });

        var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        info.Children.Add(new TextBlock
        {
            Text = $"{doc.Numero}  [{doc.TipoDocumento}]",
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"))
        });

        var detalle = new StackPanel { Orientation = Orientation.Horizontal };
        detalle.Children.Add(new TextBlock
        {
            Text = doc.Estado.ToString(),
            FontSize = 11,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(estadoColor)),
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 8, 0)
        });
        if (doc.Tercero != null)
        {
            detalle.Children.Add(new TextBlock
            {
                Text = doc.Tercero.Nombre,
                FontSize = 11,
                Foreground = new SolidColorBrush(Colors.Gray)
            });
        }
        info.Children.Add(detalle);

        var objeto = doc.Objeto?.Length > 40 ? doc.Objeto[..40] + "..." : doc.Objeto ?? "";
        info.Children.Add(new TextBlock
        {
            Text = objeto,
            FontSize = 10,
            Foreground = new SolidColorBrush(Colors.Gray),
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        sp.Children.Add(info);

        return new Border
        {
            Width = CardWidth,
            Height = CardHeight,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorFondo)),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorBorde)),
            BorderThickness = esSeleccionado ? new Thickness(3) : new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8, 12, 8),
            Child = sp,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 6,
                ShadowDepth = 2,
                Opacity = 0.15,
                Color = Colors.Black
            }
        };
    }

    #endregion

    #region List View (Tab 2)

    private void DibujarListaRelaciones(Contrato raiz, List<Contrato> familia,
        List<ModificacionDocumento> modificaciones, Dictionary<int, Contrato> porId)
    {
        PnlDiagrama.Children.Clear();

        // Header: Jerarquía
        PnlDiagrama.Children.Add(new TextBlock
        {
            Text = "Jerarquía de documentos",
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("BrushAccent"),
            Margin = new Thickness(0, 0, 0, 8)
        });

        PnlDiagrama.Children.Add(CrearTarjetaDocumento(raiz, esPadre: true, esSeleccionado: raiz.Id == _contrato.Id));

        var hijos = familia.Where(c => c.Id != raiz.Id).ToList();
        for (int i = 0; i < hijos.Count; i++)
        {
            var hijo = hijos[i];
            var filaHijo = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 0, 0, 0) };
            var connector = i < hijos.Count - 1 ? "├─" : "└─";
            filaHijo.Children.Add(new TextBlock
            {
                Text = connector,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14,
                Foreground = (Brush)FindResource("BrushTextInactive"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            });
            filaHijo.Children.Add(CrearTarjetaDocumento(hijo, esPadre: false, esSeleccionado: hijo.Id == _contrato.Id));
            PnlDiagrama.Children.Add(filaHijo);
        }

        // Header: Modificaciones
        PnlDiagrama.Children.Add(new TextBlock
        {
            Text = "Relaciones de modificación",
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("BrushAccent"),
            Margin = new Thickness(0, 20, 0, 8)
        });

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
            foreach (var mod in modificaciones)
            {
                var origenNum = porId.TryGetValue(mod.DocumentoOrigenId, out var orig) ? orig.Numero : $"#{mod.DocumentoOrigenId}";
                var destinoNum = porId.TryGetValue(mod.DocumentoDestinoId, out var dest) ? dest.Numero : $"#{mod.DocumentoDestinoId}";
                PnlDiagrama.Children.Add(CrearFilaModificacion(mod, origenNum, destinoNum, porId));
            }
        }
    }

    private Border CrearTarjetaDocumento(Contrato doc, bool esPadre, bool esSeleccionado)
    {
        var colorFondo = esPadre ? "#E3F2FD" : "#E8F5E9";
        var colorBorde = esPadre ? "#0078D4" : "#28A745";
        if (doc.TipoDocumento == TipoDocumentoContrato.Suplemento) { colorFondo = "#FFF8E1"; colorBorde = "#E67E00"; }
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

        var objeto = doc.Objeto?.Length > 80 ? doc.Objeto[..80] + "..." : doc.Objeto ?? "";
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
        var lineaPrincipal = new StackPanel { Orientation = Orientation.Horizontal };

        var origenTipo = porId.TryGetValue(mod.DocumentoOrigenId, out var orig) ? orig.TipoDocumento.ToString() : "";
        lineaPrincipal.Children.Add(new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(6, 2, 6, 2),
            Child = new TextBlock { Text = $"{origenNum} [{origenTipo}]", FontSize = 11, FontWeight = FontWeights.SemiBold }
        });

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

        var destinoTipo = porId.TryGetValue(mod.DocumentoDestinoId, out var dest) ? dest.TipoDocumento.ToString() : "";
        lineaPrincipal.Children.Add(new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD")),
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(6, 2, 6, 2),
            Child = new TextBlock { Text = $"{destinoNum} [{destinoTipo}]", FontSize = 11, FontWeight = FontWeights.SemiBold }
        });

        sp.Children.Add(lineaPrincipal);

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

    #endregion

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.ButtonState == MouseButtonState.Pressed)
            try { DragMove(); } catch { }
    }
}
