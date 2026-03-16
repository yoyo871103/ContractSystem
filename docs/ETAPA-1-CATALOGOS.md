# Etapa 1: Catálogos del Sistema

## Estado: ✅ COMPLETADA

## Objetivo
Implementar los catálogos base que serán consumidos por el módulo de contratos: Terceros (Clientes/Proveedores), Productos/Servicios y Plantillas de Documentos. La UnidadMedida y BusinessInfo ya existen.

---

## Tareas

### 1.1 Catálogo de Terceros (Clientes/Proveedores)

- [x] **Domain**: Crear entidad `Tercero` (Id, Nombre, RazonSocial, NifCif, Direccion, Telefono, Email, Tipo[CLIENTE/PROVEEDOR/AMBOS], DeletedAt)
- [x] **Domain**: Crear entidad `ContactoTercero` (Id, TerceroId, Nombre, Cargo, Email, Telefono)
- [x] **Domain**: Crear enum `TipoTercero` (Cliente, Proveedor, Ambos)
- [x] **Infrastructure**: Crear configuraciones EF (`TerceroConfiguration`, `ContactoTerceroConfiguration`)
- [x] **Infrastructure**: Agregar DbSets al `ApplicationDbContext`
- [x] **Infrastructure**: Crear migración `20260315100000_AddNomTerceros`
- [x] **Infrastructure**: Crear `ITerceroStore` e implementación `TerceroStore`
- [x] **Application**: Crear Commands (CreateTercero, UpdateTercero, DeleteTercero, ReactivarTercero)
- [x] **Application**: Crear Queries (GetAllTerceros con filtros, GetTerceroById)
- [x] **Windows**: Crear `TerceroView` (listado con DataGrid, filtro por tipo, mostrar activos/inactivos)
- [x] **Windows**: Crear `TerceroDialogWindow` (crear/editar con gestión de contactos en DataGrid)
- [x] **Windows**: Crear `TerceroViewModel` con CRUD completo
- [x] **Windows**: Integrar en la pestaña Nomencladores de Configuración
- [x] **Windows**: Registrar en DI y DataTemplate en App.xaml

### 1.2 Catálogo de Productos/Servicios

- [x] **Domain**: Crear entidad `ProductoServicio` (Id, Codigo, Nombre, Descripcion, Tipo, UnidadMedidaId, PrecioEstimado, DeletedAt)
- [x] **Domain**: Crear enum `TipoProductoServicio` (Producto, Servicio)
- [x] **Infrastructure**: Crear configuración EF (`ProductoServicioConfiguration`)
- [x] **Infrastructure**: Agregar DbSet al `ApplicationDbContext`
- [x] **Infrastructure**: Crear migración `20260315110000_AddNomProductosServicios`
- [x] **Infrastructure**: Crear `IProductoServicioStore` e implementación `ProductoServicioStore`
- [x] **Application**: Crear Commands (Create, Update, Delete, Reactivar)
- [x] **Application**: Crear Queries (GetAll con filtros, GetById)
- [x] **Windows**: Crear `ProductoServicioView` (listado con DataGrid, filtro por tipo)
- [x] **Windows**: Crear `ProductoServicioDialogWindow` (crear/editar con selector de UnidadMedida)
- [x] **Windows**: Crear `ProductoServicioViewModel`
- [x] **Windows**: Integrar en Nomencladores, DI y App.xaml

### 1.3 Catálogo de Plantillas de Documentos

- [x] **Domain**: Crear entidad `PlantillaDocumento` (Id, Nombre, TipoDocumento, Rol, Archivo, NombreArchivo, FechaCreacion, RevisadoPorLegal)
- [x] **Domain**: Crear enums `TipoDocumentoPlantilla` y `RolPlantilla`
- [x] **Infrastructure**: Crear configuración EF (`PlantillaDocumentoConfiguration`)
- [x] **Infrastructure**: Agregar DbSet, crear migración `20260315120000_AddNomPlantillasDocumento`
- [x] **Infrastructure**: Crear `IPlantillaDocumentoStore` e implementación (listado sin cargar blob)
- [x] **Application**: Crear Commands (Create, Update, Delete) y Queries (GetAll, GetById)
- [x] **Windows**: Crear `PlantillaDocumentoView` (listado con descarga)
- [x] **Windows**: Crear `PlantillaDialogWindow` (crear con carga de archivo Word/PDF)
- [x] **Windows**: Crear `PlantillaDocumentoViewModel`
- [x] **Windows**: Integrar en Nomencladores, DI y App.xaml

---

## Criterios de aceptación
- ✅ Los tres catálogos tienen CRUD completo funcionando
- ✅ Soft delete y reactivación disponibles (Terceros y ProductoServicio)
- ✅ Plantillas con eliminación permanente (no aplica soft delete)
- ✅ Filtrado por tipo en cada catálogo
- ✅ Los diálogos siguen la convención de CONVENCION-DIALOGOS.md (3 filas: título, scroll body, pie fijo)
- ✅ Validaciones de campos obligatorios
- ✅ Compilación exitosa (0 errores)

## Nota
Los validators con FluentValidation se omitieron por ahora ya que las validaciones se hacen en los diálogos WPF antes del submit. Se pueden agregar en una iteración posterior si se necesita validación a nivel de pipeline de MediatR.
