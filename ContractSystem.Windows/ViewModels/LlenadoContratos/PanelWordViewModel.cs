using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ContractSystem.Application.Auth;
using ContractSystem.Application.Business.Queries.GetBusinessInfo;
using ContractSystem.Application.Contratos.Commands.AdjuntarDocumento;
using ContractSystem.Application.Contratos.Commands.ReemplazarContenidoAdjunto;
using ContractSystem.Application.Contratos.Queries.GetAllContratos;
using ContractSystem.Application.Contratos.Queries.GetContratoById;
using ContractSystem.Application.Contratos.Queries.GetDocumentoAdjuntoById;
using ContractSystem.Application.Contratos.Queries.GetDocumentosAdjuntos;
using ContractSystem.Application.LlenadoContratos;
using ContractSystem.Application.Nomencladores.Queries.GetAllTerceros;
using ContractSystem.Application.Nomencladores.Queries.GetTerceroById;
using ContractSystem.Domain.Contratos;
using ContractSystem.Domain.DatosLegales;
using ContractSystem.Domain.Nomencladores;
using ContractSystem.Windows.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace ContractSystem.Windows.ViewModels.LlenadoContratos;

/// <summary>
/// Panel que acompaña a Word: cada clic inserta un dato (o un bloque completo) donde está el cursor.
/// Maneja dos partes: la entidad propia ("Mis datos") y la contraparte ("Otra parte").
/// </summary>
public sealed partial class PanelWordViewModel : ObservableObject
{
    private readonly ISender _sender;
    private Contrato? _contrato;
    private string _plantillaComparecencia = BloquesContrato.ComparecenciaPredeterminada;
    private string _plantillaFirma = BloquesContrato.FirmaPredeterminada;

    public PanelWordViewModel(ISender sender)
    {
        _sender = sender;
        Mia = new ParteViewModel("Mis datos", this);
        Otra = new ParteViewModel("Otra parte", this);
        _parDenominaciones = ParDenominaciones.Habituales[0];
        AplicarDenominaciones();
    }

    public ParteViewModel Mia { get; }
    public ParteViewModel Otra { get; }

    public IReadOnlyList<ParDenominaciones> ParesDenominaciones => ParDenominaciones.Habituales;
    public IReadOnlyList<string> Monedas { get; } = new[] { "CUP", "MLC", "USD", "EUR" };

    [ObservableProperty]
    private IReadOnlyList<Tercero> _terceros = Array.Empty<Tercero>();

    [ObservableProperty]
    private Tercero? _terceroSeleccionado;

    /// <summary>true: actúo como proveedor (presto, vendo, suministro…); false: como cliente.</summary>
    [ObservableProperty]
    private bool _soyProveedor = true;

    [ObservableProperty]
    private ParDenominaciones _parDenominaciones;

    [ObservableProperty]
    private string _denominacionMia = string.Empty;

    [ObservableProperty]
    private string _denominacionOtra = string.Empty;

    [ObservableProperty]
    private string _moneda = "CUP";

    /// <summary>Contrato desde el que se abrió el panel (texto informativo), o null.</summary>
    [ObservableProperty]
    private string? _contratoDescripcion;

    [ObservableProperty]
    private string _estado = "Coloque el cursor en Word y haga clic en un dato para insertarlo.";

    [ObservableProperty]
    private bool _estadoEsError;

    public bool SoyCliente
    {
        get => !SoyProveedor;
        set => SoyProveedor = !value;
    }

    partial void OnSoyProveedorChanged(bool value)
    {
        OnPropertyChanged(nameof(SoyCliente));
        AplicarDenominaciones();
    }

    partial void OnParDenominacionesChanged(ParDenominaciones value) => AplicarDenominaciones();

    partial void OnMonedaChanged(string value)
    {
        Mia.ElegirCuentaPara(value);
        Otra.ElegirCuentaPara(value);
    }

    partial void OnTerceroSeleccionadoChanged(Tercero? value) => _ = CargarOtraParteAsync(value?.Id);

    private void AplicarDenominaciones()
    {
        if (ParDenominaciones is null) return;
        DenominacionMia = SoyProveedor ? ParDenominaciones.Proveedor : ParDenominaciones.Cliente;
        DenominacionOtra = SoyProveedor ? ParDenominaciones.Cliente : ParDenominaciones.Proveedor;
    }

