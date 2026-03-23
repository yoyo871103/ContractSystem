using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Domain.Contratos;

namespace ContractSystem.Windows.Views.Contratos;

public partial class EjecutarContratoWindow : Window
{
    private readonly List<SuplementoCheckItem> _items = new();

    public IReadOnlyList<int> SuplementosSeleccionados =>
        _items.Where(i => i.CheckBox.IsChecked == true).Select(i => i.ContratoId).ToList();

    public EjecutarContratoWindow(Contrato contrato, IReadOnlyList<Contrato> suplementos)
    {
        InitializeComponent();
        TxtTitulo.Text = $"Ejecutar — {contrato.Numero}";

        foreach (var sup in suplementos)
        {
            var cb = new CheckBox
            {
                Content = $"{sup.Numero} — {sup.Objeto}",
                IsChecked = true,
                Margin = new Thickness(0, 0, 0, 6),
                FontSize = 12
            };
            _items.Add(new SuplementoCheckItem(sup.Id, cb));
        }

        ListaSuplementos.ItemsSource = _items.Select(i => i.CheckBox);
    }

    private void BtnSeleccionarTodos_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _items) item.CheckBox.IsChecked = true;
    }

    private void BtnDeseleccionarTodos_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _items) item.CheckBox.IsChecked = false;
    }

    private void BtnEjecutar_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void BtnCancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private record SuplementoCheckItem(int ContratoId, CheckBox CheckBox);
}
