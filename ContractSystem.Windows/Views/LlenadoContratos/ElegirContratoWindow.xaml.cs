using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Domain.Contratos;

namespace ContractSystem.Windows.Views.LlenadoContratos;

/// <summary>
/// Elección del contrato en el que guardar el documento de Word, cuando el panel se abrió sin contrato.
/// Si hay otra parte elegida en el panel, se muestran primero (filtrados) sus contratos.
/// </summary>
public partial class ElegirContratoWindow : Window
{
    private readonly IReadOnlyList<ContratoItem> _todos;
    private readonly int? _terceroId;

    public ElegirContratoWindow(IReadOnlyList<Contrato> contratos, string documento, int? terceroId, string? terceroNombre)
    {
        InitializeComponent();
        _terceroId = terceroId;
        _todos = contratos
            .OrderByDescending(c => c.FechaFirma ?? c.FechaCreacion)
            .Select(c => new ContratoItem(c))
            .ToList();

        TxtDocumento.Text = $"Documento: «{documento}». Se guardará como adjunto del contrato elegido.";
        if (terceroId is not null && _todos.Any(c => c.Contrato.TerceroId == terceroId))
        {
            ChkSoloTercero.Content = $"Solo contratos con {terceroNombre}";
            ChkSoloTercero.IsChecked = true;
        }
        else
        {
            ChkSoloTercero.Visibility = Visibility.Collapsed;
        }

        Filtrar();
        Loaded += (_, _) => TxtBuscar.Focus();
    }

    public Contrato? Seleccionado => (LstContratos.SelectedItem as ContratoItem)?.Contrato;

    private void Filtrar()
    {
        var texto = TxtBuscar.Text.Trim();
        IEnumerable<ContratoItem> lista = _todos;
        if (ChkSoloTercero.IsChecked == true && _terceroId is not null)
            lista = lista.Where(c => c.Contrato.TerceroId == _terceroId);
        if (texto.Length > 0)
            lista = lista.Where(c => c.Busqueda.Contains(texto, StringComparison.CurrentCultureIgnoreCase));

        var items = lista.ToList();
        LstContratos.ItemsSource = items;
        if (items.Count == 1) LstContratos.SelectedIndex = 0;
        TxtVacio.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) => Filtrar();
    private void Filtro_Changed(object sender, RoutedEventArgs e) { if (IsLoaded) Filtrar(); }

    private void LstContratos_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        BtnGuardar.IsEnabled = Seleccionado is not null;

    private void LstContratos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (Seleccionado is not null) BtnGuardar_Click(sender, e);
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private sealed class ContratoItem
    {
        public ContratoItem(Contrato c)
        {
            Contrato = c;
            Numero = c.Numero;
            Objeto = c.Objeto;
            var partes = new List<string>();
            if (c.Tercero?.Nombre is { Length: > 0 } tercero) partes.Add(tercero);
            partes.Add(c.TipoDocumento.ToString());
            partes.Add(c.Estado.ToString());
            Detalle = "· " + string.Join(" · ", partes);
            Busqueda = $"{c.Numero} {c.Objeto} {c.Tercero?.Nombre}";
        }

        public Contrato Contrato { get; }
        public string Numero { get; }
        public string Objeto { get; }
        public string Detalle { get; }
        public string Busqueda { get; }
    }
}
