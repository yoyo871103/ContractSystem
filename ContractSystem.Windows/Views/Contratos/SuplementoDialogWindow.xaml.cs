using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos;
using ContractSystem.Domain.Contratos;
using ContractSystem.Domain.Nomencladores;

namespace ContractSystem.Windows.Views.Contratos;

public partial class SuplementoDialogWindow : Window
{
    private IDocumentoNumeracionService? _numeracionService;
    private readonly ObservableCollection<ModificacionEditItem> _modificaciones = new();
    private int _contratoPadreId;
    private TipoDocumentoContrato _tipoPadre;

    public SuplementoDialogWindow()
    {
        InitializeComponent();
        DgModificaciones.ItemsSource = _modificaciones;
    }

    // --- Propiedades públicas ---

    public int ContratoPadreId => _contratoPadreId;

    public RolContrato RolContrato
    {
        get
        {
            var item = CmbRol.SelectedItem as ComboBoxItem;
            return item?.Tag switch
            {
                "1" => Domain.Contratos.RolContrato.Cliente,
                _ => Domain.Contratos.RolContrato.Proveedor
            };
        }
    }

    public string NumeroDocumento => (TxtNumero.Text ?? "").Trim();
    public string ObjetoContrato => (TxtObjeto.Text ?? "").Trim();
    public bool EsModificacionGenerales => ChkModificacionGenerales.IsChecked == true;

    public DateTime? FechaFirma => DpFechaFirma.SelectedDate;
    public DateTime? FechaEntradaVigor => DpFechaEntradaVigor.SelectedDate;
    public DateTime? FechaVigencia => DpFechaVigencia.SelectedDate;
    public string? Duracion => string.IsNullOrWhiteSpace(TxtDuracion.Text) ? null : TxtDuracion.Text.Trim();

    public int? MiEmpresaId => null;
    public int? TerceroId => (CmbTercero.SelectedItem as Tercero)?.Id;

    public decimal? ValorTotal
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TxtValorTotal.Text)) return null;
            return decimal.TryParse(TxtValorTotal.Text.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out var val) ? val : null;
        }
    }

    public string? CondicionesEntrega =>
        string.IsNullOrWhiteSpace(TxtCondicionesEntrega.Text) ? null : TxtCondicionesEntrega.Text.Trim();
    public string? CostosAsociados =>
        string.IsNullOrWhiteSpace(TxtCostosAsociados.Text) ? null : TxtCostosAsociados.Text.Trim();

    public IReadOnlyList<ModificacionEditItem> Modificaciones =>
        _modificaciones.Where(m => m.DocumentoDestinoId > 0).ToList();

    // --- Métodos públicos ---

    public void SetNumeracionService(IDocumentoNumeracionService service)
    {
        _numeracionService = service;
    }

    public void ConfigurarParaContrato(Contrato contratoPadre, IReadOnlyList<Contrato> documentosDisponibles, IReadOnlyList<Tercero> terceros)
    {
        _contratoPadreId = contratoPadre.Id;
        _tipoPadre = contratoPadre.TipoDocumento;

        TxtContratoInfo.Text = $"Padre: {contratoPadre.Numero} ({contratoPadre.TipoDocumento})";

        // Mostrar checkbox de modificación de generales solo si padre es Marco
        ChkModificacionGenerales.Visibility = _tipoPadre == TipoDocumentoContrato.Marco
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Si padre es Marco, preseleccionar modificación de generales como true
        if (_tipoPadre == TipoDocumentoContrato.Marco)
            ChkModificacionGenerales.IsChecked = true;

        // Configurar ComboBox de documentos destino (todos los contratos/suplementos excepto el que estamos creando)
        ColDocumentoDestino.ItemsSource = documentosDisponibles;

        // Terceros
        CmbTercero.ItemsSource = terceros;

        // El tercero se hereda del padre (no editable)
        if (contratoPadre.TerceroId.HasValue)
        {
            CmbTercero.SelectedItem = terceros.FirstOrDefault(t => t.Id == contratoPadre.TerceroId.Value);
            CmbTercero.IsEnabled = false;
        }

        // El rol se hereda del padre (no editable)
        foreach (ComboBoxItem item in CmbRol.Items)
        {
            if (item.Tag?.ToString() == ((int)contratoPadre.Rol).ToString())
            {
                CmbRol.SelectedItem = item;
                break;
            }
        }
        CmbRol.IsEnabled = false;
    }

    // --- Event handlers ---

    private async void BtnGenerarNumero_Click(object sender, RoutedEventArgs e)
    {
        if (_numeracionService is null) return;

        try
        {
            var tercero = CmbTercero.SelectedItem as Tercero;
            var numero = await _numeracionService.GenerarNumeroAsync(
                TipoDocumentoContrato.Suplemento,
                tercero?.NifCif);
            TxtNumero.Text = numero;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al generar número: " + ex.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNumero.Text))
        {
            MessageBox.Show("El número del documento es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNumero.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtObjeto.Text))
        {
            MessageBox.Show("El objeto del suplemento es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtObjeto.Focus();
            return;
        }

        // R09: Si padre es Marco y no es modificación de generales, advertir
        if (_tipoPadre == TipoDocumentoContrato.Marco && ChkModificacionGenerales.IsChecked != true)
        {
            MessageBox.Show(
                "Los suplementos de un Contrato Marco solo pueden ser para modificaciones de condiciones generales.",
                "Validación R09", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else
            DragMove();
    }

    // --- Atajos de vigencia ---

    private void AplicarVigencia(int anios)
    {
        var fechaBase = DpFechaFirma.SelectedDate ?? DateTime.Today;
        DpFechaVigencia.SelectedDate = fechaBase.AddYears(anios);
        TxtDuracion.Text = anios == 1 ? "1 año" : $"{anios} años";
    }

    private void BtnVigencia1_Click(object sender, RoutedEventArgs e) => AplicarVigencia(1);
    private void BtnVigencia2_Click(object sender, RoutedEventArgs e) => AplicarVigencia(2);
    private void BtnVigencia3_Click(object sender, RoutedEventArgs e) => AplicarVigencia(3);
    private void BtnVigencia4_Click(object sender, RoutedEventArgs e) => AplicarVigencia(4);
    private void BtnVigencia5_Click(object sender, RoutedEventArgs e) => AplicarVigencia(5);

    private void OnlyDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var textBox = sender as TextBox;
        var newText = textBox?.Text.Insert(textBox.CaretIndex, e.Text) ?? e.Text;
        e.Handled = !decimal.TryParse(newText, NumberStyles.Any, CultureInfo.CurrentCulture, out _)
                    && e.Text != separator;
    }
}

/// <summary>
/// Item editable para la grilla de relaciones "modifica a".
/// </summary>
public class ModificacionEditItem
{
    public int DocumentoDestinoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
