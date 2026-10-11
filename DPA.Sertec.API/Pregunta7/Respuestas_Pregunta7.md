# Pregunta 7 — Buenas prácticas y reflexión

**Curso:** Desarrollo de ambiente web
**Práctica Calificada 01 — 2026-02**
**Proyecto:** DPA.Sertec — Web API para una empresa de servicio técnico de equipos electrónicos
**Integrantes:** _(completar)_
**Fecha:** 11/10/2026

---

## a. ¿Qué dificultades tuviste durante el scaffolding y cómo las solucionaste?

### 1. El comando `Scaffold-DbContext` falló buscando el ensamblado en el proyecto equivocado

Al ejecutar `Scaffold-DbContext` desde la Consola del Administrador de paquetes con `DPA.Sertec.Data` como proyecto predeterminado, la compilación fue exitosa pero el comando abortó con:

```
Build succeeded.
File 'C:\...\DPA.Sertec.Data\bin\Debug\net10.0\DPA.Sertec.API.dll' not found.
```

Las herramientas de EF Core compilaban el proyecto de inicio (`DPA.Sertec.API`) pero buscaban su DLL dentro de la carpeta `bin` de la biblioteca de clases. El problema es de **resolución del proyecto de inicio**, agravado porque la solución usa el formato nuevo `.slnx`, que las herramientas de EF Core todavía no interpretan igual que el `.sln` clásico.

**Solución:** indicar explícitamente que el proyecto de inicio sea la propia biblioteca. Para generar el modelo no hace falta ejecutar la Web API, así que es perfectamente válido:

```powershell
# En la Consola del Administrador de paquetes
Scaffold-DbContext "..." Microsoft.EntityFrameworkCore.SqlServer -StartupProject DPA.Sertec.Data ...
```

Finalmente lo ejecutamos con la CLI equivalente, que resolvió sin problemas:

```powershell
dotnet ef dbcontext scaffold "Server=DESKTOP-CAAG7MO\SQL2025;Database=SertecDB;User Id=sa;Password=***;TrustServerCertificate=True" `
  Microsoft.EntityFrameworkCore.SqlServer `
  --output-dir Infrastructure/Entities `
  --context-dir Infrastructure/Data `
  --context SertecDbContext `
  --no-onconfiguring --force --startup-project .
