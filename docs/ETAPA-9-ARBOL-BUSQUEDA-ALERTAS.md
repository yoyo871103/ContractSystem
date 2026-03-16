# Etapa 9: Vista de Árbol, Búsqueda Avanzada y Alertas

## Objetivo
Implementar la vista de reporte consolidado en árbol jerárquico, la búsqueda avanzada y el sistema de alertas de vencimiento.

---

## Tareas

### 9.1 Vista de Árbol (Sección 12)

- [x] **Application**: `GetArbolContratosQuery` retorna estructura jerárquica (`NodoArbol` record recursivo)
- [x] **Windows**: `ArbolContratosView` con TreeView expandible/colapsable
- [x] **Windows**: Iconos diferenciados por tipo (Marco=folder, Específico=page, Independiente=doc, Suplemento=edit)
- [x] **Windows**: Colores por estado (Vigente=verde, Vencido=amarillo, Rescindido=rojo, Ejecutado=cyan, Borrador=gris)
- [x] **Windows**: Información por nodo: Número [Estado] — Objeto (truncado a 60 chars)
- [x] **Windows**: Orden por fecha de firma (más antiguo primero)
- [x] **Windows**: `ArbolContratosViewModel` con filtros y carga asíncrona
- [x] **Windows**: Navegación "Árbol" en sidebar del MainWindow

### 9.2 Filtros del Árbol

- [x] **Windows**: Panel de filtros (Tipo, Estado, Rol) con ComboBoxes
- [x] **Windows**: Los filtros reducen el árbol mostrando solo ramas con coincidencias
- [x] **Application**: Filtros aplicados recursivamente (incluye nodo si coincide o tiene hijos que coinciden)

### 9.3 Búsqueda Avanzada (Sección 13)

- [x] **Application**: `BusquedaAvanzadaQuery` con criterios combinables:
  - Por Tercero, texto en producto/servicio (líneas de detalle), rango de fechas (firma y vigencia), tipo, estado, texto en objeto

### 9.4 Alertas de Vencimiento (Sección 4)

- [x] **Application**: `GetContratosProximosAVencerQuery` (configurable en días de antelación, default 30)
- [x] **Application**: `GetResumenContratosQuery` (contadores por estado + próximos a vencer)

### 9.5 Dashboard de Inicio

- [x] **Windows**: InicioView actualizado con dashboard:
  - Resumen de contratos por estado (tarjetas con colores)
  - Lista de contratos próximos a vencer (DataGrid con fondo amarillo)
  - Botón Actualizar
- [x] **Windows**: `InicioViewModel` con carga de resumen y alertas via ISender

### 9.6 Integración

- [x] **Windows**: DataTemplate ArbolContratosViewModel→ArbolContratosView en App.xaml
- [x] **Windows**: ArbolContratosViewModel registrado en DI
- [x] **Windows**: NavigateToArbol en MainViewModel
- [x] Compilación exitosa (0 errores)

---

## Estado: ✅ Completada
