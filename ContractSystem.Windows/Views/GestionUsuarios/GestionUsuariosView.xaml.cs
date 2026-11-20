using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Windows.ViewModels;

namespace ContractSystem.Windows.Views.GestionUsuarios
{
    public partial class GestionUsuariosView : UserControl
    {
        public GestionUsuariosView()
        {
            InitializeComponent();
        }

        private void GridUsuarios_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid dg || dg.SelectedItem is null) return;
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is null) return;
            if (DataContext is not GestionUsuariosViewModel vm) return;

            if (vm.EditarUsuarioCommand.CanExecute(null))
            {
                e.Handled = true;
                vm.EditarUsuarioCommand.Execute(null);
            }
        }
    }
}
