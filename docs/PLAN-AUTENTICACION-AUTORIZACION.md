# Plan de implementación: Autenticación y autorización

Este documento describe el plan para implementar el sistema de autenticación y autorización del Sistema de Inventario, incluyendo la infraestructura necesaria y las reglas de negocio para **SQL Server** (login como administrador SA / modo recuperación) y **SQLite** (recuperación de contraseña sin SA).

---

## 1. Resumen del contexto actual

- **Aplicación**: WPF (InventorySystem.Windows) con capas Application, Domain e Infrastructure.
- **Bases de datos**: SQL Server o SQLite; la conexión se configura en runtime (ConnectionConfigWindow) y se persiste en archivo (FileConnectionConfigurationStore).
- **Login actual**: `LoginWindow` solo muestra usuario/contraseña y al aceptar cierra con `DialogResult = true` sin validar credenciales.
- **SQL Server**: Existe flujo de setup con SA (`SqlServerDatabaseSetupService`) que crea usuario interno `InvApp_*`; la UI de configuración actual usa credenciales de un usuario SQL cualquiera (no obligatoriamente SA).

---

## 2. Objetivos y reglas de negocio

| Aspecto | Regla |
|--------|--------|
| **Autenticación normal** | Usuarios de la aplicación (tablas propias) con usuario/contraseña; acceso completo según roles. |
| **SQL Server – Admin (SA)** | Si se inicia sesión con un usuario administrador de SQL (p. ej. `sa`), **solo** se permite la opción de **gestión de usuarios**: resetear contraseña y definir contraseña temporal. El usuario deberá cambiar esa contraseña en el **primer login**. Esta opción **solo existe para conexión SQL Server**. |
| **SQLite – Recuperación** | No existe SA; debe haber **otro mecanismo** de recuperación cuando se pierde la contraseña (p. ej. archivo de recuperación, clave maestra, o flujo específico documentado). |

---

## 3. Fases del plan (sin implementar aún)

### Fase 1: Infraestructura de autenticación y autorización

Objetivo: tener el soporte técnico para identificar quién está usando la app y con qué permisos, sin aún una pantalla completa de “gestión de usuarios”.

#### 1.1 Modelo de identidad en Domain

- **Entidades** (en Domain, sin EF):
  - `Usuario`: Id, NombreUsuario (único), NombreParaMostrar, HashContraseña, Salt, RequiereCambioContraseña, Activo, FechaCreación, ÚltimoAcceso, etc.
  - `Rol`: Id, Nombre (ej. Administrador, Operador, SoloConsulta).
  - Relación muchos a muchos Usuario–Rol (tabla `UsuarioRol` o equivalente).
- **Valores de negocio**:
  - `RequiereCambioContraseña`: `true` cuando se ha asignado una contraseña temporal (p. ej. desde el modo SA en SQL Server).
  - Al menos un usuario inicial (administrador) debe poder crearse en el primer arranque o durante el setup de la BD.

#### 1.2 Persistencia en Infrastructure

- **DbContext**: agregar `DbSet<Usuario>`, `DbSet<Rol>`, `DbSet<UsuarioRol>` (o entidades EF que mapeen a estas tablas).
- **Configuraciones EF**: Fluent API o IEntityTypeConfiguration para tablas `Usuarios`, `Roles`, `UsuarioRoles`; índices únicos (p. ej. `NombreUsuario`), relaciones y nombres de tabla coherentes para SQL Server y SQLite.
- **Migraciones**: primera migración que cree tablas de identidad (y, si se define después, tablas de auditoría o sesiones).

#### 1.3 Contratos de aplicación (Application)

- **IAuthService** (o nombres análogos):
  - `Task<AuthResult> AuthenticateAsync(string nombreUsuario, string contraseña, CancellationToken ct)`  
    - Valida contra usuarios de la aplicación (hash/salt).  
    - Devuelve éxito/fallo, datos del usuario (id, nombre, roles) y si `RequiereCambioContraseña`.
  - Opcional: `Task<bool> ChangePasswordAsync(int usuarioId, string contraseñaActual, string contraseñaNueva, CancellationToken ct)` para cambio desde la app.
- **IAuthContext** (o ICurrentUser): interfaz para “usuario actual” (Id, NombreUsuario, Roles, EsAdmin, RequiereCambioContraseña). Implementación que se rellena tras login y se usa en toda la app (menú, permisos, redirección al cambio de contraseña).
- **Resultados tipados**: `AuthResult` con: éxito, mensaje de error, datos del usuario (incl. RequiereCambioContraseña y roles).

#### 1.4 Detección de “login como administrador SQL” (solo SQL Server)

