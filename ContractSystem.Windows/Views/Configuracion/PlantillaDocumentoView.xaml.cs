using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Windows.ViewModels;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class PlantillaDocumentoView : UserControl
{
    public PlantillaDocumentoView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Doble clic sobre una plantilla: abrirla en el visor sin descargarla.
    /// </summary>
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement fe || fe.DataContext is not PlantillaDocumento) return;
        if (DataContext is PlantillaDocumentoViewModel vm && vm.VerCommand.CanExecute(null))
            vm.VerCommand.Execute(null);
    }
}