```

### 2. Faltaban los paquetes NuGet en la biblioteca de clases

El scaffolding no corre sobre un proyecto "pelado". Hubo que instalar en `DPA.Sertec.Data`:

- `Microsoft.EntityFrameworkCore.SqlServer` — el proveedor de base de datos
- `Microsoft.EntityFrameworkCore.Tools` — el que habilita el comando `Scaffold-DbContext`

### 3. Error de certificado al conectarse a SQL Server

La instancia local usa un certificado autofirmado, por lo que la conexión fallaba hasta agregar `TrustServerCertificate=True` a la cadena.

### 4. Por defecto, la contraseña quedaba incrustada en el código generado

Sin opciones adicionales, EF Core genera un método `OnConfiguring` con la cadena de conexión *hardcodeada* y hasta emite una advertencia al respecto. Como el repositorio es público, lo resolvimos con el modificador **`--no-onconfiguring`**: el `DbContext` generado solo recibe `DbContextOptions` por constructor y la cadena vive únicamente en `appsettings.json`, inyectada desde `Program.cs`.

### 5. El pluralizador generó nombres en inglés

EF Core pluralizó las entidades según reglas del inglés, produciendo `DbSet<OrdenReparacion> OrdenReparacions` y `DbSet<TipoReparacion> TipoReparacions`. Es solo cosmético y funciona correctamente, por lo que decidimos **no editarlo**: es código autogenerado y cualquier cambio manual se perdería al volver a ejecutar el scaffolding.

### 6. Ciclos infinitos al serializar a JSON

El scaffolding genera navegaciones bidireccionales (`TipoReparacion → OrdenReparacions → TipoReparacion`). El primer `GET` sobre `TipoReparacion` fallaba con *"object cycle detected"*. Se resolvió configurando el serializador en `Program.cs`:

```csharp
options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
```

---

## b. ¿Qué diferencias notaste entre un controlador autogenerado y uno con repository? ¿Qué enfoque prefieres?

Implementamos los dos enfoques a propósito para poder compararlos:

| Aspecto | Autogenerado (`TipoReparacionController`) | Con Repository (`OrdenReparacionController`) |
|---|---|---|
| **Dependencias** | `SertecDbContext` (clase concreta de EF) | `IOrdenReparacionService` (interfaz) |
| **Dónde vive la lógica** | Dentro del propio controlador | Repartida: reglas en el *Service*, consultas en el *Repository* |
| **Qué devuelve** | La entidad de EF tal cual | Un DTO diseñado para el cliente |
| **Líneas de código** | ~115 en un solo archivo | ~330 repartidas en 7 archivos |
| **Velocidad de escritura** | Muy rápido | Más lento al inicio |
| **Testeabilidad** | Baja: para probarlo se necesita una base de datos | Alta: el *Service* se prueba con un *mock* de `IOrdenReparacionRepository` |
| **Costo del cambio** | Migrar de EF a Dapper obliga a reescribir el controlador | Solo se reescribe `OrdenReparacionRepository` |
| **Riesgo de over-posting** | Existe: el cliente postea la entidad completa | Nulo: el DTO solo expone los campos editables |
| **Acoplamiento a HTTP** | El acceso a datos convive con los códigos de estado | El *Service* devuelve `ServiceResult`, sin saber que existe HTTP |

**Diferencia que más se notó en la práctica:** el controlador autogenerado se escribe en minutos, pero cuando apareció la necesidad de validar que `EquipoId` y `TipoReparacionId` existieran antes de insertar, esa lógica no tenía dónde ir salvo dentro del propio controlador. En el enfoque con *Repository* la validación quedó naturalmente en `OrdenReparacionService.ValidarReferenciasAsync`, y el controlador quedó limitado a traducir el resultado a un código HTTP.

**Preferencia:** depende del tipo de entidad, y por eso usamos cada uno donde corresponde.

- Para **catálogos simples** (`TipoReparacion`: tres columnas, sin reglas de negocio) el patrón Repository es sobreingeniería. El DbContext directo es honesto y legible.
- Para **entidades con reglas de negocio** (`OrdenReparacion`: valida estados, verifica claves foráneas, impide borrar órdenes entregadas, relaciona tres tablas) preferimos el Repository. La inversión extra se paga sola en cuanto aparece la primera regla.

Si hubiera que elegir **un solo enfoque para un proyecto real que va a crecer**, elegimos el Repository con capa de servicio: el controlador autogenerado envejece mal porque mezcla responsabilidades y se vuelve imposible de testear sin base de datos.

---

## c. ¿Qué buenas prácticas implementaste (en código real) y por qué?

### Separación de responsabilidades

1. **DTOs separados de las entidades** — `Core/DTOs/`. El cliente nunca recibe entidades de EF. El `OrdenReparacionDto` aplana las relaciones (`equipoDescripcion`, `clienteNombreCompleto`, `tipoReparacionNombre`), de modo que la respuesta es útil en una sola llamada y no arrastra referencias cíclicas.

2. **DTOs distintos para leer y escribir** — `OrdenReparacionCreateDto` no expone `Id` (lo genera SQL Server con `IDENTITY`) ni propiedades de navegación. Esto previene **over-posting**: el cliente no puede inyectar campos que no le corresponden.

3. **`ServiceResult<T>` en lugar de códigos HTTP en el servicio** — `Core/Common/ServiceResult.cs`. La capa de negocio devuelve `Ok`/`NotFound`/`Invalid`/`Conflict` sin saber que detrás hay una Web API. El controlador es el único punto que traduce a HTTP. Así el servicio se puede reutilizar desde una app de escritorio o una tarea programada.

4. **Inyección contra la interfaz, no contra la clase concreta** — en `Program.cs` se registra `AddScoped<IOrdenReparacionRepository, OrdenReparacionRepository>()`. El controlador no conoce ninguna clase concreta de `Infrastructure`.

5. **`Scoped` como ciclo de vida** — una instancia por petición HTTP, igual que el `DbContext`. Usar `Singleton` habría provocado errores de concurrencia sobre el *change tracker*.

### Seguridad

6. **La cadena de conexión fuera del código** — gracias a `--no-onconfiguring`, vive solo en `appsettings.json` y se inyecta desde `Program.cs`, en lugar de quedar incrustada en el `DbContext` generado.

   Dicho esto, hay que ser honestos sobre el alcance de esta práctica: sacar la cadena del código **no la convierte en secreta**, porque `appsettings.json` sí se versiona y este repositorio es público. Es aceptable aquí porque se trata de una instancia local de desarrollo sin datos reales. En un entorno productivo el paso siguiente sería usar **User Secrets** en desarrollo (`dotnet user-secrets set`) y **variables de entorno o Azure Key Vault** en producción, dejando en `appsettings.json` únicamente la cadena vacía como plantilla. Lo ideal además sería autenticación integrada de Windows en vez del usuario `sa`, para no manejar contraseñas en absoluto.

7. **Validación de entrada con DataAnnotations** — `[Required]`, `[StringLength(500, MinimumLength = 5)]`, `[Range]`. Combinadas con `[ApiController]`, ASP.NET Core rechaza la petición con un `ValidationProblemDetails` **antes** de entrar al cuerpo de la acción.

### Integridad de datos

8. **Restricciones en la base de datos, no solo en la aplicación** — `UNIQUE` en `Equipo.NumeroSerie` y `TipoReparacion.Nombre`, `CHECK` sobre `Estado` y sobre los importes (`>= 0`), y las tres claves foráneas. La base de datos es la última línea de defensa: si mañana otro sistema escribe directamente, las reglas se siguen cumpliendo.

9. **Validación de claves foráneas antes de persistir** — `OrdenReparacionService.ValidarReferenciasAsync` comprueba que el equipo y el tipo de reparación existan. Sin esto, un `equipoId` inválido produciría un **500** con el error crudo de SQL Server; con esto produce un **400** con el mensaje *"No existe el equipo con Id 999"*.

10. **Captura de `DbUpdateException` → `409 Conflict`** — al intentar borrar un `TipoReparacion` referenciado por órdenes, se responde un mensaje entendible en vez de un 500.

11. **El catálogo de estados del servicio está alineado con `CK_OrdenReparacion_Estado`** — y está documentado en el código para que no se desincronicen.

### Rendimiento

12. **`AsNoTracking()` en todas las lecturas** — las consultas de solo lectura no necesitan el *change tracker*.

13. **`Include`/`ThenInclude` en una consulta base** — `OrdenReparacionRepository.BaseQuery()` trae orden, equipo, cliente y tipo en un solo viaje a la base de datos, evitando el problema **N+1**.

14. **Índices sobre las tres claves foráneas** — SQL Server crea índices automáticamente para las claves primarias y las restricciones `UNIQUE`, pero **no** para las foráneas; sin ellos, cada `JOIN` haría un recorrido completo de tabla.

15. **`async`/`await` en todo el acceso a datos** — libera el hilo mientras SQL Server responde.

### Corrección de la API

16. **Códigos de estado HTTP correctos** — `200` lectura, `201 Created` con cabecera `Location` (vía `CreatedAtAction`), `204 No Content` en borrados, `400` entrada inválida, `404` inexistente, `409` conflicto.

17. **Restricción de ruta `{id:int}`** — una petición a `/api/OrdenReparacion/abc` devuelve `404` en el enrutamiento, sin llegar al controlador.

18. **Consistencia entre lo que devuelve el POST y lo que devuelve el GET** — la columna `FechaIngreso` es `DATETIME2(0)`, así que SQL Server redondea a segundos. Como `DateTime.Now` incluye milisegundos, la respuesta del `POST` mostraba `12:23:32.9518123` pero un `GET` posterior devolvía `12:23:33`. Se añadió `TruncarASegundos()` en el servicio para que ambas respuestas coincidan exactamente.

### Un trade-off que asumimos conscientemente

El enunciado define `Core` como "interfaces y DTOs" e `Infrastructure` como "implementación y lógica de negocio". Por eso las **entidades generadas por el scaffolding quedaron en `Infrastructure/Entities`**, y como consecuencia `IOrdenReparacionRepository` (que está en `Core`) importa tipos de `Infrastructure`.

En una Clean Architecture estricta esa dependencia estaría invertida: las entidades de dominio vivirían en `Core` y nada de `Core` apuntaría hacia `Infrastructure`. Mantuvimos la estructura pedida por el enunciado y aislamos el impacto haciendo que **`IOrdenReparacionService` trabaje exclusivamente con DTOs** — así las entidades de EF nunca cruzan hacia la capa de presentación, que es lo que realmente importa.

---

## d. ¿Usaste herramientas de IA en esta práctica? ¿Cuáles y en qué parte del proyecto?

**Sí.** Se utilizó **Claude (Anthropic), a través de Claude Code integrado al flujo de trabajo**, como asistente durante toda la práctica.

### En qué partes se usó

| Parte del proyecto | Uso de la IA |
|---|---|
| **P1 — Base de datos** | Redacción del script `SertecDB.sql`, incluyendo la elección de tipos (`DECIMAL(10,2)` para importes), restricciones `UNIQUE`/`CHECK`, índices sobre las FK y los datos de prueba. |
| **P2 — Estructura** | Comandos `dotnet new classlib`, `dotnet sln add` y `dotnet add reference`, y la organización de carpetas `Core`/`Infrastructure`. |
| **P3 — Scaffolding** | **Diagnóstico del error** `DPA.Sertec.API.dll not found`: la IA identificó que era un problema de resolución del proyecto de inicio y propuso `-StartupProject`. También sugirió `--no-onconfiguring` para no dejar la contraseña en el código. |
| **P4 y P5 — Controladores** | Generación del código de controladores, repositorio, servicio, DTOs e interfaces. |
| **P6 — Postman** | Generación de la colección JSON con los 23 requests y sus tests automáticos. |
| **P7 — Este documento** | Redacción a partir de lo que realmente ocurrió durante el desarrollo. |

### Qué hicimos nosotros

- **Definimos el diseño y las decisiones**: nombre de la base de datos, motor y autenticación, estructura de la solución, y qué entidad usaría cada enfoque de controlador.
- **Revisamos todo el código antes de aceptarlo.** El trabajo se hizo paso a paso, revisando cada archivo en Visual Studio antes de pasar a la siguiente pregunta.
- **Ejecutamos el script SQL** y verificamos las tablas creadas.
- **Probamos manualmente los endpoints en Postman**, que es donde se confirmó que la API funcionaba de extremo a extremo.

### Nuestra valoración

La IA fue más útil como **par de diagnóstico** que como generadora de código. El momento de mayor valor fue el error del scaffolding: identificar que el problema era el proyecto de inicio (y no la cadena de conexión, que era la sospecha inicial) nos ahorró bastante tiempo de la práctica.

También detectó dos errores antes de que llegaran a ejecución: que `ActionResult<T>` no implementa `IActionResult` —lo que habría roto la compilación del `DELETE`— y la inconsistencia de milisegundos entre la respuesta del `POST` y la del `GET`.

El límite claro está en las decisiones de diseño: la IA propone alternativas, pero elegir entre `sa` y autenticación de Windows, o decidir qué entidad lleva Repository y cuál no, siguió siendo una decisión nuestra que había que justificar.