- **Criterio**: se considera “modo administrador SQL” cuando el usuario elige explícitamente “Conectar como administrador SQL” (o similar) y las credenciales introducidas permiten abrir una conexión a SQL Server con un login que tenga privilegios suficientes (p. ej. pertenencia a `sysadmin` o login `sa`).
- **No reutilizar** la cadena de conexión guardada para el día a día: usar una conexión **temporal** solo para esta comprobación y para las operaciones de “gestión de usuarios” (resetear contraseña, establecer temporal).
- **Contratos**:
  - `ISqlServerAdminAuthService` (o similar), solo registrado cuando el proveedor es SQL Server:
    - `Task<SqlAdminAuthResult> ValidateSqlAdminAsync(string server, string user, string password, string? database, CancellationToken ct)`: abre conexión con esas credenciales y comprueba si el login es administrador (p. ej. `IS_SRVROLEMEMBER('sysadmin')`). No guardar contraseña.
    - Devuelve éxito/fallo y, si aplica, datos mínimos para mostrar “modo solo gestión de usuarios”.
  - La UI de login tendrá una opción visible solo cuando la conexión configurada sea SQL Server: “Entrar como administrador SQL” que pida servidor, usuario, contraseña (y opcionalmente base de datos) y llame a `ValidateSqlAdminAsync`. Si es correcto, no se usa `IAuthService` de usuarios de app; se navega a una “vista restringida” solo con gestión de usuarios.

#### 1.5 Flujo de login unificado (lógica, sin UI final)

1. Si el usuario eligió “Entrar como administrador SQL” (solo si Provider == SqlServer):  
   - Llamar a `ValidateSqlAdminAsync`.  
   - Si OK: establecer “sesión” de tipo solo-admin-SQL (sin usuario de aplicación), abrir ventana principal con **solo** menú/pantalla “Gestión de usuarios” (resetear contraseña, definir temporal).  
   - Si fallo: mostrar error y permanecer en login.
2. Si no (login normal):  
   - Llamar a `IAuthService.AuthenticateAsync(nombreUsuario, contraseña)`.  
   - Si OK y `RequiereCambioContraseña`: redirigir a pantalla “Cambiar contraseña” obligatoria; tras cambiarla, continuar al resto de la app.  
   - Si OK y no requiere cambio: abrir ventana principal con permisos según roles.  
   - Si fallo: mostrar credenciales incorrectas.

#### 1.6 Autorización en la app

- **Puntos de control**: antes de abrir vistas o ejecutar comandos sensibles (gestión de usuarios, configuración, informes, etc.), consultar `IAuthContext` (roles o permisos).  
- **Vista “solo gestión de usuarios”**: cuando la sesión es “admin SQL”, solo se muestra esta funcionalidad; el resto de menús/vistas no se muestran o se deshabilitan.  
- No implementar aún la pantalla completa de gestión de usuarios; solo la infraestructura (servicios, contexto, detección de admin SQL y flujo de login).

---

### Fase 2: Gestión de usuarios (siguiente paso)

Objetivo: pantallas y casos de uso para administrar usuarios, contraseñas temporales y primer login.

#### 2.1 Solo para conexión SQL Server – Modo administrador SQL

- **Acceso**: únicamente cuando se ha entrado con “Entrar como administrador SQL” (credenciales SA o equivalente).
- **Funcionalidades**:
  - Listar usuarios de la aplicación (tabla `Usuarios`).
  - **Resetear contraseña**: elegir usuario, generar o introducir contraseña temporal, marcar `RequiereCambioContraseña = true`. El usuario deberá cambiar la contraseña en el próximo login normal.
  - No se permite aquí crear/eliminar usuarios si se desea limitar a “solo reseteo”; o bien se puede permitir crear/desactivar usuarios según criterio.
- **Seguridad**: no persistir en ningún sitio la contraseña del SA; usar la conexión temporal solo en memoria para ejecutar los comandos de actualización en la BD de la aplicación.

#### 2.2 Contraseña temporal y primer login

- Al iniciar sesión normal, si `RequiereCambioContraseña == true`:
  - Redirigir a “Cambiar contraseña” (pantalla o diálogo).
  - Validar contraseña actual (la temporal), pedir nueva dos veces, actualizar hash y poner `RequiereCambioContraseña = false`.
  - Después permitir acceso normal.
- Esto aplica tanto a usuarios que vengan de un reseteo por SA (SQL Server) como a cualquier otro flujo futuro que marque contraseña temporal.

#### 2.3 SQLite – Recuperación cuando se pierde la contraseña

