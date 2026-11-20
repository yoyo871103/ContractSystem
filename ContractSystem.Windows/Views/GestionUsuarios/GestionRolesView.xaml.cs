using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Windows.ViewModels;

namespace ContractSystem.Windows.Views.GestionUsuarios;

public partial class GestionRolesView : UserControl
{
    public GestionRolesView()
    {
        InitializeComponent();
    }

    private void GridRoles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dg || dg.SelectedItem is null) return;
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is null) return;
        if (DataContext is not GestionRolesViewModel vm) return;

        if (vm.EditarRolCommand.CanExecute(null))
        {
            e.Handled = true;
            vm.EditarRolCommand.Execute(null);
        }
    }
}
