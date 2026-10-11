# DPA.Sertec — Práctica Calificada 01

Web API en **ASP.NET Core 10** con enfoque **Database First** sobre **SQL Server**, para una empresa que brinda servicio técnico a equipos electrónicos.

**Curso:** Desarrollo de ambiente web — 2026-02
**Integrantes:** _(completar)_

---

## Dónde está cada pregunta

| Pregunta | Entregable | Ubicación |
|---|---|---|
| **P1** — Diseño de base de datos | Script de creación con tablas, PK, FK, restricciones y datos de prueba | [`DPA.Sertec.API/Database/SertecDB.sql`](DPA.Sertec.API/Database/SertecDB.sql) |
| **P2** — Estructura del proyecto | Web API + biblioteca de clases con `Core` e `Infrastructure` | `DPA.Sertec.API/` y `DPA.Sertec.Data/` |
| **P3** — Scaffolding con EF Core | Entidades y `DbContext` generados | `DPA.Sertec.Data/Infrastructure/Entities/` y `Infrastructure/Data/` |
| **P4** — Controlador autogenerado | `TipoReparacion` con inyección directa del `DbContext` | [`Controllers/TipoReparacionController.cs`](DPA.Sertec.API/Controllers/TipoReparacionController.cs) |
| **P5** — Controlador con Repository | `OrdenReparacion` con Repository + Service + interfaces | `DPA.Sertec.Data/Core/` y `Infrastructure/`, [`Controllers/OrdenReparacionController.cs`](DPA.Sertec.API/Controllers/OrdenReparacionController.cs) |
| **P6** — Pruebas con Postman | Colección con 23 requests y tests automáticos | [`DPA.Sertec.API/Pregunta6/`](DPA.Sertec.API/Pregunta6/) |
| **P7** — Buenas prácticas y reflexión | Respuestas a las preguntas a, b, c y d | [`DPA.Sertec.API/Pregunta7/`](DPA.Sertec.API/Pregunta7/) |

---

## Estructura de la solución

```
DPA.Sertec.slnx
├── DPA.Sertec.API/                     Capa de presentación (Web API)
│   ├── Controllers/
│   │   ├── TipoReparacionController.cs      P4 — DbContext directo
│   │   └── OrdenReparacionController.cs     P5 — solo depende de IOrdenReparacionService
│   ├── Database/SertecDB.sql                P1 — script de la base de datos
│   ├── Pregunta6/                           P6 — colección de Postman
│   ├── Pregunta7/                           P7 — documento de reflexión
│   └── Program.cs                           Registro del DbContext y de las dependencias
│
└── DPA.Sertec.Data/                    Biblioteca de clases
    ├── Core/                                Contratos puros, sin Entity Framework
    │   ├── Common/ServiceResult.cs              Resultado de negocio desacoplado de HTTP
    │   ├── DTOs/                                DTOs de lectura y de escritura
    │   └── Interfaces/                          IOrdenReparacionRepository, IOrdenReparacionService
    │
    └── Infrastructure/                      Implementaciones y acceso a datos
        ├── Data/SertecDbContext.cs              Generado por scaffolding
        ├── Entities/                            Generadas por scaffolding
        ├── Repositories/                        Acceso a datos con EF Core
        └── Services/                            Reglas de negocio y mapeo a DTOs
```

**Flujo de dependencias en la P5:**

```
Controller  →  IOrdenReparacionService  →  IOrdenReparacionRepository  →  SertecDbContext
 (API)            (Core)                        (Core)                     (Infrastructure)
                     ↑                             ↑
          OrdenReparacionService        OrdenReparacionRepository
             (Infrastructure)              (Infrastructure)
```

El controlador no conoce Entity Framework ni ninguna clase concreta: todo se resuelve por inyección de dependencias contra interfaces.

---

## Modelo de datos

```
Cliente (1) ──< (N) Equipo (1) ──< (N) OrdenReparacion (N) >── (1) TipoReparacion
```

| Tabla | Columnas |
|---|---|
| `TipoReparacion` | `Id` (PK, IDENTITY), `Nombre` (UNIQUE), `PrecioBase` |
| `Cliente` | `Id` (PK, IDENTITY), `Paterno`, `Materno`, `Nombres`, `Correo`, `Telefono` |
| `Equipo` | `Id` (PK, IDENTITY), `NumeroSerie` (UNIQUE), `Marca`, `Modelo`, `Anio`, `ClienteId` (FK) |
| `OrdenReparacion` | `Id` (PK, IDENTITY), `FechaIngreso`, `DescripcionProblema`, `CostoEstimado`, `Estado` (CHECK), `EquipoId` (FK), `TipoReparacionId` (FK) |

Todos los identificadores son **autogenerados** (`IDENTITY(1,1)`).

---

## Cómo ejecutar

### 1. Crear la base de datos

Ejecutar [`DPA.Sertec.API/Database/SertecDB.sql`](DPA.Sertec.API/Database/SertecDB.sql) en SQL Server (SSMS o `sqlcmd`). El script crea la base, las cuatro tablas con sus restricciones y carga datos de prueba.

### 2. Ajustar la cadena de conexión

En `DPA.Sertec.API/appsettings.json`, adaptar `ConnectionStrings:SertecDB` al nombre de tu instancia.

### 3. Levantar la API

```powershell
dotnet run --project DPA.Sertec.API
```

Queda disponible en **http://localhost:5030**.

### 4. Probar con Postman

Importar `DPA.Sertec.API/Pregunta6/DPA.Sertec.postman_collection.json` y ejecutar la colección de arriba hacia abajo (algunos requests guardan Ids que reutilizan los siguientes).

---

## Endpoints

### `TipoReparacion` — P4, DbContext directo

| Método | Ruta | Respuesta |
|---|---|---|
| `GET` | `/api/TipoReparacion` | `200` |
| `GET` | `/api/TipoReparacion/{id}` | `200` · `404` |
| `POST` | `/api/TipoReparacion` | `201` · `409` nombre duplicado |
| `PUT` | `/api/TipoReparacion/{id}` | `204` · `400` · `404` |
| `DELETE` | `/api/TipoReparacion/{id}` | `204` · `404` · `409` si tiene órdenes asociadas |

### `OrdenReparacion` — P5, Repository + Service

| Método | Ruta | Respuesta |
|---|---|---|
| `GET` | `/api/OrdenReparacion` | `200` |
| `GET` | `/api/OrdenReparacion/{id}` | `200` · `404` |
| `POST` | `/api/OrdenReparacion` | `201` · `400` validaciones y claves foráneas |
| `PUT` | `/api/OrdenReparacion/{id}` | `200` · `400` · `404` |
| `DELETE` | `/api/OrdenReparacion/{id}` | `204` · `404` · `409` si la orden ya fue entregada |

---

## Comando de scaffolding utilizado (P3)

```powershell
Scaffold-DbContext "Server=<servidor>\SQL2025;Database=SertecDB;User Id=sa;Password=***;TrustServerCertificate=True" `
  Microsoft.EntityFrameworkCore.SqlServer `
  -StartupProject DPA.Sertec.Data `
  -OutputDir Infrastructure\Entities `
  -ContextDir Infrastructure\Data `
  -Context SertecDbContext `
  -NoOnConfiguring -Force
```

`-NoOnConfiguring` evita que la cadena de conexión quede incrustada en el `DbContext` generado. Las dificultades encontradas durante este paso están documentadas en la [P7](DPA.Sertec.API/Pregunta7/Respuestas_Pregunta7.md).

---

## Entorno

- .NET SDK 10.0.401
- Entity Framework Core 10.0.12
- SQL Server 2025 Developer Edition
