# Etapa 8: Ciclo de Vida, Estados e Historial

## Objetivo
Implementar la gestión de estados de contratos/suplementos (Borrador, Vigente, Vencido, Rescindido, Ejecutado), la cascada de rescisión, la ejecución con propuesta de suplementos, y el historial completo de cambios.

---

## Tareas

### 8.1 Gestión de Estados

- [x] **Application**: `CambiarEstadoContratoCommand` - cambia estado con registro en historial (R07)
- [x] **Windows**: `CambiarEstadoWindow` - selector de nuevo estado

### 8.2 Rescisión en Cascada (R01, R02)

- [x] **Application**: `GetDocumentosAfectadosPorRescisionQuery` - obtiene documentos que serían rescindidos en cascada
- [x] **Application**: `RescindirContratoCommand` - rescinde recursivamente (Marco→Específicos→Suplementos, R01/R02)
- [x] **Windows**: Diálogo de confirmación mostrando documentos afectados en cascada
- [x] **Windows**: Feedback visual del resultado (contador de documentos rescindidos)

### 8.3 Ejecución con Propuesta (R08)

- [x] **Application**: `EjecutarContratoCommand` - marca como Ejecutado con lista de suplementos seleccionados
- [x] **Windows**: `EjecutarContratoWindow` - checkboxes para seleccionar suplementos a ejecutar
- [x] **Windows**: Botones "Seleccionar todos" / "Deseleccionar todos"

### 8.4 Revertibilidad de Estados (R07)

- [x] **Application**: Cambio de estado en cualquier dirección (CambiarEstadoContratoCommand)
- [x] **Application**: Cada cambio registrado automáticamente en historial con valor anterior y nuevo

### 8.5 Historial de Cambios (Sección 11)

- [x] **Domain**: Entidad `HistorialCambio` (Id, FechaHora, UsuarioId, UsuarioNombre, TipoCambio, ContratoId, Descripcion, ValorAnterior JSON, ValorNuevo JSON)
- [x] **Domain**: Enum `TipoCambio` (Estado, Relacion, Metadato, Lineas)
- [x] **Infrastructure**: `HistorialCambioConfiguration`, migración `20260315170000_AddHistorialCambios`
- [x] **Infrastructure**: `IHistorialCambioStore` e implementación `HistorialCambioStore`
- [x] **Application**: `GetHistorialByContratoQuery` con filtro por tipo de cambio

### 8.6 UI de Historial

- [x] **Windows**: `HistorialCambiosWindow` (3-row dialog convention)
- [x] **Windows**: DataGrid cronológico con fecha, tipo, descripción, usuario
- [x] **Windows**: Filtro por tipo de cambio (ComboBox: Todos/Estado/Relación/Metadato/Líneas)
- [x] **Windows**: Botones "Estado" e "Historial" en ContratosView

### 8.7 Vencimiento Automático

- [x] **Application**: `ActualizarVencimientosCommand` - detecta vigentes con fecha vencida y marca como Vencido
- [x] **Windows**: Ejecuta automáticamente al iniciar la aplicación (App.xaml.cs)

---

## Estado: ✅ Completada
