using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos.Queries.GetContratoById;
using ContractSystem.Application.Contratos.Queries.GetInformesContratos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Contratos;

public partial class InformesView : UserControl
{
    public InformesView()
    {
        InitializeComponent();
    }

    private async void DgFacturacionPorContrato_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (sender is not DataGrid dg) return;
        if (dg.SelectedItem is not FacturacionPorContratoDto dto) return;
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is not FacturacionPorContratoDto) return;

        await AbrirDetalleContratoAsync(dto.ContratoId);
    }

    private async void DgProximosAVencer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (sender is not DataGrid dg) return;
        if (dg.SelectedItem is not ContratoProximoVencerDto dto) return;
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is not ContratoProximoVencerDto) return;

        await AbrirDetalleContratoAsync(dto.ContratoId);
    }

    private async Task AbrirDetalleContratoAsync(int contratoId)
    {
        try
        {
            var sender = App.Services.GetRequiredService<ISender>();
            var contrato = await sender.Send(new GetContratoByIdQuery(contratoId, IncludeRelaciones: true));
            if (contrato is null)
            {
                MessageBox.Show("No se encontró el contrato.", "Detalle",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var win = new DetalleContratoWindow(sender, contrato)
            {
                Owner = Window.GetWindow(this)
            };
            win.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir el detalle: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