    /// <summary>Carga los datos propios, los terceros y, si se indica, el contexto de un contrato.</summary>
    public async Task InicializarAsync(Contrato? contrato)
    {
        try
        {
            await CargarDatosAsync();
            await AplicarContratoAsync(contrato);
        }
        catch (Exception ex)
        {
            MostrarError("No se pudieron cargar los datos: " + ex.Message);
        }
    }

    private async Task CargarDatosAsync()
    {
        var info = await _sender.Send(new GetBusinessInfoQuery());
        Mia.Ficha = info is null ? null : FichaParte.DesdeEntidad(info);
        _plantillaComparecencia = string.IsNullOrWhiteSpace(info?.PlantillaComparecencia)
            ? BloquesContrato.ComparecenciaPredeterminada : info.PlantillaComparecencia;
        _plantillaFirma = string.IsNullOrWhiteSpace(info?.PlantillaFirma)
            ? BloquesContrato.FirmaPredeterminada : info.PlantillaFirma;

        Terceros = await _sender.Send(new GetAllTercerosQuery());
    }

    /// <summary>Vuelve a leer los datos (por si se completaron en Configuración con el panel abierto).</summary>
    [RelayCommand]
    private async Task RecargarAsync()
    {
        try
        {
            var terceroId = TerceroSeleccionado?.Id;
            await CargarDatosAsync();
            var tercero = Terceros.FirstOrDefault(t => t.Id == terceroId);
            if (tercero is not null)
                TerceroSeleccionado = tercero; // objeto nuevo: dispara la recarga de la otra parte
            EstadoEsError = false;
            Estado = "Datos actualizados.";
        }
        catch (Exception ex)
        {
            MostrarError("No se pudieron recargar los datos: " + ex.Message);
        }
    }

    /// <summary>Toma del contrato el papel propio y la contraparte.</summary>
    public async Task AplicarContratoAsync(Contrato? contrato)
    {
        if (contrato is null) return;

        _contrato = contrato;
        ContratoDescripcion = $"Contrato {contrato.Numero}";
        SoyProveedor = contrato.Rol == RolContrato.Proveedor;

        if (contrato.TerceroId is int terceroId)
        {
            var tercero = Terceros.FirstOrDefault(t => t.Id == terceroId);
            if (tercero is not null && !ReferenceEquals(tercero, TerceroSeleccionado))
                TerceroSeleccionado = tercero; // dispara la carga
            else if (tercero is null)
                await CargarOtraParteAsync(terceroId);
        }
    }

    private async Task CargarOtraParteAsync(int? terceroId)
    {
        if (terceroId is null)
        {
            Otra.Ficha = null;
            return;
        }

        try
        {
            // La lista de terceros no trae representantes ni cuentas: se carga el tercero completo.
            var tercero = await _sender.Send(new GetTerceroByIdQuery(terceroId.Value));
            Otra.Ficha = tercero is null ? null : FichaParte.DesdeTercero(tercero);
        }
        catch (Exception ex)
        {
            MostrarError("No se pudieron cargar los datos de la otra parte: " + ex.Message);
        }
    }

    /// <summary>Datos de "Mis datos" o de "Otra parte" con el representante y la cuenta elegidos (null si no hay ficha).</summary>
    internal ContextoParte? ContextoDe(bool mia) => (mia ? Mia : Otra).Contexto();

    /// <summary>Hay datos escritos por el autollenado aún resaltados en Word.</summary>
    [ObservableProperty]
    private bool _hayResaltado;

