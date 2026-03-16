# Etapa 2: Modelo de Dominio de Contratos y Suplementos

## Objetivo
Crear las entidades de dominio, persistencia y migraciones para contratos, suplementos, sus relaciones jerárquicas y la tabla M:N de modificaciones. Esta etapa es solo backend (sin UI).

> **Decisión de diseño**: Se usa una **tabla única** para Contratos y Suplementos (discriminador `TipoDocumento`), lo que simplifica las relaciones M:N de modificación ya que ambos tipos viven en la misma tabla.

---

## Tareas

### 2.1 Entidades de Dominio

- [x] **Domain**: Crear enum `TipoDocumentoContrato` (Marco, Especifico, Independiente, Suplemento)
- [x] **Domain**: Crear enum `RolContrato` (Proveedor, Cliente)
- [x] **Domain**: Crear enum `EstadoContrato` (Borrador, Vigente, Vencido, Rescindido, Ejecutado)
- [x] **Domain**: Crear entidad `Contrato` (tabla única para todos los tipos):
  - Id, Numero, Objeto, TipoDocumento, Rol, Estado
  - FechaFirma, FechaEntradaVigor, FechaVigencia, Duracion (texto)
  - Ejecutado (bool), FechaEjecucion
  - MiEmpresaId (FK a BusinessInfo), TerceroId (FK a Tercero)
  - ContratoPadreId (FK nullable, auto-referencia para Marco→Específico y Contrato→Suplemento)
  - ValorTotal (decimal?), CondicionesEntrega, CostosAsociados
  - EsModificacionGenerales (bool, para suplementos)
  - FechaCreacion, ISoftDeletable
- [x] **Domain**: Crear entidad `ModificacionDocumento` (tabla M:N):
  - Id, DocumentoOrigenId, DocumentoDestinoId (ambos FK a Contrato)
  - Descripcion, FechaRegistro

### 2.2 Persistencia

- [x] **Infrastructure**: Crear `ContratoConfiguration` (índices en Numero único, Estado, TerceroId, auto-referencia con Restrict)
- [x] **Infrastructure**: Crear `ModificacionDocumentoConfiguration` (índice compuesto único Origen+Destino, Restrict deletes)
- [x] **Infrastructure**: Agregar DbSets al ApplicationDbContext
- [x] **Infrastructure**: Crear migración `20260315130000_AddContratosYModificaciones`
- [x] **Infrastructure**: Crear `IContratoStore` e implementación (filtros ricos: tipo, estado, rol, tercero, fechas, texto)
- [x] **Infrastructure**: Crear `IModificacionDocumentoStore` e implementación
- [x] **Infrastructure**: Registrar stores en DependencyInjection.cs

### 2.3 Validaciones de Negocio

- [x] **Application**: Crear `IContratoValidationService` + `ValidationResult` record
- [x] **Infrastructure**: Implementar `ContratoValidationService`:
  - R03: fecha_firma(A) >= fecha_firma(B) para relaciones "modifica a"
  - R04: No permitir relaciones circulares (verifica directa e inversa)
  - R06: Números de documento únicos
  - R09: Suplementos de MARCO solo para modificaciones generales
- [x] **Infrastructure**: Registrar `IContratoValidationService` en DI
- [x] **Verificación**: Compilación exitosa sin errores

---

## Criterios de aceptación
- ✅ Todas las entidades creadas con sus relaciones correctas
- ✅ Migración dual-provider (SQL Server + SQLite) creada
- ✅ Índices creados en campos clave
- ✅ Validadores de reglas de negocio críticas implementados
- ✅ Soft delete configurado en Contrato
- ✅ Compilación exitosa
