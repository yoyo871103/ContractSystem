using System.Collections;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ContractSystem.Windows.Shared.Controls;

/// <summary>
/// Selector con búsqueda incremental (autocompletado) y botón opcional para dar de alta
/// un elemento nuevo sin salir de la ventana.
/// Sustituye a un ComboBox: expone <see cref="ItemsSource"/>, <see cref="SelectedItem"/>,
/// <see cref="DisplayMemberPath"/> y el evento <see cref="SelectionChanged"/>.
/// </summary>
public partial class AutoCompleteSelector : UserControl
{
    private const int MaxSugerencias = 50;
    private bool _actualizandoTexto;

    public AutoCompleteSelector()
    {
        InitializeComponent();
        IsEnabledChanged += (_, _) => ActualizarEstadoVisual();
        Loaded += (_, _) => ActualizarEstadoVisual();
    }

    // ---------- Propiedades ----------

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(AutoCompleteSelector),
        new PropertyMetadata(null, (d, _) => ((AutoCompleteSelector)d).OnItemsSourceChanged()));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(AutoCompleteSelector),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (d, e) => ((AutoCompleteSelector)d).OnSelectedItemChanged(e.OldValue, e.NewValue)));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Propiedad del item que se muestra en la caja y en la lista.</summary>
    public string? DisplayMemberPath { get; set; }

    /// <summary>
    /// Propiedades adicionales (separadas por coma) sobre las que también se busca (ej: "Codigo,NifCif").
    /// </summary>
    public string? SearchMemberPaths { get; set; }

    /// <summary>Texto de ayuda cuando no hay selección.</summary>
    public string Placeholder
    {
        get => TxtPlaceholder.Text;
        set => TxtPlaceholder.Text = value;
    }

    /// <summary>Texto del botón de alta (ej: "Nuevo tercero").</summary>
    public string AddButtonText
    {
        get => TxtAgregar.Text;
        set => TxtAgregar.Text = value;
    }

    /// <summary>Muestra el botón para agregar un elemento nuevo.</summary>
    public bool ShowAddButton
    {
        get => BtnAgregar.Visibility == Visibility.Visible;
        set => BtnAgregar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    public string AddButtonToolTip
    {
        set => BtnAgregar.ToolTip = value;
    }

    /// <summary>Se dispara al pulsar el botón de alta. El texto buscado viene en <see cref="SearchText"/>.</summary>
    public event EventHandler? AddNewRequested;

    public event SelectionChangedEventHandler? SelectionChanged;

    /// <summary>Texto escrito actualmente en la caja de búsqueda.</summary>
    public string SearchText => TxtBuscar.Text ?? string.Empty;

    // ---------- Lógica ----------

    private void OnItemsSourceChanged()
    {
        // Si el item seleccionado ya no está en la nueva lista, se conserva igualmente
        // (puede ser una instancia equivalente); solo refrescamos el texto.
        MostrarTextoSeleccion();
    }

    private void OnSelectedItemChanged(object? oldValue, object? newValue)
    {
        MostrarTextoSeleccion();
        BtnLimpiar.Visibility = newValue is not null && IsEnabled ? Visibility.Visible : Visibility.Collapsed;

        var removed = oldValue is null ? Array.Empty<object>() : new[] { oldValue };
        var added = newValue is null ? Array.Empty<object>() : new[] { newValue };
        SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(System.Windows.Controls.Primitives.Selector.SelectionChangedEvent, removed, added));
    }

    private void ActualizarEstadoVisual()
    {
        TxtBuscar.IsReadOnly = !IsEnabled;
        BrdTexto.Background = IsEnabled ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF4, 0xF4, 0xF4));
        BtnLimpiar.Visibility = SelectedItem is not null && IsEnabled ? Visibility.Visible : Visibility.Collapsed;
        ActualizarPlaceholder();
    }

    private void MostrarTextoSeleccion()
    {
        _actualizandoTexto = true;
        TxtBuscar.Text = SelectedItem is null ? string.Empty : TextoDe(SelectedItem);
        TxtBuscar.CaretIndex = TxtBuscar.Text.Length;
        _actualizandoTexto = false;
        ActualizarPlaceholder();
    }

    private void ActualizarPlaceholder()
    {
        TxtPlaceholder.Visibility = string.IsNullOrEmpty(TxtBuscar.Text) ? Visibility.Visible : Visibility.Collapsed;
    }

    private string TextoDe(object item) => LeerPropiedad(item, DisplayMemberPath) ?? item.ToString() ?? string.Empty;

    private static string? LeerPropiedad(object item, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        object? actual = item;
        foreach (var parte in path.Split('.'))
        {
            if (actual is null) return null;
            actual = actual.GetType().GetProperty(parte)?.GetValue(actual);
        }
        return actual?.ToString();
    }

    private bool Coincide(object item, string[] terminos)
    {
        var sb = new StringBuilder(TextoDe(item));
        if (!string.IsNullOrWhiteSpace(SearchMemberPaths))
        {
            foreach (var path in SearchMemberPaths.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                sb.Append(' ').Append(LeerPropiedad(item, path));
        }
        var texto = Normalizar(sb.ToString());
        return terminos.All(t => texto.Contains(t, StringComparison.Ordinal));
    }

    /// <summary>Minúsculas y sin tildes, para buscar "credito" y encontrar "Crédito".</summary>
    private static string Normalizar(string s)
    {
        var descompuesto = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString();
    }

    private void Filtrar(bool mostrarTodo)
    {
        if (ItemsSource is null) return;

        var terminos = mostrarTodo
            ? Array.Empty<string>()
            : Normalizar(TxtBuscar.Text ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var resultados = ItemsSource.Cast<object>()
            .Where(i => terminos.Length == 0 || Coincide(i, terminos))
            .Take(MaxSugerencias)
            .Select(i => new ListBoxItem { Content = TextoDe(i), Tag = i })
            .ToList();

        LstSugerencias.ItemsSource = resultados;
        LstSugerencias.Visibility = resultados.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        TxtSinResultados.Visibility = resultados.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        TxtSinResultados.Text = ShowAddButton
            ? $"Sin resultados. Use \"{AddButtonText}\" para darlo de alta."
            : "Sin resultados.";

        if (resultados.Count > 0)
        {
            var actual = resultados.FirstOrDefault(r => ReferenceEquals(r.Tag, SelectedItem));
            LstSugerencias.SelectedItem = actual ?? resultados[0];
            LstSugerencias.ScrollIntoView(LstSugerencias.SelectedItem);
        }

        PopSugerencias.IsOpen = true;
    }

    private void Seleccionar(object? item)
    {
        SelectedItem = item;
        PopSugerencias.IsOpen = false;
        MostrarTextoSeleccion();
    }

    // ---------- Eventos UI ----------

    private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
    {
        ActualizarPlaceholder();
        if (_actualizandoTexto || !IsEnabled || !TxtBuscar.IsKeyboardFocusWithin) return;
        Filtrar(mostrarTodo: false);
    }

    private void TxtBuscar_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (!IsEnabled) return;
        TxtBuscar.SelectAll();
    }

    /// <summary>
    /// Clic en la caja: se comporta como un desplegable y muestra toda la lista.
    /// El primer clic también selecciona el texto para poder sobrescribirlo al teclear.
    /// </summary>
    private void TxtBuscar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsEnabled) return;
        if (!TxtBuscar.IsKeyboardFocusWithin)
        {
            TxtBuscar.Focus();
            e.Handled = true; // evita que el clic deshaga el SelectAll
        }
        if (!PopSugerencias.IsOpen)
            Filtrar(mostrarTodo: true);
    }

    private void TxtBuscar_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        // Si el foco pasa a la lista de sugerencias no hacemos nada (se resolverá en el click).
        if (e.NewFocus is DependencyObject d && PopSugerencias.Child is not null && EsDescendiente(d, PopSugerencias.Child))
            return;

        PopSugerencias.IsOpen = false;

        // Texto vacío => sin selección. Texto distinto del seleccionado => se restaura.
        if (string.IsNullOrWhiteSpace(TxtBuscar.Text))
        {
            if (SelectedItem is not null) SelectedItem = null;
        }
        else
        {
            MostrarTextoSeleccion();
        }
    }

    private void TxtBuscar_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsEnabled) return;

        switch (e.Key)
        {
            case Key.Down:
                if (!PopSugerencias.IsOpen) { Filtrar(mostrarTodo: SelectedItem is not null); }
                else MoverSeleccion(+1);
                e.Handled = true;
                break;
            case Key.Up:
                if (PopSugerencias.IsOpen) { MoverSeleccion(-1); e.Handled = true; }
                break;
            case Key.Enter:
                if (PopSugerencias.IsOpen && LstSugerencias.SelectedItem is ListBoxItem li)
                {
                    Seleccionar(li.Tag);
                    e.Handled = true;
                }
                break;
            case Key.Escape:
                if (PopSugerencias.IsOpen)
                {
                    PopSugerencias.IsOpen = false;
                    MostrarTextoSeleccion();
                    e.Handled = true;
                }
                break;
        }
    }

    private void MoverSeleccion(int delta)
    {
        var count = LstSugerencias.Items.Count;
        if (count == 0) return;
        var idx = Math.Clamp(LstSugerencias.SelectedIndex + delta, 0, count - 1);
        LstSugerencias.SelectedIndex = idx;
        LstSugerencias.ScrollIntoView(LstSugerencias.SelectedItem);
    }

    private void LstSugerencias_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject d)
        {
            var li = ItemsControl.ContainerFromElement(LstSugerencias, d) as ListBoxItem;
            if (li is not null)
            {
                Seleccionar(li.Tag);
                TxtBuscar.Focus();
            }
        }
    }

    private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
    {
        SelectedItem = null;
        TxtBuscar.Focus();
        Filtrar(mostrarTodo: true);
    }

    private void BtnAgregar_Click(object sender, RoutedEventArgs e)
    {
        PopSugerencias.IsOpen = false;
        AddNewRequested?.Invoke(this, EventArgs.Empty);
    }

    private static bool EsDescendiente(DependencyObject hijo, DependencyObject padre)
    {
        var actual = hijo;
        while (actual is not null)
        {
            if (ReferenceEquals(actual, padre)) return true;
            actual = actual is System.Windows.Media.Visual or System.Windows.Media.Media3D.Visual3D
                ? System.Windows.Media.VisualTreeHelper.GetParent(actual)
                : LogicalTreeHelper.GetParent(actual);
        }
        return false;
    }
}
