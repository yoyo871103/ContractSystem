# Etapa 6: Anexos y Líneas de Detalle

## Objetivo
Implementar los anexos (secciones internas de documentos) y las líneas de detalle (productos/servicios) con soporte para catálogo, inline y snapshot.

---

## Tareas

### 6.1 Modelo de Anexos

- [x] **Domain**: Crear entidad `Anexo`:
  - Id, Nombre, CondicionesEntrega, CostosAsociados
  - ContratoId (FK), Orden (int)
- [x] **Infrastructure**: Configuración EF, migración
- [x] **Infrastructure**: Crear `IAnexoStore` e implementación
- [x] **Application**: Commands (Create, Update, Delete)
- [x] **Application**: Queries (GetByContrato)

### 6.2 Modelo de Líneas de Detalle

- [x] **Domain**: Crear entidad `LineaDetalle`:
  - Id, Tipo (TipoProductoServicio), Concepto, Descripcion
  - Cantidad (decimal), PrecioUnitario (decimal), ImporteTotal (decimal)
  - UnidadMedidaId (nullable informativo), UnidadMedidaTexto
  - ProductoServicioOrigenId (nullable informativo, snapshot R05)
  - ContratoId (FK), AnexoId (FK nullable)
  - EsCopiaDeOriginal (bool), Orden
- [x] **Infrastructure**: Configuración EF, migración
- [x] **Infrastructure**: Crear `ILineaDetalleStore` e implementación
- [x] **Application**: Commands (Create, Update, Delete, CopiarLineasContrato)
- [x] **Application**: Queries (GetByContrato, GetByAnexo)
- [x] **Application**: Validar R05 - snapshot al seleccionar de catálogo

### 6.3 UI de Anexos y Líneas

- [x] **Windows**: `AnexosLineasWindow` con TabControl (Anexos / Líneas)
- [x] **Windows**: Agregar/Eliminar anexos con InputBox
- [x] **Windows**: Agregar líneas desde catálogo (`SeleccionProductoWindow`, snapshot R05)
- [x] **Windows**: Agregar líneas inline
- [x] **Windows**: Totales del documento (suma de importes)
- [x] **Windows**: Botón "Anexos/Líneas" en `ContratosView` y comando en ViewModel

### 6.4 Compilación

- [x] Verificar compilación exitosa (0 errores)

> **Nota de diseño**: Se usó tabla única para Contratos (incluyendo Suplementos), por lo que Anexo y LineaDetalle usan ContratoId como FK directa. El snapshot R05 copia datos del catálogo al momento de selección (ProductoServicioOrigenId es solo informativo).

---

## Estado: ✅ Completada
