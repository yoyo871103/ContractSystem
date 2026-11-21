using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContractSystem.Application.LlenadoContratos;
using ContractSystem.Windows.Services;

namespace ContractSystem.Windows.ViewModels.LlenadoContratos;

/// <summary>Parte a la que se asigna un hueco en la revisión.</summary>
public sealed record OpcionParte(string Nombre, bool EsMia)
{
    public override string ToString() => Nombre;
}

/// <summary>
/// Revisión previa al autollenado: el usuario ve cada hueco encontrado, de qué parte es y qué se escribirá,
/// y puede corregir la parte, el valor o desmarcarlo. Nada se escribe en Word sin pasar por aquí.
/// </summary>
public sealed partial class RevisionAutollenadoViewModel : ObservableObject
{
    private readonly PanelWordViewModel _panel;

    public RevisionAutollenadoViewModel(PanelWordViewModel panel, LecturaDocumento lectura, ResultadoAnalisis analisis)
    {
        _panel = panel;
        Lectura = lectura;
        OpcionMia = new OpcionParte("Mis datos", true);
        OpcionOtra = new OpcionParte("Otra parte", false);
        Opciones = new[] { OpcionMia, OpcionOtra };

        foreach (var hueco in analisis.Huecos)
        {
            OpcionParte? parte = hueco.Lado switch
            {
                LadoParte.Proveedor => panel.SoyProveedor ? OpcionMia : OpcionOtra,
                LadoParte.Cliente => panel.SoyProveedor ? OpcionOtra : OpcionMia,
                _ => null
            };
            var fila = new FilaAutollenado(this, hueco, lectura.Parrafos[hueco.ParrafoIndice], parte);
            fila.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(FilaAutollenado.Aplicar)) ActualizarResumen();
            };
            Filas.Add(fila);
        }

        ActualizarResumen();
    }

    public LecturaDocumento Lectura { get; }
    public OpcionParte OpcionMia { get; }
    public OpcionParte OpcionOtra { get; }
    public IReadOnlyList<OpcionParte> Opciones { get; }
    public ObservableCollection<FilaAutollenado> Filas { get; } = new();

    public string Titulo => $"Autollenar «{Lectura.Nombre}»";

    [ObservableProperty]
    private string _resumen = string.Empty;

    [ObservableProperty]
    private string _textoAceptar = "Llenar";

    [ObservableProperty]
    private bool _hayMarcados;

    /// <summary>Valor a escribir para un hueco según la parte elegida (vacío si esa parte no tiene el dato).</summary>
    internal string ValorPara(HuecoDetectado hueco, OpcionParte? parte)
    {
        if (parte is null) return string.Empty;
        var ctx = _panel.ContextoDe(parte.EsMia);
        return ctx is null ? string.Empty : AnalizadorProforma.Valor(hueco, ctx);
    }

    /// <summary>Si se detectó al revés quién es quién: cambia "Mis datos" por "Otra parte" en todas las filas.</summary>
    [RelayCommand]
    private void IntercambiarPartes()
    {
        foreach (var fila in Filas)
        {
            if (fila.Parte is not null)
                fila.Parte = fila.Parte.EsMia ? OpcionOtra : OpcionMia;
        }
    }

    private void ActualizarResumen()
    {
        var total = Filas.Count;
        var marcados = Filas.Count(f => f.Aplicar);
        var sinParte = Filas.Count(f => f.Parte is null);
        var sinDato = Filas.Count(f => f.Parte is not null && !f.TieneValor);

        var partes = new List<string> { $"Encontré {total} {(total == 1 ? "dato" : "datos")} para llenar" };
        if (sinDato > 0) partes.Add($"{sinDato} sin valor guardado");
        if (sinParte > 0) partes.Add($"{sinParte} sin parte clara (elíjala)");
        Resumen = string.Join(" · ", partes) + ".";

        HayMarcados = marcados > 0;
        TextoAceptar = marcados == 1 ? "Llenar 1 dato" : $"Llenar {marcados} datos";
    }

    /// <summary>Cambios a aplicar en Word con lo marcado.</summary>
    public IReadOnlyList<ReemplazoWord> Reemplazos() =>
        Filas.Where(f => f.Aplicar && f.TieneValor)
             .Select(f => new ReemplazoWord(
                 f.Parrafo.Inicio, f.Parrafo.FinReal, f.Parrafo.Texto,
                 f.Hueco.Inicio, f.Hueco.Longitud, f.Hueco.TextoHueco, f.Hueco.Prefijo, f.Valor.Trim()))
             .ToList();

    internal void NotificarCambio() => ActualizarResumen();
}

public sealed partial class FilaAutollenado : ObservableObject
{
    private readonly RevisionAutollenadoViewModel _revision;

    public FilaAutollenado(RevisionAutollenadoViewModel revision, HuecoDetectado hueco, ParrafoDocumento parrafo, OpcionParte? parte)
    {
        _revision = revision;
        Hueco = hueco;
        Parrafo = parrafo;
        _parte = parte;
        _valor = revision.ValorPara(hueco, parte);
        _aplicar = parte is not null && !string.IsNullOrWhiteSpace(_valor);
    }

    public HuecoDetectado Hueco { get; }
    public ParrafoDocumento Parrafo { get; }

    public string Dato => NombreCampo(Hueco.Campo) + (Hueco.Moneda is null ? string.Empty : $" ({Hueco.Moneda})");
    public string Contexto => Hueco.Contexto;

    [ObservableProperty]
    private bool _aplicar;

    [ObservableProperty]
    private OpcionParte? _parte;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneValor))]
    private string _valor;

    public bool TieneValor => !string.IsNullOrWhiteSpace(Valor);

    partial void OnParteChanged(OpcionParte? value)
    {
        Valor = _revision.ValorPara(Hueco, value);
        Aplicar = value is not null && TieneValor;
        _revision.NotificarCambio();
    }

    partial void OnValorChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value) && Aplicar) Aplicar = false;
        _revision.NotificarCambio();
    }

    private static string NombreCampo(CampoContrato campo) => campo switch
    {
        CampoContrato.Nombre => "Nombre de la entidad",
        CampoContrato.Reeup => "Código REEUP",
        CampoContrato.Nit => "NIT",
        CampoContrato.Direccion => "Domicilio legal",
        CampoContrato.Telefono => "Teléfono",
        CampoContrato.Email => "Correo",
        CampoContrato.RegistroMercantil => "Registro Mercantil",
        CampoContrato.DocumentoConstitucion => "Documento de constitución",
        CampoContrato.Representante => "Representante",
        CampoContrato.Cargo => "Cargo",
        CampoContrato.CarneIdentidad => "Carné de identidad",
        CampoContrato.DocumentoNombramiento => "Documento de nombramiento",
        CampoContrato.FechaNombramiento => "Fecha de nombramiento",
        CampoContrato.Cuenta => "Cuenta bancaria",
        CampoContrato.Banco => "Banco",
        CampoContrato.Sucursal => "Sucursal",
        CampoContrato.DireccionSucursal => "Dirección de la sucursal",
        CampoContrato.Titular => "Titular de la cuenta",
        _ => campo.ToString()
    };
}
