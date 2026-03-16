# Etapa 3: Numeración Configurable de Documentos

## Objetivo
Implementar el sistema de numeración automática y configurable para contratos y suplementos, con soporte para prefijos, sufijos, variables automáticas y contador secuencial.

---

## Tareas

### 3.1 Modelo y Persistencia

- [x] **Domain**: Crear entidad `ConfiguracionNumeracion` (Formato, DigitosPadding, ContadorPorAnio, Activa)
- [x] **Domain**: Crear entidad `ContadorNumeracion` (Anio nullable, UltimoNumero)
- [x] **Infrastructure**: Crear `ConfiguracionNumeracionConfiguration` y `ContadorNumeracionConfiguration`
- [x] **Infrastructure**: Agregar DbSets al ApplicationDbContext
- [x] **Infrastructure**: Crear migración `20260315140000_AddNumeracionConfigurable` (con seed de configuración por defecto)
- [x] **Infrastructure**: Crear `IConfiguracionNumeracionStore` e implementación (incluye ObtenerSiguienteNumeroAsync)

### 3.2 Servicio de Numeración

- [x] **Application**: Crear `IDocumentoNumeracionService` (GenerarNumeroAsync, VistaPrevia)
- [x] **Infrastructure**: Implementar `DocumentoNumeracionService`:
  - Resolución de variables: {YYYY}, {MM}, {TIPO}, {CODIGO_CLIENTE}, {CONTADOR}
  - Verificación de unicidad del número generado (R06)
  - Vista previa sin incrementar contador

### 3.3 Commands/Queries MediatR

- [x] **Application**: `UpdateConfiguracionNumeracionCommand` (crea o actualiza la configuración activa)
- [x] **Application**: `GetConfiguracionNumeracionQuery` (obtiene la configuración activa)
- [x] **Application**: `GetVistaPreviaNumeracionQuery` (genera vista previa del formato)

### 3.4 UI de Configuración

- [x] **Windows**: Crear `NumeracionConfigViewModel` con vista previa en tiempo real
- [x] **Windows**: Crear `NumeracionConfigView` (formato editable, variables disponibles, padding, tipo contador, vista previa)
- [x] **Windows**: Agregar pestaña "Numeración de Documentos" en ConfiguracionView
- [x] **Windows**: Registrar DataTemplate en App.xaml
- [x] **Windows**: Registrar ViewModel y servicios en DI

### 3.5 Integración

- [x] **Infrastructure**: Registrar `IConfiguracionNumeracionStore` y `IDocumentoNumeracionService` en DI
- [x] **Verificación**: Compilación exitosa sin errores

---

## Criterios de aceptación
- ✅ El formato es configurable desde la UI
- ✅ Las variables se resuelven correctamente ({YYYY}, {MM}, {TIPO}, {CODIGO_CLIENTE}, {CONTADOR})
- ✅ Soporte para contador por año o global
- ✅ Vista previa del formato funcionando en tiempo real
- ✅ Seed de configuración por defecto: "CON-{TIPO}-{YYYY}-{CONTADOR}"
- ✅ Compilación exitosa