- **No** hay SA ni “entrar como administrador SQL”. Se debe definir **un solo** mecanismo de recuperación (o varios opcionales) y documentarlo. Opciones razonables:
  - **Opción A – Archivo de recuperación**: durante el primer setup (o desde un usuario ya autenticado), se genera un archivo (p. ej. `recovery.key`) que se guarda en una ubicación segura. Si se pierde la contraseña, la app permite “Recuperar acceso” introduciendo o seleccionando ese archivo; con él se puede restablecer la contraseña del administrador o desbloquear un único usuario admin.
  - **Opción B – Pregunta secreta / correo**: menos ideal en una app de escritorio sin servidor de correo; se puede dejar para una fase posterior.
  - **Opción C – Borrar archivo de BD y volver a configurar**: opción “nuclear”: borrar o renombrar el archivo SQLite y volver a pasar el asistente de configuración; se pierden todos los datos. Solo como último recurso y bien documentado.
- **Recomendación**: implementar **Opción A** (archivo de recuperación) para SQLite y documentar el flujo en la ayuda o en un README. La infraestructura podría incluir:
  - Una clave maestra derivada del archivo de recuperación (o el archivo firmado/cifrado) que permita en la app “modo recuperación” y, con ello, restablecer la contraseña del primer usuario administrador.
- En la Fase 1 no es obligatorio implementar ya el flujo completo de recuperación SQLite; sí dejar **definido** en el plan que será un camino distinto al de SQL Server (sin SA).

---

## 4. Resumen de componentes a crear (Fase 1)

| Capa | Componentes |
|------|-------------|
| **Domain** | Entidades `Usuario`, `Rol`, relación Usuario–Rol; valores como `RequiereCambioContraseña`. |
| **Infrastructure** | DbSets y configuraciones EF; migración inicial de tablas de identidad; implementación de `IAuthService` (hash/verificación con salt); implementación de `ISqlServerAdminAuthService` (validar sysadmin/sa); implementación de `IAuthContext` (usuario actual en memoria). |
| **Application** | Contratos: `IAuthService`, `IAuthContext`, `ISqlServerAdminAuthService`; DTOs/resultados `AuthResult`, `SqlAdminAuthResult`. |
| **Windows** | Ajustes en `LoginWindow`: opción “Entrar como administrador SQL” (solo SQL Server), llamadas a `AuthenticateAsync` o `ValidateSqlAdminAsync`; después de login, establecer `IAuthContext` y decidir si mostrar “Cambiar contraseña” o ventana principal; en modo admin SQL, abrir vista restringida solo a “Gestión de usuarios” (la pantalla concreta se implementa en Fase 2). |

---

## 5. Orden sugerido de tareas (Fase 1)

1. Definir en Domain las entidades de identidad (Usuario, Rol, relación) y enums/valores necesarios.  
2. Añadir al DbContext los conjuntos y configuraciones EF; generar y aplicar migración.  
3. Implementar en Infrastructure: hashing de contraseñas (ej. PBKDF2 o Argon2), `IAuthService` y `IAuthContext`.  
4. Definir en Application los contratos y resultados (`AuthResult`, etc.) y registrar servicios.  
5. Implementar `ISqlServerAdminAuthService` en Infrastructure (validación con conexión temporal y comprobación de sysadmin).  
6. Adaptar `LoginWindow`: opción admin SQL (solo si Provider == SqlServer), flujo normal vs admin SQL, establecimiento de `IAuthContext`.  
7. Añadir pantalla/diálogo “Cambiar contraseña” y enlazarlo cuando `RequiereCambioContraseña == true`.  
8. Definir vista “solo gestión de usuarios” (menú restringido o pantalla vacía con título) para modo admin SQL; en Fase 2 se rellenará con la gestión real.

---

## 6. Notas de seguridad

- Contraseñas: nunca en texto plano; almacenar solo hash + salt (PBKDF2-HMAC-SHA256 o Argon2id).  
- SA: no guardar ni registrar en logs la contraseña de SA; usar conexión temporal y cerrarla tras las operaciones.  
- SQLite: el archivo de recuperación debe ser tratado como secreto; recomendación de ubicación segura y, si se quiere, cifrado del archivo con contraseña.  
- Sesión: en una app de escritorio, el “estado de sesión” (usuario actual / modo admin SQL) puede vivir en memoria; al cerrar la app se pierde y se exige de nuevo login.

---

## 7. Próximo paso

Una vez aprobado este plan, se puede ejecutar la **Fase 1** en el orden indicado; la **gestión de usuarios** (pantallas completas, reseteo desde SA, contraseña temporal y recuperación SQLite) se desarrollará en la **Fase 2** después.
