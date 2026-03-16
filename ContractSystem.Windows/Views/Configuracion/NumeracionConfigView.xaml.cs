using System.Windows.Controls;
using System.Windows.Input;

namespace ContractSystem.Windows.Views.Configuracion;

public partial class NumeracionConfigView : UserControl
{
    public NumeracionConfigView()
    {
        InitializeComponent();
    }

    private void OnlyNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !int.TryParse(e.Text, out _);
    }
}