    /// <summary>
    /// Lee el documento activo en Word, reconoce los huecos y a qué parte pertenecen, muestra la revisión
    /// y escribe lo confirmado (resaltado en amarillo).
    /// </summary>
    [RelayCommand]
    private void Autollenar()
    {
        var (lectura, error) = WordInterop.LeerDocumentoActivo();
        if (lectura is null)
        {
            MostrarError(error ?? "No se pudo leer el documento.");
            return;
        }

        var analisis = AnalizadorProforma.Analizar(lectura.Parrafos);
        if (analisis.Huecos.Count == 0)
        {
            MostrarError($"No encontré en «{lectura.Nombre}» espacios para llenar que reconozca (líneas ____ o puntos …. junto a «NIT», «domicilio», «representada por»…). Use los botones de abajo para insertar donde esté el cursor.");
            return;
        }

        // Las denominaciones del propio documento (EL ARRENDADOR, LA COMPRADORA…) pasan a ser las del panel.
        var miDenominacion = SoyProveedor ? analisis.DenominacionProveedor : analisis.DenominacionCliente;
        var suDenominacion = SoyProveedor ? analisis.DenominacionCliente : analisis.DenominacionProveedor;
        if (miDenominacion is not null) DenominacionMia = miDenominacion;
        if (suDenominacion is not null) DenominacionOtra = suDenominacion;

        var revision = new RevisionAutollenadoViewModel(this, lectura, analisis);
        var ventana = new Views.LlenadoContratos.RevisionAutollenadoWindow(revision)
        {
            Owner = VentanaPanel()
        };
        if (ventana.ShowDialog() != true)
        {
            EstadoEsError = false;
            Estado = "Autollenado cancelado: no se escribió nada.";
            return;
        }

        var resultado = WordInterop.AplicarAutollenado(lectura.Documento, revision.Reemplazos());
        if (!resultado.Ok)
        {
            MostrarError(resultado.Mensaje ?? "No se pudo completar el autollenado.");
            return;
        }

        HayResaltado = resultado.Aplicados > 0;
        EstadoEsError = resultado.Omitidos > 0;
        Estado = $"Se llenaron {resultado.Aplicados} datos en «{lectura.Nombre}» (resaltados en amarillo; Ctrl+Z en Word los deshace todos)."
                 + (resultado.Omitidos > 0
                     ? $" {resultado.Omitidos} no se escribieron porque esa parte del documento cambió después de analizarla: vuelva a pulsar «Autollenar»."
                     : " Revíselos y pulse «Quitar resaltado» al terminar.");
    }

    [RelayCommand]
    private void QuitarResaltado()
    {
        var resultado = WordInterop.QuitarResaltado();
        HayResaltado = false;
        EstadoEsError = !resultado.Ok;
        Estado = resultado.Ok ? $"Resaltado quitado en «{resultado.Documento}»." : resultado.Mensaje ?? string.Empty;
    }

    private const string ObjetivoAdjunto = "Contrato llenado en Word";

