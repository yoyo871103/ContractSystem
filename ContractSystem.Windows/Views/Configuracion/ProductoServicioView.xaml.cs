using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Windows.ViewModels;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class ProductoServicioView : UserControl
{
    public ProductoServicioView()
    {
        InitializeComponent();
    }

    private void CmbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ProductoServicioViewModel vm) return;
        if (CmbFiltroTipo.SelectedItem is not ComboBoxItem item) return;

        var tag = item.Tag?.ToString();
        vm.FiltroTipo = tag switch
        {
            "0" => TipoProductoServicio.Producto,
            "1" => TipoProductoServicio.Servicio,
            _ => null
        };
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ProductoServicioViewModel vm && vm.EditarCommand.CanExecute(null))
            vm.EditarCommand.Execute(null);
    }
}
