# Etapa 5: CRUD de Suplementos y Relaciones M:N

## Objetivo
Implementar la creación, edición y gestión de suplementos con sus relaciones de modificación (M:N), incluyendo validaciones de fechas, prevención de ciclos y trazabilidad bidireccional.

> **Nota de diseño**: Los suplementos son Contratos con TipoDocumento=Suplemento en la tabla única. Las relaciones M:N se manejan via ModificacionDocumento.

---

## Tareas

### 5.1 Application Layer

- [x] **Application**: Crear `CreateSuplementoCommand` y handler (con relaciones "modifica a", R03, R04, R06, R09)
- [x] **Application**: Crear `AddModificacionCommand` y handler (R03, R04)
- [x] **Application**: Crear `RemoveModificacionCommand` y handler
- [x] **Application**: Crear `GetSuplementosByContratoQuery` (hijos de un contrato)
- [x] **Application**: Reutilizar `UpdateContratoCommand` y `DeleteContratoCommand` para suplementos

### 5.2 Diálogo de Crear Suplemento

- [x] **Windows**: Crear `SuplementoDialogWindow` siguiendo CONVENCION-DIALOGOS.md (3 filas)
- [x] **Windows**: Sección "Datos Básicos": Rol, Número (auto + editable), Objeto
- [x] **Windows**: Sección "Modificación de Generales": Checkbox (solo visible si padre es Marco, R09)
- [x] **Windows**: Sección "Fechas": Firma, Entrada en vigor, Vigencia, Duración
- [x] **Windows**: Sección "Partes": Tercero (preseleccionado del padre)
- [x] **Windows**: Sección "Documentos que Modifica": DataGrid editable con selector de documentos destino y descripción
- [x] **Windows**: Sección "Valores Económicos": Valor total, Condiciones, Costos
- [x] **Windows**: Validaciones en formulario: número/objeto obligatorios, R09 para Marco
- [x] **Windows**: Pre-configuración desde contrato padre (rol, tercero, info del padre)

### 5.3 Integración con Listado

- [x] **Windows**: Agregar botón "Nuevo Suplemento" en ContratosView (solo habilitado con contrato seleccionado activo)
- [x] **Windows**: `NuevoSuplementoCommand` en ContratosViewModel con carga de datos auxiliares
- [x] **Windows**: Suplementos aparecen en el listado general con TipoDocumento=Suplemento

### 5.4 Verificación

- [x] **Compilación**: Exitosa sin errores

---

## Criterios de aceptación
- ✅ Se pueden crear suplementos desde cualquier contrato activo
- ✅ Las relaciones M:N "modifica a" funcionan con descripción por relación
- ✅ Validación R03 (fechas) implementada en CreateSuplementoCommand
- ✅ Validación R04 (ciclos) implementada en CreateSuplementoCommand y AddModificacionCommand
- ✅ Validación R09 (Marco) restringe suplementos de Marco a modificaciones generales
- ✅ El suplemento hereda rol y tercero del contrato padre
- ✅ Compilación exitosa