    /// <summary>
    /// Guarda el documento activo de Word en los adjuntos del contrato: actualiza el adjunto del que se abrió
    /// (o uno con el mismo nombre) o lo agrega como adjunto nuevo. Si el panel no tiene contrato, se elige.
    /// </summary>
    [RelayCommand]
    private async Task GuardarEnContratoAsync()
    {
        try
        {
            var auth = App.Services.GetRequiredService<IAuthContext>();
            if (!auth.TienePermiso(Permissions.AdjuntosGestionar))
            {
                MostrarError("Su usuario no tiene permiso para adjuntar documentos a los contratos.");
                return;
            }

            var (doc, error) = WordInterop.EstadoDocumentoActivo();
            if (doc is null)
            {
                MostrarError(error ?? "No se pudo consultar el documento de Word.");
                return;
            }

            // 1) Contrato y adjunto de destino.
            var origen = ContratosEnWord.AdjuntoDeOrigen(doc.Documento) is int origenId
                ? await _sender.Send(new GetDocumentoAdjuntoByIdQuery(origenId))
                : null;

            Contrato? contrato = _contrato;
            if (origen is not null && origen.ContratoId != contrato?.Id)
                contrato = await _sender.Send(new GetContratoByIdQuery(origen.ContratoId));
            contrato ??= ElegirContrato(await _sender.Send(new GetAllContratosQuery()), doc.Nombre);
            if (contrato is null)
            {
                if (!EstadoEsError)
                    Estado = "No se guardó: no se eligió contrato.";
                return;
            }

            var nombreArchivo = ContratosEnWord.EsWord(System.IO.Path.GetExtension(doc.Nombre))
                ? doc.Nombre
                : System.IO.Path.GetFileNameWithoutExtension(doc.Nombre) + ".docx";

            var reemplazar = origen;
            if (origen is not null)
            {
                var r = Preguntar($"Este documento se abrió desde el adjunto «{origen.NombreArchivo}» del contrato {contrato.Numero}.\n\n" +
                                  "Sí: actualizar ese adjunto con esta versión.\nNo: guardarlo como un adjunto nuevo.");
                if (r == MessageBoxResult.Cancel) return;
                if (r == MessageBoxResult.No) reemplazar = null;
            }
            else
            {
                var mismoNombre = (await _sender.Send(new GetDocumentosAdjuntosQuery(contrato.Id)))
                    .LastOrDefault(a => string.Equals(a.NombreArchivo, nombreArchivo, StringComparison.OrdinalIgnoreCase));
                if (mismoNombre is not null)
                {
                    var r = Preguntar($"El contrato {contrato.Numero} ya tiene un adjunto «{mismoNombre.NombreArchivo}» (del {mismoNombre.FechaCarga.ToLocalTime():dd/MM/yyyy HH:mm}).\n\n" +
                                      "Sí: sustituirlo por esta versión.\nNo: guardar esta como un adjunto más.");
                    if (r == MessageBoxResult.Cancel) return;
                    if (r == MessageBoxResult.Yes) reemplazar = mismoNombre;
                }
            }

            // 2) El resaltado del autollenado es una ayuda para revisar: normalmente no debe quedar en el contrato.
            var quitarResaltado = false;
            if (doc.TieneResaltado)
            {
                var r = Preguntar("El documento aún tiene en amarillo los datos escritos por el autollenado.\n\n" +
                                  "Sí: quitar el resaltado y guardar.\nNo: guardar con el resaltado.");
                if (r == MessageBoxResult.Cancel) return;
                quitarResaltado = r == MessageBoxResult.Yes;
            }

            // 3) Word guarda el archivo y se lee su contenido.
            var carpetaCopia = System.IO.Path.Combine(ContratosEnWord.CarpetaTrabajo, "nuevos");
            var (ruta, errorGuardar) = WordInterop.GuardarDocumentoActivo(doc.Documento, quitarResaltado, carpetaCopia);
            if (ruta is null)
            {
                MostrarError(errorGuardar ?? "Word no pudo guardar el documento.");
                return;
            }
            if (quitarResaltado) HayResaltado = false;

            var contenido = await ContratosEnWord.LeerAsync(ruta);
            if (reemplazar is not null)
                await _sender.Send(new ReemplazarContenidoAdjuntoCommand(reemplazar.Id, contenido));
            else
                await _sender.Send(new AdjuntarDocumentoCommand(contrato.Id, nombreArchivo, ObjetivoAdjunto, contenido));

            ContratosEnWord.NotificarAdjuntosCambiados(contrato.Id);
            if (_contrato?.Id != contrato.Id)
                await AplicarContratoAsync(contrato);

            EstadoEsError = false;
            Estado = (reemplazar is not null
                         ? $"Actualizado el adjunto «{reemplazar.NombreArchivo}» del contrato {contrato.Numero}."
                         : $"Guardado «{nombreArchivo}» en los adjuntos del contrato {contrato.Numero}.")
                     + (string.Equals(ruta, doc.Documento, StringComparison.OrdinalIgnoreCase)
                         ? string.Empty
                         : " Word lo guardó como .docx en la carpeta de trabajo de la aplicación y sigue con esa copia.");
        }
        catch (Exception ex)
        {
            MostrarError("No se pudo guardar en el contrato: " + ex.Message);
        }
    }

    private Contrato? ElegirContrato(IReadOnlyList<Contrato> contratos, string documento)
    {
        if (contratos.Count == 0)
        {
            MostrarError("No hay contratos registrados. Cree el contrato y vuelva a pulsar «Guardar en el contrato».");
            return null;
        }

        EstadoEsError = false;
        var ventana = new Views.LlenadoContratos.ElegirContratoWindow(contratos, documento, TerceroSeleccionado?.Id, TerceroSeleccionado?.Nombre)
        {
            Owner = VentanaPanel()
        };
        return ventana.ShowDialog() == true ? ventana.Seleccionado : null;
    }

    private static MessageBoxResult Preguntar(string mensaje) =>
        VentanaPanel() is { } panel
            ? MessageBox.Show(panel, mensaje, "Guardar en el contrato", MessageBoxButton.YesNoCancel, MessageBoxImage.Question)
            : MessageBox.Show(mensaje, "Guardar en el contrato", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

    private static Window? VentanaPanel() =>
        System.Windows.Application.Current.Windows.OfType<Views.LlenadoContratos.PanelWordWindow>().FirstOrDefault();

    internal string Denominacion(ParteViewModel parte) =>
        ReferenceEquals(parte, Mia) ? DenominacionMia : DenominacionOtra;

    internal string PlantillaComparecencia => _plantillaComparecencia;
    internal string PlantillaFirma => _plantillaFirma;

    internal void Insertar(string texto, string descripcion)
    {
        var resultado = WordInterop.InsertarEnCursor(texto);
        if (resultado.Ok)
        {
            EstadoEsError = false;
            Estado = $"Insertado: {descripcion} — en «{resultado.Documento}».";
        }
        else
        {
            MostrarError(resultado.Mensaje ?? "No se pudo insertar.");
        }
    }

    private void MostrarError(string mensaje)
    {
        EstadoEsError = true;
        Estado = mensaje;
    }

    [RelayCommand]
    private void AbrirDocumento()
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Abrir la proforma de contrato",
            Filter = "Documentos de Word|*.docx;*.doc;*.docm;*.rtf|Todos los archivos|*.*"
        };
        if (dialogo.ShowDialog() != true) return;

