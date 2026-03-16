using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Windows.ViewModels;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class TerceroView : UserControl
{
    public TerceroView()
    {
        InitializeComponent();
    }

    private void CmbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not TerceroViewModel vm) return;
        if (CmbFiltroTipo.SelectedItem is not ComboBoxItem item) return;

        var tag = item.Tag?.ToString();
        vm.FiltroTipo = tag switch
        {
            "0" => TipoTercero.Cliente,
            "1" => TipoTercero.Proveedor,
            "2" => TipoTercero.Ambos,
            _ => null
        };
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is TerceroViewModel vm && vm.EditarCommand.CanExecute(null))
            vm.EditarCommand.Execute(null);
    }
}
