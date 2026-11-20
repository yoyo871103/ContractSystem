using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using ContractSystem.Domain.Contratos;

namespace ContractSystem.Windows.Views.Contratos;

public partial class HistorialCambioDetalleWindow : Window
{
    public HistorialCambioDetalleWindow(HistorialCambio cambio)
    {
        InitializeComponent();
        TxtFecha.Text = cambio.FechaHora.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        TxtTipo.Text = cambio.TipoCambio.ToString();
        TxtUsuario.Text = string.IsNullOrWhiteSpace(cambio.UsuarioNombre) ? "—" : cambio.UsuarioNombre;
        TxtDescripcion.Text = string.IsNullOrWhiteSpace(cambio.Descripcion) ? "—" : cambio.Descripcion;
        TxtValorAnterior.Text = FormatearJson(cambio.ValorAnterior);
        TxtValorNuevo.Text = FormatearJson(cambio.ValorNuevo);
    }

    private static string FormatearJson(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "—";
        try
        {
            using var doc = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return raw;
        }
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
