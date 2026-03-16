using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ContractSystem.Application.Contratos;
using ContractSystem.Domain.Contratos;
using ContractSystem.Domain.Nomencladores;
using MediatR;

namespace ContractSystem.Windows.Views.Contratos;

public partial class ContratoDialogWindow : Window
{
    private IDocumentoNumeracionService? _numeracionService;
    private bool _esEdicion;
    private int? _contratoId;
    private string? _contratoNumero;

    public ContratoDialogWindow()
    {
        InitializeComponent();
    }

    // --- Propiedades públicas para leer los valores del formulario ---

    public TipoDocumentoContrato TipoDocumento
    {
        get
        {
            var item = CmbTipo.SelectedItem as ComboBoxItem;
            return item?.Tag switch
            {
                "0" => TipoDocumentoContrato.Marco,
                "1" => TipoDocumentoContrato.Especifico,
                "2" => TipoDocumentoContrato.Independiente,
                _ => TipoDocumentoContrato.Independiente
            };
        }
    }

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

    public DateTime? FechaFirma => DpFechaFirma.SelectedDate;
    public DateTime? FechaEntradaVigor => DpFechaEntradaVigor.SelectedDate;
    public DateTime? FechaVigencia => DpFechaVigencia.SelectedDate;
    public string? Duracion => string.IsNullOrWhiteSpace(TxtDuracion.Text) ? null : TxtDuracion.Text.Trim();

    public int? MiEmpresaId => null; // Se asignará en futuras etapas si hay múltiples sedes

    public int? TerceroId => (CmbTercero.SelectedItem as Tercero)?.Id;

    public int? ContratoPadreId =>
        TipoDocumento == TipoDocumentoContrato.Especifico
            ? (CmbMarcoPadre.SelectedItem as Contrato)?.Id
            : null;

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

    // --- Métodos públicos ---

    public void SetNumeracionService(IDocumentoNumeracionService service)
    {
        _numeracionService = service;
    }

    public void CargarDatosAuxiliares(IReadOnlyList<Contrato> contratosMarco, IReadOnlyList<Tercero> terceros)
    {
        CmbMarcoPadre.ItemsSource = contratosMarco;
        CmbTercero.ItemsSource = terceros;
    }

    public void CargarContrato(Contrato contrato)
    {
        _esEdicion = true;
        _contratoId = contrato.Id;
        _contratoNumero = contrato.Numero;
        TxtTitulo.Text = "Editar contrato";

        // Mostrar botón Anexos/Líneas en edición
        BtnAnexosLineas.Visibility = Visibility.Visible;

        // Tipo (no editable en edición)
        CmbTipo.IsEnabled = false;
        foreach (ComboBoxItem item in CmbTipo.Items)
        {
            if (item.Tag?.ToString() == ((int)contrato.TipoDocumento).ToString())
            {
                CmbTipo.SelectedItem = item;
                break;
            }
        }

        // Rol
        foreach (ComboBoxItem item in CmbRol.Items)
        {
            if (item.Tag?.ToString() == ((int)contrato.Rol).ToString())
            {
                CmbRol.SelectedItem = item;
                break;
            }
        }

        TxtNumero.Text = contrato.Numero;
        TxtObjeto.Text = contrato.Objeto;

        DpFechaFirma.SelectedDate = contrato.FechaFirma;
        DpFechaEntradaVigor.SelectedDate = contrato.FechaEntradaVigor;
        DpFechaVigencia.SelectedDate = contrato.FechaVigencia;
        TxtDuracion.Text = contrato.Duracion ?? string.Empty;

        // Tercero
        if (contrato.TerceroId.HasValue && CmbTercero.ItemsSource is IEnumerable<Tercero> terceros)
        {
            CmbTercero.SelectedItem = terceros.FirstOrDefault(t => t.Id == contrato.TerceroId.Value);
        }

        // Marco padre
        if (contrato.ContratoPadreId.HasValue && CmbMarcoPadre.ItemsSource is IEnumerable<Contrato> marcos)
        {
            CmbMarcoPadre.SelectedItem = marcos.FirstOrDefault(m => m.Id == contrato.ContratoPadreId.Value);
        }

        TxtValorTotal.Text = contrato.ValorTotal?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        TxtCondicionesEntrega.Text = contrato.CondicionesEntrega ?? string.Empty;
        TxtCostosAsociados.Text = contrato.CostosAsociados ?? string.Empty;

        // Ocultar botón generar número en edición
        BtnGenerarNumero.Visibility = Visibility.Collapsed;

        ActualizarVisibilidadMarcoPadre();
    }

