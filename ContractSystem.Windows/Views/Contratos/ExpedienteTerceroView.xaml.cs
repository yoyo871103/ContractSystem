using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos.Queries.GetAllContratos;
using ContractSystem.Windows.ViewModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Contratos;

public partial class ExpedienteTerceroView : UserControl
{
    public ExpedienteTerceroView()
    {
        InitializeComponent();
    }

    private async void TvExpediente_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TvExpediente.SelectedItem is not NodoExpediente nodo) return;
        if (nodo.EsRaiz || nodo.ContratoId is null) return;

        try
        {
            var mediator = App.Services.GetRequiredService<ISender>();
            var todos = await mediator.Send(new GetAllContratosQuery());
            var contrato = todos.FirstOrDefault(c => c.Id == nodo.ContratoId);
            if (contrato is null) return;

            var window = new DetalleContratoWindow(mediator, contrato);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir detalle: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
