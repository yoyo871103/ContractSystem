namespace ContractSystem.Application.Auth;

/// <summary>
/// Nombres de permisos de la aplicación. El rol "Administrador de sistema" tiene siempre todos los permisos.
/// </summary>
public static class Permissions
{
    // --- Contratos ---
    public const string ContratosVer = "Contratos.Ver";
    public const string ContratosCrear = "Contratos.Crear";
    public const string ContratosEditar = "Contratos.Editar";
    public const string ContratosEliminar = "Contratos.Eliminar";
    public const string ContratosCambiarEstado = "Contratos.CambiarEstado";
    public const string ContratosRescindir = "Contratos.Rescindir";
    public const string ContratosEjecutar = "Contratos.Ejecutar";

    // --- Suplementos ---
    public const string SuplementosCrear = "Suplementos.Crear";
    public const string SuplementosGestionarModificaciones = "Suplementos.GestionarModificaciones";

    // --- Anexos y Líneas ---
    public const string AnexosGestionar = "Anexos.Gestionar";
    public const string AnexosCopiarLineas = "Anexos.CopiarLineas";

    // --- Facturas ---
    public const string FacturasVer = "Facturas.Ver";
    public const string FacturasGestionar = "Facturas.Gestionar";

    // --- Documentos Adjuntos ---
    public const string AdjuntosVer = "Adjuntos.Ver";
    public const string AdjuntosGestionar = "Adjuntos.Gestionar";

    // --- Vistas ---
    public const string ArbolVer = "Arbol.Ver";
    public const string ExpedienteVer = "Expediente.Ver";
    public const string InformesVer = "Informes.Ver";
    public const string HistorialVer = "Historial.Ver";

    // --- Terceros ---
    public const string TercerosVer = "Terceros.Ver";
    public const string TercerosCrear = "Terceros.Crear";
    public const string TercerosEditar = "Terceros.Editar";
    public const string TercerosEliminar = "Terceros.Eliminar";

    // --- Productos/Servicios ---
    public const string ProductosVer = "Productos.Ver";
    public const string ProductosCrear = "Productos.Crear";
    public const string ProductosEditar = "Productos.Editar";
    public const string ProductosEliminar = "Productos.Eliminar";

    // --- Plantillas ---
    public const string PlantillasVer = "Plantillas.Ver";
    public const string PlantillasGestionar = "Plantillas.Gestionar";

    // --- Configuración ---
    public const string UnidadesMedidaGestionar = "UnidadesMedida.Gestionar";
    public const string DatosNegocioEditar = "DatosNegocio.Editar";
    public const string NumeracionConfigurar = "Numeracion.Configurar";

    // --- Usuarios y Roles ---
    public const string GestionarUsuarios = "GestionarUsuarios";
    public const string UsuariosResetearContraseña = "Usuarios.ResetearContraseña";

    /// <summary>
    /// Todos los permisos del sistema con su descripción y categoría, para seed y UI.
    /// </summary>
    public static readonly IReadOnlyList<PermisoDefinicion> Todos = new List<PermisoDefinicion>
    {
        // Contratos
        new(ContratosVer, "Ver listado y detalle de contratos", "Contratos"),
        new(ContratosCrear, "Crear contratos (Marco, Específico, Independiente)", "Contratos"),
        new(ContratosEditar, "Editar datos de contratos existentes", "Contratos"),
        new(ContratosEliminar, "Eliminar contratos", "Contratos"),
        new(ContratosCambiarEstado, "Cambiar estado de contratos", "Contratos"),
        new(ContratosRescindir, "Rescindir contratos (acción en cascada)", "Contratos"),
        new(ContratosEjecutar, "Marcar contratos como ejecutados", "Contratos"),

        // Suplementos
        new(SuplementosCrear, "Crear suplementos sobre contratos", "Suplementos"),
        new(SuplementosGestionarModificaciones, "Gestionar documentos que modifica un suplemento", "Suplementos"),

        // Anexos
        new(AnexosGestionar, "Crear, editar y eliminar anexos y líneas de detalle", "Anexos"),
        new(AnexosCopiarLineas, "Copiar líneas de un contrato a otro", "Anexos"),

        // Facturas
        new(FacturasVer, "Ver facturas de un contrato", "Facturas"),
        new(FacturasGestionar, "Crear, editar y eliminar facturas", "Facturas"),

        // Adjuntos
        new(AdjuntosVer, "Ver y descargar documentos adjuntos", "Adjuntos"),
        new(AdjuntosGestionar, "Adjuntar y eliminar documentos", "Adjuntos"),

        // Vistas
        new(ArbolVer, "Ver la vista jerárquica de contratos", "Vistas"),
        new(ExpedienteVer, "Ver el expediente/historial de un tercero", "Vistas"),
        new(InformesVer, "Acceder a los informes y reportes", "Vistas"),
        new(HistorialVer, "Ver historial de cambios de contratos", "Vistas"),

        // Terceros
        new(TercerosVer, "Ver listado de terceros", "Terceros"),
        new(TercerosCrear, "Crear nuevos terceros", "Terceros"),
        new(TercerosEditar, "Editar terceros existentes", "Terceros"),
        new(TercerosEliminar, "Eliminar/reactivar terceros", "Terceros"),

        // Productos
        new(ProductosVer, "Ver listado de productos/servicios", "Productos"),
        new(ProductosCrear, "Crear nuevos productos/servicios", "Productos"),
        new(ProductosEditar, "Editar productos/servicios existentes", "Productos"),
        new(ProductosEliminar, "Eliminar/reactivar productos/servicios", "Productos"),

        // Plantillas
        new(PlantillasVer, "Ver y descargar plantillas", "Plantillas"),
        new(PlantillasGestionar, "Crear, editar y eliminar plantillas", "Plantillas"),

        // Configuración
        new(UnidadesMedidaGestionar, "Gestionar unidades de medida", "Configuración"),
        new(DatosNegocioEditar, "Modificar información de la empresa", "Configuración"),
        new(NumeracionConfigurar, "Configurar numeración automática", "Configuración"),

        // Usuarios
        new(GestionarUsuarios, "Gestionar usuarios (ver pestaña y realizar CRUD)", "Usuarios"),
        new(UsuariosResetearContraseña, "Restablecer contraseñas de otros usuarios", "Usuarios"),
    };
}

/// <summary>
/// Definición de un permiso para seed y UI.
/// </summary>
public sealed record PermisoDefinicion(string Nombre, string Descripcion, string Categoria);