    // --- Event handlers ---

    private void CmbTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ActualizarVisibilidadMarcoPadre();
    }

    private void ActualizarVisibilidadMarcoPadre()
    {
        if (PnlMarcoPadre is null) return;
        var esEspecifico = TipoDocumento == TipoDocumentoContrato.Especifico;
        PnlMarcoPadre.Visibility = esEspecifico ? Visibility.Visible : Visibility.Collapsed;

        // Si no es Específico, restaurar Rol y Tercero como editables
        if (!esEspecifico && !_esEdicion)
        {
            CmbRol.IsEnabled = true;
            CmbTercero.IsEnabled = true;
        }
    }

    private async void BtnGenerarNumero_Click(object sender, RoutedEventArgs e)
    {
        if (_numeracionService is null) return;

        try
        {
            var tercero = CmbTercero.SelectedItem as Tercero;
            var numero = await _numeracionService.GenerarNumeroAsync(
                TipoDocumento,
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
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(TxtNumero.Text))
        {
            MessageBox.Show("El número del documento es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtNumero.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtObjeto.Text))
        {
            MessageBox.Show("El objeto del contrato es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtObjeto.Focus();
            return;
        }

        if (TipoDocumento == TipoDocumentoContrato.Especifico && CmbMarcoPadre.SelectedItem is null)
        {
            MessageBox.Show("Debe seleccionar un Contrato Marco padre para un contrato Específico.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            CmbMarcoPadre.Focus();
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
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        else
        {
            DragMove();
        }
    }

    // --- Atajos de vigencia (desde fecha firma) ---

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

    // --- Auto-asignar tercero del padre al seleccionar Marco padre ---

    private void CmbMarcoPadre_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_esEdicion) return; // No sobreescribir en edición
        if (CmbMarcoPadre.SelectedItem is not Contrato padre) return;

        // Heredar tercero del padre
        if (padre.TerceroId.HasValue && CmbTercero.ItemsSource is IEnumerable<Tercero> terceros)
        {
            var tercero = terceros.FirstOrDefault(t => t.Id == padre.TerceroId.Value);
            if (tercero is not null)
            {
                CmbTercero.SelectedItem = tercero;
                CmbTercero.IsEnabled = false;
            }
        }

        // Heredar rol del padre
        foreach (ComboBoxItem item in CmbRol.Items)
        {
            if (item.Tag?.ToString() == ((int)padre.Rol).ToString())
            {
                CmbRol.SelectedItem = item;
                break;
            }
        }
        CmbRol.IsEnabled = false;
    }

    private void BtnAnexosLineas_Click(object sender, RoutedEventArgs e)
    {
        if (!_contratoId.HasValue) return;

        var mediator = App.Services.GetService(typeof(ISender)) as ISender;
        if (mediator is null) return;

        var window = new AnexosLineasWindow(mediator, _contratoId.Value, _contratoNumero ?? "");
        window.Owner = this;
        window.ShowDialog();
    }

    private void OnlyDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var textBox = sender as TextBox;
        var newText = textBox?.Text.Insert(textBox.CaretIndex, e.Text) ?? e.Text;
        e.Handled = !decimal.TryParse(newText, NumberStyles.Any, CultureInfo.CurrentCulture, out _)
                    && e.Text != separator;
    }
}
