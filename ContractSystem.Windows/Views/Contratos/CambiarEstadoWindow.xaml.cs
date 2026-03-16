using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Domain.Contratos;

namespace ContractSystem.Windows.Views.Contratos;

public partial class CambiarEstadoWindow : Window
{
    public EstadoContrato NuevoEstado { get; private set; }

    public CambiarEstadoWindow(EstadoContrato estadoActual)
    {
        InitializeComponent();
        TxtEstadoActual.Text = $"Estado actual: {estadoActual}";

        foreach (EstadoContrato estado in Enum.GetValues<EstadoContrato>())
        {
            if (estado == estadoActual) continue;
            CmbEstado.Items.Add(new ComboBoxItem { Content = estado.ToString(), Tag = estado });
        }

        if (CmbEstado.Items.Count > 0)
            CmbEstado.SelectedIndex = 0;
    }

    private void BtnCambiar_Click(object sender, RoutedEventArgs e)
    {
        if (CmbEstado.SelectedItem is ComboBoxItem item && item.Tag is EstadoContrato estado)
        {
            NuevoEstado = estado;
            DialogResult = true;
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) return;
        DragMove();
    }
}
