using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.DatosLegales;
using ContractSystem.Domain.DatosLegales;

namespace ContractSystem.Windows.ViewModels.Shared;

/// <summary>
/// Modelo editable de los datos legales (REEUP, registro, constitución, representantes y cuentas)
/// que comparten la ficha de la entidad propia y la de los terceros. Lo consume <c>DatosLegalesEditor</c>.
/// </summary>
public partial class DatosLegalesEditModel : ObservableObject
{
    public static IReadOnlyList<string> MonedasSugeridas { get; } = new[] { "CUP", "MLC", "USD", "EUR" };

    [ObservableProperty]
    private string _codigoReeup = string.Empty;

    [ObservableProperty]
    private string _registroMercantil = string.Empty;

    [ObservableProperty]
    private string _documentoConstitucion = string.Empty;

    [ObservableProperty]
    private RepresentanteEditItem? _representanteSeleccionado;

    [ObservableProperty]
    private CuentaEditItem? _cuentaSeleccionada;

    public ObservableCollection<RepresentanteEditItem> Representantes { get; } = new();
    public ObservableCollection<CuentaEditItem> Cuentas { get; } = new();

    public DatosLegalesEditModel()
    {
        Representantes.CollectionChanged += (_, e) => Suscribir(e, OnRepresentanteChanged);
        Cuentas.CollectionChanged += (_, e) => Suscribir(e, OnCuentaChanged);
    }

    private static void Suscribir(NotifyCollectionChangedEventArgs e, PropertyChangedEventHandler handler)
    {
        foreach (var item in e.NewItems?.OfType<INotifyPropertyChanged>() ?? Enumerable.Empty<INotifyPropertyChanged>())
            item.PropertyChanged += handler;
        foreach (var item in e.OldItems?.OfType<INotifyPropertyChanged>() ?? Enumerable.Empty<INotifyPropertyChanged>())
            item.PropertyChanged -= handler;
    }

