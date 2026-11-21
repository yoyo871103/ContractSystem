using System.Windows;
using ContractSystem.Windows.ViewModels.LlenadoContratos;

namespace ContractSystem.Windows.Views.LlenadoContratos;

/// <summary>Revisión de los datos encontrados en la proforma antes de escribirlos en Word.</summary>
public partial class RevisionAutollenadoWindow : Window
{
    public RevisionAutollenadoWindow(RevisionAutollenadoViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void BtnLlenar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