        var resultado = WordInterop.AbrirDocumento(dialogo.FileName);
        if (resultado.Ok)
        {
            EstadoEsError = false;
            Estado = $"Abriendo «{resultado.Documento}» en Word. Coloque el cursor donde quiera insertar y haga clic en un dato.";
        }
        else
        {
            MostrarError(resultado.Mensaje ?? "No se pudo abrir el documento.");
        }
    }
}

/// <summary>Una de las dos partes del contrato dentro del panel.</summary>
public sealed partial class ParteViewModel : ObservableObject
{
    private readonly PanelWordViewModel _panel;
    private readonly ObservableCollection<CampoInsertable> _campos = new();

    public ParteViewModel(string titulo, PanelWordViewModel panel)
    {
        Titulo = titulo;
        _panel = panel;
        CamposVista = new ListCollectionView(_campos);
        CamposVista.GroupDescriptions.Add(new PropertyGroupDescription(nameof(CampoInsertable.Grupo)));
    }

    public string Titulo { get; }

    /// <summary>Campos agrupados (Entidad, Representante, Cuenta bancaria).</summary>
    public ICollectionView CamposVista { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneFicha), nameof(Representantes), nameof(Cuentas))]
    [NotifyCanExecuteChangedFor(nameof(InsertarComparecenciaCommand), nameof(InsertarFirmaCommand))]
    private FichaParte? _ficha;

    [ObservableProperty]
    private RepresentanteLegal? _representante;

    [ObservableProperty]
    private CuentaBancaria? _cuenta;

    public bool TieneFicha => Ficha is not null;

    public IReadOnlyList<RepresentanteLegal> Representantes => Ficha?.Representantes ?? Array.Empty<RepresentanteLegal>();
    public IReadOnlyList<CuentaBancaria> Cuentas => Ficha?.Cuentas ?? Array.Empty<CuentaBancaria>();

    partial void OnFichaChanged(FichaParte? value)
    {
        Representante = value?.RepresentantePredeterminado;
        Cuenta = value?.CuentaPara(_panel.Moneda);
        Recalcular();
    }

    partial void OnRepresentanteChanged(RepresentanteLegal? value) => Recalcular();
    partial void OnCuentaChanged(CuentaBancaria? value) => Recalcular();

    internal void ElegirCuentaPara(string moneda)
    {
        if (Ficha is not null)
            Cuenta = Ficha.CuentaPara(moneda);
    }

    internal ContextoParte? Contexto() =>
        Ficha is null ? null : new ContextoParte(Ficha, Representante, Cuenta, _panel.Denominacion(this));

    private void Recalcular()
    {
        _campos.Clear();
        if (Contexto() is { } ctx)
        {
            foreach (var campo in BloquesContrato.Campos(ctx))
                _campos.Add(campo);
        }
    }

    [RelayCommand]
    private void InsertarCampo(CampoInsertable? campo)
    {
        if (campo is null || !campo.TieneValor) return;
        _panel.Insertar(campo.Valor, $"{campo.Etiqueta.ToLowerInvariant()} ({Titulo.ToLowerInvariant()})");
    }

    [RelayCommand(CanExecute = nameof(TieneFicha))]
    private void InsertarComparecencia()
    {
        if (Contexto() is { } ctx)
            _panel.Insertar(BloquesContrato.Renderizar(_panel.PlantillaComparecencia, ctx), $"comparecencia ({Titulo.ToLowerInvariant()})");
    }

    [RelayCommand(CanExecute = nameof(TieneFicha))]
    private void InsertarFirma()
    {
        if (Contexto() is { } ctx)
            _panel.Insertar(BloquesContrato.Renderizar(_panel.PlantillaFirma, ctx), $"bloque de firma ({Titulo.ToLowerInvariant()})");
    }
}