    /// <summary>Solo un representante predeterminado: marcar uno desmarca los demás.</summary>
    private void OnRepresentanteChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(RepresentanteEditItem.EsPredeterminado) || sender is not RepresentanteEditItem { EsPredeterminado: true } marcado)
            return;
        foreach (var r in Representantes.Where(r => !ReferenceEquals(r, marcado)))
            r.EsPredeterminado = false;
    }

    /// <summary>Una cuenta predeterminada por moneda: marcar una desmarca las demás de su moneda.</summary>
    private void OnCuentaChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not CuentaEditItem { EsPredeterminada: true } marcada)
            return;
        if (e.PropertyName is not (nameof(CuentaEditItem.EsPredeterminada) or nameof(CuentaEditItem.Moneda)))
            return;
        foreach (var c in Cuentas.Where(c => !ReferenceEquals(c, marcada) && string.Equals(c.Moneda, marcada.Moneda, StringComparison.OrdinalIgnoreCase)))
            c.EsPredeterminada = false;
    }

    public void Cargar(
        string? codigoReeup,
        string? registroMercantil,
        string? documentoConstitucion,
        IEnumerable<RepresentanteLegal> representantes,
        IEnumerable<CuentaBancaria> cuentas)
    {
        CodigoReeup = codigoReeup ?? string.Empty;
        RegistroMercantil = registroMercantil ?? string.Empty;
        DocumentoConstitucion = documentoConstitucion ?? string.Empty;

        Representantes.Clear();
        foreach (var r in representantes.OrderByDescending(r => r.EsPredeterminado).ThenBy(r => r.Nombre))
        {
            Representantes.Add(new RepresentanteEditItem
            {
                Nombre = r.Nombre,
                Cargo = r.Cargo,
                CarneIdentidad = r.CarneIdentidad ?? string.Empty,
                DocumentoNombramiento = r.DocumentoNombramiento ?? string.Empty,
                FechaNombramiento = r.FechaNombramiento,
                EsPredeterminado = r.EsPredeterminado
            });
        }

        Cuentas.Clear();
        foreach (var c in cuentas.OrderBy(c => c.Moneda).ThenByDescending(c => c.EsPredeterminada))
        {
            Cuentas.Add(new CuentaEditItem
            {
                Numero = c.Numero,
                Moneda = c.Moneda,
                Banco = c.Banco,
                Sucursal = c.Sucursal ?? string.Empty,
                DireccionSucursal = c.DireccionSucursal ?? string.Empty,
                Titular = c.Titular ?? string.Empty,
                EsPredeterminada = c.EsPredeterminada
            });
        }
    }

    public DatosLegalesDto ToDto() => new(
        CodigoReeup,
        RegistroMercantil,
        DocumentoConstitucion,
        Representantes.Select(r => new RepresentanteLegalDto(
            r.Nombre, r.Cargo, r.CarneIdentidad, r.DocumentoNombramiento, r.FechaNombramiento, r.EsPredeterminado)).ToList(),
        Cuentas.Select(c => new CuentaBancariaDto(
            c.Numero, c.Moneda, c.Banco, c.Sucursal, c.DireccionSucursal, c.Titular, c.EsPredeterminada)).ToList());

    /// <summary>
    /// Mensaje con el primer dato incompleto, o null si todo es válido.
    /// Las filas totalmente vacías se ignoran (se descartan al guardar).
    /// </summary>
    public string? Validar()
    {
        foreach (var r in Representantes)
        {
            if (r.EstaVacio) continue;
            if (string.IsNullOrWhiteSpace(r.Nombre))
                return "Hay un representante legal sin nombre.";
            if (string.IsNullOrWhiteSpace(r.Cargo))
                return $"Indique el cargo del representante '{r.Nombre.Trim()}'.";
        }

        foreach (var c in Cuentas)
        {
            if (c.EstaVacia) continue;
            if (string.IsNullOrWhiteSpace(c.Numero))
                return "Hay una cuenta bancaria sin número.";
            if (string.IsNullOrWhiteSpace(c.Banco))
                return $"Indique el banco de la cuenta '{c.Numero.Trim()}'.";
        }

        return null;
    }

    [RelayCommand]
    private void AgregarRepresentante()
    {
        var item = new RepresentanteEditItem { EsPredeterminado = Representantes.Count == 0 };
        Representantes.Add(item);
        RepresentanteSeleccionado = item;
    }

    [RelayCommand]
    private void EliminarRepresentante()
    {
        if (RepresentanteSeleccionado is { } item)
            Representantes.Remove(item);
    }

    [RelayCommand]
    private void AgregarCuenta()
    {
        var item = new CuentaEditItem { Moneda = "CUP" };
        item.EsPredeterminada = Cuentas.All(c => c.Moneda != item.Moneda);
        Cuentas.Add(item);
        CuentaSeleccionada = item;
    }

    [RelayCommand]
    private void EliminarCuenta()
    {
        if (CuentaSeleccionada is { } item)
            Cuentas.Remove(item);
    }
}

public partial class RepresentanteEditItem : ObservableObject
{
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _cargo = string.Empty;
    [ObservableProperty] private string _carneIdentidad = string.Empty;
    [ObservableProperty] private string _documentoNombramiento = string.Empty;
    [ObservableProperty] private DateTime? _fechaNombramiento;
    [ObservableProperty] private bool _esPredeterminado;

    public bool EstaVacio =>
        string.IsNullOrWhiteSpace(Nombre) && string.IsNullOrWhiteSpace(Cargo) &&
        string.IsNullOrWhiteSpace(CarneIdentidad) && string.IsNullOrWhiteSpace(DocumentoNombramiento) &&
        FechaNombramiento is null;
}

public partial class CuentaEditItem : ObservableObject
{
    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private string _moneda = "CUP";
    [ObservableProperty] private string _banco = string.Empty;
    [ObservableProperty] private string _sucursal = string.Empty;
    [ObservableProperty] private string _direccionSucursal = string.Empty;
    [ObservableProperty] private string _titular = string.Empty;
    [ObservableProperty] private bool _esPredeterminada;

    public bool EstaVacia =>
        string.IsNullOrWhiteSpace(Numero) && string.IsNullOrWhiteSpace(Banco) &&
        string.IsNullOrWhiteSpace(Sucursal) && string.IsNullOrWhiteSpace(DireccionSucursal) &&
        string.IsNullOrWhiteSpace(Titular);
}
