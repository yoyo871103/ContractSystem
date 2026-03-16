using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos;
using ContractSystem.Domain.Contratos;
using ContractSystem.Windows.ViewModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Views.Contratos;

public partial class ContratosView : UserControl
{
    public ContratosView()
    {
        InitializeComponent();
    }

    private void CmbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ContratosViewModel vm) return;
        if (CmbFiltroTipo.SelectedItem is not ComboBoxItem item) return;

        vm.FiltroTipo = item.Tag?.ToString() switch
        {
            "0" => TipoDocumentoContrato.Marco,
            "1" => TipoDocumentoContrato.Especifico,
            "2" => TipoDocumentoContrato.Independiente,
            "3" => TipoDocumentoContrato.Suplemento,
            _ => null
        };
    }

    private void CmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ContratosViewModel vm) return;
        if (CmbFiltroEstado.SelectedItem is not ComboBoxItem item) return;

        vm.FiltroEstado = item.Tag?.ToString() switch
        {
            "0" => EstadoContrato.Borrador,
            "1" => EstadoContrato.Vigente,
            "2" => EstadoContrato.Vencido,
            "3" => EstadoContrato.Rescindido,
            "4" => EstadoContrato.Ejecutado,
            _ => null
        };
    }

    private void CmbFiltroRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ContratosViewModel vm) return;
        if (CmbFiltroRol.SelectedItem is not ComboBoxItem item) return;

        vm.FiltroRol = item.Tag?.ToString() switch
        {
            "0" => RolContrato.Proveedor,
            "1" => RolContrato.Cliente,
            _ => null
        };
    }

    private void CmbFiltroTercero_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ContratosViewModel vm) return;
        if (CmbFiltroTercero.SelectedItem is ContractSystem.Domain.Nomencladores.Tercero tercero)
            vm.FiltroTerceroId = tercero.Id;
    }

    private void BtnLimpiarTercero_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        CmbFiltroTercero.SelectedItem = null;
        if (DataContext is ContratosViewModel vm)
            vm.FiltroTerceroId = null;
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ContratosViewModel vm && vm.EditarCommand.CanExecute(null))
            vm.EditarCommand.Execute(null);
    }

    private void CtxVerMapa_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ContratosViewModel vm || vm.Seleccionado is null) return;

        var mediator = App.Services.GetRequiredService<ISender>();
        var modStore = App.Services.GetRequiredService<IModificacionDocumentoStore>();
        var window = new MapaModificacionesWindow(mediator, modStore, vm.Seleccionado);
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
    }
}
