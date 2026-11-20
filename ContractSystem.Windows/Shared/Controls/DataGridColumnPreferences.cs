using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ContractSystem.Application.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace ContractSystem.Windows.Shared.Controls;

/// <summary>
/// Propiedad adjunta que guarda y restaura el orden y el ancho de las columnas de un DataGrid
/// como preferencia del usuario (archivo JSON en %LocalAppData%\ContractSystem\Preferencias).
/// Uso: &lt;DataGrid controls:DataGridColumnPreferences.Key="Contratos" .../&gt;
/// Las columnas se identifican por su Header (texto), así que añadir/quitar columnas no rompe nada.
/// </summary>
public static class DataGridColumnPreferences
{
    public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
        "Key", typeof(string), typeof(DataGridColumnPreferences),
        new PropertyMetadata(null, OnKeyChanged));

    public static string? GetKey(DependencyObject d) => (string?)d.GetValue(KeyProperty);
    public static void SetKey(DependencyObject d, string? value) => d.SetValue(KeyProperty, value);

    private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
        "State", typeof(GridState), typeof(DataGridColumnPreferences));

    private static readonly DependencyPropertyDescriptor WidthDescriptor =
        DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn));

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly object FileLock = new();

    private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid || e.NewValue is not string key || string.IsNullOrWhiteSpace(key)) return;
        if (grid.GetValue(StateProperty) is GridState) return;

        var state = new GridState(grid, key);
        grid.SetValue(StateProperty, state);
        grid.Loaded += (_, _) => state.Attach();
        grid.Unloaded += (_, _) => state.Detach(save: true);
        if (grid.IsLoaded) state.Attach();
    }

    /// <summary>
    /// Borra la preferencia guardada y restaura el orden/ancho definidos en XAML.
    /// </summary>
    public static void Reset(DataGrid grid)
    {
        if (grid.GetValue(StateProperty) is GridState state)
            state.Reset();
    }

    // ---------- Persistencia ----------

    private sealed record ColumnPref(string Header, int DisplayIndex, double Width, DataGridLengthUnitType Unit);

    private static string FilePath()
    {
        string usuario = "default";
        try
        {
            var auth = App.Services.GetService<IAuthContext>();
            if (!string.IsNullOrWhiteSpace(auth?.NombreUsuario)) usuario = auth!.NombreUsuario!;
        }
        catch { /* sin contenedor (diseñador) */ }

        foreach (var c in Path.GetInvalidFileNameChars()) usuario = usuario.Replace(c, '_');
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ContractSystem", "Preferencias");
        return Path.Combine(dir, $"columnas_{usuario}.json");
    }

    private static Dictionary<string, List<ColumnPref>> LoadAll()
    {
        try
        {
            var path = FilePath();
            lock (FileLock)
            {
                if (!File.Exists(path)) return new();
                return JsonSerializer.Deserialize<Dictionary<string, List<ColumnPref>>>(File.ReadAllText(path)) ?? new();
            }
        }
        catch { return new(); }
    }

    private static void SaveGrid(string key, List<ColumnPref>? prefs)
    {
        try
        {
            var all = LoadAll();
            if (prefs is null) all.Remove(key); else all[key] = prefs;
            var path = FilePath();
            lock (FileLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, JsonSerializer.Serialize(all, JsonOptions));
            }
        }
        catch { /* una preferencia no guardada no debe romper la UI */ }
    }

    private static string HeaderDe(DataGridColumn c) => c.Header?.ToString() ?? string.Empty;

    // ---------- Estado por grid ----------

    private sealed class GridState
    {
        private readonly DataGrid _grid;
        private readonly string _key;
        private readonly DispatcherTimer _debounce;
        private List<ColumnPref>? _defaults;
        private bool _attached;
        private bool _applying;

        public GridState(DataGrid grid, string key)
        {
            _grid = grid;
            _key = key;
            _debounce = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _debounce.Tick += (_, _) => { _debounce.Stop(); Save(); };
        }

        public void Attach()
        {
            if (_attached) return;
            _attached = true;

            _defaults ??= Capture();
            Apply();

            _grid.ColumnReordered += OnChanged;
            foreach (var col in _grid.Columns)
                WidthDescriptor.AddValueChanged(col, OnChanged);
        }

        public void Detach(bool save)
        {
            if (!_attached) return;
            _attached = false;
            _grid.ColumnReordered -= OnChanged;
            foreach (var col in _grid.Columns)
                WidthDescriptor.RemoveValueChanged(col, OnChanged);

            if (save && _debounce.IsEnabled)
            {
                _debounce.Stop();
                Save();
            }
        }

        public void Reset()
        {
            _debounce.Stop();
            SaveGrid(_key, null);
            if (_defaults is not null) ApplyPrefs(_defaults);
        }

        private void OnChanged(object? sender, EventArgs e)
        {
            if (_applying) return;
            _debounce.Stop();
            _debounce.Start();
        }

        private List<ColumnPref> Capture() =>
            _grid.Columns
                .Select(c => new ColumnPref(HeaderDe(c), c.DisplayIndex, c.Width.Value, c.Width.UnitType))
                .ToList();

        private void Save() => SaveGrid(_key, Capture());

        private void Apply()
        {
            if (LoadAll().TryGetValue(_key, out var prefs) && prefs.Count > 0)
                ApplyPrefs(prefs);
        }

        private void ApplyPrefs(List<ColumnPref> prefs)
        {
            _applying = true;
            try
            {
                var porHeader = _grid.Columns
                    .GroupBy(HeaderDe)
                    .ToDictionary(g => g.Key, g => g.First());

                // Anchos
                foreach (var p in prefs)
                {
                    if (!porHeader.TryGetValue(p.Header, out var col)) continue;
                    if (p.Unit is DataGridLengthUnitType.Pixel or DataGridLengthUnitType.Star && p.Width > 0)
                        col.Width = new DataGridLength(p.Width, p.Unit);
                    else if (p.Unit is DataGridLengthUnitType.Auto or DataGridLengthUnitType.SizeToCells or DataGridLengthUnitType.SizeToHeader)
                        col.Width = new DataGridLength(1, p.Unit);
                }

                // Orden: se asigna DisplayIndex de menor a mayor para que no se pisen.
                var max = _grid.Columns.Count - 1;
                foreach (var p in prefs.OrderBy(p => p.DisplayIndex))
                {
                    if (!porHeader.TryGetValue(p.Header, out var col)) continue;
                    col.DisplayIndex = Math.Clamp(p.DisplayIndex, 0, max);
                }
            }
            catch
            {
                // Preferencia corrupta o incompatible: se ignora.
            }
            finally
            {
                _applying = false;
            }
        }
    }
}
