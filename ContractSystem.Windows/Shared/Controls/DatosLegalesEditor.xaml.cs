using System.Windows.Controls;

namespace ContractSystem.Windows.Shared.Controls;

/// <summary>
/// Editor de datos legales (REEUP, registro mercantil, constitución, representantes y cuentas bancarias).
/// Se usa igual en la ficha de la entidad propia y en la de los terceros.
/// DataContext: <see cref="ViewModels.Shared.DatosLegalesEditModel"/>.
/// </summary>
public partial class DatosLegalesEditor : UserControl
{
    public DatosLegalesEditor()
    {
        InitializeComponent();
    }
}
