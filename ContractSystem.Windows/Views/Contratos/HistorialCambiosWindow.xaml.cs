using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos.Queries.GetHistorialByContrato;
using ContractSystem.Domain.Contratos;
using MediatR;

namespace ContractSystem.Windows.Views.Contratos;

public partial class HistorialCambiosWindow : Window
{
    private readonly ISender _sender;
    private readonly int _contratoId;
    private readonly ObservableCollection<HistorialCambio> _historial = new();

    public HistorialCambiosWindow(ISender sender, int contratoId, string contratoNumero)
    {
        InitializeComponent();
        _sender = sender;
        _contratoId = contratoId;
        TxtTitulo.Text = $"Historial — {contratoNumero}";
        DgHistorial.ItemsSource = _historial;
        Loaded += async (_, _) => await CargarAsync();
    }

    private async Task CargarAsync(TipoCambio? filtro = null)
    {
        try
        {
            var lista = await _sender.Send(new GetHistorialByContratoQuery(_contratoId, filtro));
            _historial.Clear();
            foreach (var h in lista)
                _historial.Add(h);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cargar historial: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_sender is null || !IsLoaded) return;
        if (CmbFiltro.SelectedItem is ComboBoxItem item)
        {
            var tag = item.Tag?.ToString();
            TipoCambio? filtro = string.IsNullOrEmpty(tag) ? null : (TipoCambio)int.Parse(tag);
            await CargarAsync(filtro);
        }
    }

    private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
}
