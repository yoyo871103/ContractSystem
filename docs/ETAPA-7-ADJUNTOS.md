# Etapa 7: Documentos Adjuntos

## Objetivo
Implementar la gestión de archivos adjuntos para contratos y suplementos, con validación de tipos permitidos, campo de objetivo y descarga.

---

## Tareas

### 7.1 Modelo y Persistencia

- [x] **Domain**: Crear entidad `DocumentoAdjunto` (Id, NombreArchivo, Extension, Objetivo, Contenido byte[], TamanioBytes, FechaCarga, UsuarioCargaId, ContratoId FK)
- [x] **Domain**: Constantes de extensiones permitidas (.pdf, .jpg, .png, .gif, .doc, .docx, .xls, .xlsx, .txt) y prohibidas (.exe, .bat, .sh, etc.)
- [x] **Infrastructure**: Configuración EF (`DocumentoAdjuntoConfiguration`), migración `20260315160000_AddDocumentosAdjuntos`
- [x] **Infrastructure**: `IDocumentoAdjuntoStore` e implementación `DocumentoAdjuntoStore` (listado sin blob, descarga con blob)
- [x] **Infrastructure**: Registro DI

### 7.2 Application Layer

- [x] **Application**: `AdjuntarDocumentoCommand` con validación de extensión (permitidas/prohibidas)
- [x] **Application**: `EliminarDocumentoAdjuntoCommand`
- [x] **Application**: `GetDocumentosAdjuntosQuery` (sin contenido blob)
- [x] **Application**: `DescargarDocumentoAdjuntoQuery` (retorna entidad completa con contenido)

### 7.3 UI

- [x] **Windows**: `DocumentosAdjuntosWindow` (3-row dialog convention)
- [x] **Windows**: DataGrid con nombre, extensión, objetivo, tamaño formateado, fecha
- [x] **Windows**: Botón "Adjuntar" con OpenFileDialog filtrado por extensiones permitidas
- [x] **Windows**: Campo "Objetivo" obligatorio al adjuntar (InputBox)
- [x] **Windows**: Botón "Descargar" con SaveFileDialog
- [x] **Windows**: Botón "Eliminar" con confirmación
- [x] **Windows**: Botón "Adjuntos" en ContratosView y comando en ContratosViewModel
- [x] Compilación exitosa (0 errores)

> **Nota de diseño**: El store excluye la columna `Contenido` en listados para evitar cargar blobs en memoria. Solo `DescargarDocumentoAdjuntoQuery` carga el contenido completo. `AdjuntoListItem` formatea el tamaño (B/KB/MB/GB).

---

## Estado: ✅ Completada
