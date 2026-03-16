using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos;
using ContractSystem.Application.Contratos.Queries.GetAnexosByContrato;
using ContractSystem.Application.Contratos.Queries.GetDocumentosAdjuntos;
using ContractSystem.Application.Contratos.Queries.GetLineasByContrato;
using ContractSystem.Domain.Contratos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Contratos;

public partial class DetalleContratoWindow : Window
{
    private readonly ISender _sender;
    private readonly Contrato _contrato;

    public DetalleContratoWindow(ISender sender, Contrato contrato)
    {
        InitializeComponent();
        _sender = sender;
        _contrato = contrato;

        CargarDatosGenerales();
        Loaded += async (_, _) => await CargarDetallesAsync();
    }

    private void CargarDatosGenerales()
    {
        TxtTitulo.Text = $"Detalle — {_contrato.Numero}";
        TxtNumero.Text = _contrato.Numero;
        TxtTipo.Text = _contrato.TipoDocumento.ToString();
        TxtEstado.Text = _contrato.Estado.ToString();
        TxtRol.Text = _contrato.Rol.ToString();
        TxtTercero.Text = _contrato.Tercero?.Nombre ?? "—";
        TxtValorTotal.Text = _contrato.ValorTotal?.ToString("N2", CultureInfo.CurrentCulture) ?? "—";
        TxtFechaFirma.Text = _contrato.FechaFirma?.ToString("dd/MM/yyyy") ?? "—";
        TxtFechaVigencia.Text = _contrato.FechaVigencia?.ToString("dd/MM/yyyy") ?? "—";
        TxtObjeto.Text = _contrato.Objeto;
        TxtDuracion.Text = _contrato.Duracion ?? "—";

        // Color del estado
        TxtEstado.Foreground = _contrato.Estado switch
        {
            EstadoContrato.Vigente => new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#28A745")),
            EstadoContrato.Vencido => new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFC107")),
            EstadoContrato.Rescindido => new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC3545")),
            EstadoContrato.Ejecutado => new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#17A2B8")),
            _ => new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#888888")),
        };
    }

    private async Task CargarDetallesAsync()
    {
        try
        {
            // Cargar anexos y sus líneas
            var anexos = await _sender.Send(new GetAnexosByContratoQuery(_contrato.Id));
            var lineas = await _sender.Send(new GetLineasByContratoQuery(_contrato.Id));

            PnlAnexos.Children.Clear();

            if (anexos.Count == 0)
            {
                PnlAnexos.Children.Add(new TextBlock
                {
                    Text = "No hay anexos registrados.",
                    FontSize = 12,
                    Foreground = (System.Windows.Media.Brush)FindResource("BrushTextInactive"),
                    Margin = new Thickness(0, 0, 0, 8)
                });
            }
            else
            {
                foreach (var anexo in anexos)
                {
                    var lineasAnexo = lineas.Where(l => l.AnexoId == anexo.Id).ToList();

                    var header = new TextBlock
                    {
                        Text = $"{anexo.Nombre} ({lineasAnexo.Count} línea(s))",
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 12,
                        Margin = new Thickness(0, 4, 0, 2)
                    };
                    PnlAnexos.Children.Add(header);

                    if (lineasAnexo.Count > 0)
                    {
                        foreach (var linea in lineasAnexo)
                        {
                            var txt = new TextBlock
                            {
                                Text = $"  {linea.Concepto}  —  {linea.Cantidad:N2} x {linea.PrecioUnitario:N2} = {linea.ImporteTotal:N2}",
                                FontSize = 11,
                                Foreground = (System.Windows.Media.Brush)FindResource("BrushTextInactive"),
                                Margin = new Thickness(8, 0, 0, 1)
                            };
                            PnlAnexos.Children.Add(txt);
                        }

                        var total = lineasAnexo.Sum(l => l.ImporteTotal);
                        var totalTxt = new TextBlock
                        {
                            Text = $"  Total anexo: {total:N2}",
                            FontWeight = FontWeights.SemiBold,
                            FontSize = 11,
                            Margin = new Thickness(8, 2, 0, 6)
                        };
                        PnlAnexos.Children.Add(totalTxt);
                    }
                }
            }

            // Cargar adjuntos
            var adjuntos = await _sender.Send(new GetDocumentosAdjuntosQuery(_contrato.Id));
            var adjuntoItems = adjuntos.Select(a => new
            {
                a.NombreArchivo,
                a.Extension,
                a.Objetivo,
                TamanioTexto = FormatBytes(a.TamanioBytes),
                a.FechaCarga
            }).ToList();
            DgAdjuntos.ItemsSource = adjuntoItems;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar detalles: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:N1} KB",
        _ => $"{bytes / (1024.0 * 1024.0):N1} MB"
    };

    private void BtnAbrirAnexos_Click(object sender, RoutedEventArgs e)
    {
        var window = new AnexosLineasWindow(_sender, _contrato.Id, _contrato.Numero);
        window.Owner = this;
        window.ShowDialog();
        _ = CargarDetallesAsync();
    }

    private void BtnAbrirAdjuntos_Click(object sender, RoutedEventArgs e)
    {
        var window = new DocumentosAdjuntosWindow(_sender, _contrato.Id, _contrato.Numero);
        window.Owner = this;
        window.ShowDialog();
        _ = CargarDetallesAsync();
    }

    private void BtnAbrirMapa_Click(object sender, RoutedEventArgs e)
    {
        var modStore = App.Services.GetRequiredService<IModificacionDocumentoStore>();
        var window = new MapaModificacionesWindow(_sender, modStore, _contrato);
        window.Owner = this;
        window.ShowDialog();
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
