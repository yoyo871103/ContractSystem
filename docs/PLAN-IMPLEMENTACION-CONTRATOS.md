# Plan de Implementación: Sistema de Gestión de Contratos

## Resumen

Plan por etapas para implementar el sistema completo de gestión de contratos y suplementos según las especificaciones en `APLICACIÓN PARA GESTIÓN DE CONTRATO.txt`.

**Ya implementado**: Autenticación, Gestión de usuarios, BusinessInfo (Mi Empresa), UnidadMedida, Conexión BD.

---

## Etapas

| # | Etapa | Documento | Estado |
|---|-------|-----------|--------|
| 1 | Catálogos del Sistema (Terceros, Productos, Plantillas) | [ETAPA-1-CATALOGOS.md](ETAPA-1-CATALOGOS.md) | ✅ Completada |
| 2 | Modelo de Dominio de Contratos y Suplementos | [ETAPA-2-MODELO-CONTRATOS.md](ETAPA-2-MODELO-CONTRATOS.md) | ✅ Completada |
| 3 | Numeración Configurable de Documentos | [ETAPA-3-NUMERACION.md](ETAPA-3-NUMERACION.md) | ✅ Completada |
| 4 | CRUD de Contratos (Marco, Específico, Independiente) | [ETAPA-4-CRUD-CONTRATOS.md](ETAPA-4-CRUD-CONTRATOS.md) | ✅ Completada |
| 5 | CRUD de Suplementos y Relaciones M:N | [ETAPA-5-CRUD-SUPLEMENTOS.md](ETAPA-5-CRUD-SUPLEMENTOS.md) | ✅ Completada |
| 6 | Anexos y Líneas de Detalle | [ETAPA-6-ANEXOS-LINEAS.md](ETAPA-6-ANEXOS-LINEAS.md) | ✅ Completada |
| 7 | Documentos Adjuntos | [ETAPA-7-ADJUNTOS.md](ETAPA-7-ADJUNTOS.md) | ✅ Completada |
| 8 | Ciclo de Vida, Estados e Historial | [ETAPA-8-ESTADOS-HISTORIAL.md](ETAPA-8-ESTADOS-HISTORIAL.md) | ✅ Completada |
| 9 | Vista de Árbol, Búsqueda Avanzada y Alertas | [ETAPA-9-ARBOL-BUSQUEDA-ALERTAS.md](ETAPA-9-ARBOL-BUSQUEDA-ALERTAS.md) | ✅ Completada |

---

## Orden y dependencias

```
Etapa 1 (Catálogos) ──────────────────────┐
Etapa 2 (Modelo Contratos) ───────────────┤
Etapa 3 (Numeración) ─────────────────────┼──▶ Etapa 4 (CRUD Contratos)
                                           │         │
                                           │         ▼
                                           │    Etapa 5 (Suplementos M:N)
                                           │         │
                                           │         ▼
                                           ├──▶ Etapa 6 (Anexos y Líneas)
                                           │
                                           ├──▶ Etapa 7 (Adjuntos) [paralelo con 6]
                                           │
                                           └──▶ Etapa 8 (Estados/Historial)
                                                      │
                                                      ▼
                                                 Etapa 9 (Árbol/Búsqueda/Alertas)
```

- **Etapas 1, 2 y 3** pueden ejecutarse en paralelo (son independientes entre sí)
- **Etapa 4** requiere las tres anteriores
- **Etapa 5** requiere Etapa 4
- **Etapas 6 y 7** pueden ejecutarse en paralelo después de Etapa 5
- **Etapa 8** puede comenzar después de Etapa 5
- **Etapa 9** requiere todas las anteriores

---

## Reglas de negocio cubiertas

| ID | Regla | Etapa |
|----|-------|-------|
| R01 | Rescindir Marco → rescinde Específicos | 8 |
| R02 | Rescindir Específico/Independiente → rescinde Suplementos | 8 |
| R03 | fecha_firma(A) >= fecha_firma(B) para relación "modifica a" | 2, 5 |
| R04 | No relaciones circulares | 2, 5 |
| R05 | Snapshot al seleccionar de catálogo | 6 |
| R06 | Números de documento únicos | 3 |
| R07 | Estados revertibles con historial | 8 |
| R08 | Al ejecutar, proponer suplementos | 8 |
| R09 | Suplementos de Marco solo para modificaciones generales | 2, 5 |

---

## Convenciones

- **Diálogos**: Seguir [CONVENCION-DIALOGOS.md](CONVENCION-DIALOGOS.md) (3 filas: título fijo, body con scroll, pie fijo)
- **Arquitectura**: Clean Architecture (Domain → Application → Infrastructure → Windows)
- **Patrones**: CQRS con MediatR, FluentValidation, MVVM Toolkit, Soft Delete con ISoftDeletable
