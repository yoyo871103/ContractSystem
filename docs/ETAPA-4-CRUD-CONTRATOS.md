# Etapa 4: CRUD de Contratos

## Objetivo
Implementar la creación, edición, listado y visualización de contratos (Marco, Específico, Independiente), incluyendo el flujo completo del usuario según la sección 17.1 de las especificaciones.

---

## Tareas

### 4.1 Application Layer

- [x] **Application**: Crear `CreateContratoCommand` y handler (con validación R06)
- [x] **Application**: Crear `UpdateContratoCommand` y handler (con validación R06 excluyendo propio)
- [x] **Application**: Crear `DeleteContratoCommand` (soft delete) y handler
- [x] **Application**: Crear `GetAllContratosQuery` (con filtros: tipo, estado, rol, tercero, fechas, texto)
- [x] **Application**: Crear `GetContratoByIdQuery` (con includes de relaciones)
- [x] **Application**: Crear `GetContratosMarcoQuery` (para seleccionar Marco al crear Específico)

### 4.2 Listado de Contratos

- [x] **Windows**: Crear `ContratosView` como vista principal del módulo
- [x] **Windows**: DataGrid con columnas: Número, Tipo, Objeto, Rol, Estado, Tercero, Fecha Firma, Vigencia
- [x] **Windows**: Panel de filtros (por tipo, estado, rol, búsqueda de texto)
- [x] **Windows**: Botones de acción: Nuevo, Editar, Eliminar, Actualizar
- [x] **Windows**: Indicador visual de contratos vencidos/rescindidos (color diferenciado)
- [x] **Windows**: Crear `ContratosViewModel` con carga de datos, filtros y búsqueda con delay

### 4.3 Diálogo de Crear/Editar Contrato

- [x] **Windows**: Crear `ContratoDialogWindow` siguiendo CONVENCION-DIALOGOS.md (3 filas)
- [x] **Windows**: Sección "Tipo y Rol": Tipo (selector, no editable en edición), Rol (selector)
- [x] **Windows**: Sección "Marco Padre": Solo visible si Tipo=ESPECÍFICO, selector de Contratos Marco
- [x] **Windows**: Sección "Datos Básicos": Número (auto + editable con botón Generar), Objeto (multilinea)
- [x] **Windows**: Sección "Fechas": Firma, Entrada en vigor, Vigencia, Duración (texto libre)
- [x] **Windows**: Sección "Partes": Tercero (selector del catálogo)
- [x] **Windows**: Sección "Valores Económicos": Valor total, Condiciones entrega, Costos asociados
- [x] **Windows**: Validaciones en formulario: número y objeto obligatorios, Marco padre si tipo=Específico
- [x] **Windows**: Integración con servicio de numeración automática (botón Generar)

### 4.4 Navegación

- [x] **Windows**: Agregar botón "Contratos" en el menú lateral (MainWindow sidebar)
- [x] **Windows**: Agregar `NavigateToContratos` command en MainViewModel
- [x] **Windows**: Registrar DataTemplate ContratosViewModel→ContratosView en App.xaml
- [x] **Windows**: Registrar ContratosViewModel en DI

### 4.5 Verificación

- [x] **Compilación**: Exitosa sin errores (0 errores, solo warnings preexistentes)

---

## Criterios de aceptación
- ✅ Se pueden crear contratos Marco, Específico e Independiente
- ✅ Al crear un Específico, se selecciona obligatoriamente un Marco padre
- ✅ El número se genera automáticamente pero es editable
- ✅ Los filtros del listado funcionan correctamente (tipo, estado, rol, búsqueda)
- ✅ Las validaciones impiden datos inconsistentes
- ✅ El diálogo sigue la convención de 3 filas (título fijo, scroll, pie fijo)
- ✅ Contratos vencidos/rescindidos se destacan visualmente
- ✅ Compilación exitosa
