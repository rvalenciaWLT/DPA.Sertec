USE master;
GO

IF DB_ID('SertecDB') IS NOT NULL
BEGIN
    ALTER DATABASE SertecDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SertecDB;
END
GO

CREATE DATABASE SertecDB;
GO

USE SertecDB;
GO

CREATE TABLE dbo.TipoReparacion
(
    Id         INT            IDENTITY(1,1) NOT NULL,
    Nombre     NVARCHAR(100)  NOT NULL,
    PrecioBase DECIMAL(10, 2) NOT NULL,
    CONSTRAINT PK_TipoReparacion PRIMARY KEY (Id),
    CONSTRAINT UQ_TipoReparacion_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_TipoReparacion_PrecioBase CHECK (PrecioBase >= 0)
);
GO

CREATE TABLE dbo.Cliente
(
    Id       INT           IDENTITY(1,1) NOT NULL,
    Paterno  NVARCHAR(50)  NOT NULL,
    Materno  NVARCHAR(50)  NULL,
    Nombres  NVARCHAR(100) NOT NULL,
    Correo   NVARCHAR(150) NULL,
    Telefono NVARCHAR(20)  NULL,
    CONSTRAINT PK_Cliente PRIMARY KEY (Id)
);
GO

CREATE TABLE dbo.Equipo
(
    Id          INT          IDENTITY(1,1) NOT NULL,
    NumeroSerie NVARCHAR(50) NOT NULL,
    Marca       NVARCHAR(50) NOT NULL,
    Modelo      NVARCHAR(50) NOT NULL,
    Anio        INT          NULL,
    ClienteId   INT          NOT NULL,
    CONSTRAINT PK_Equipo PRIMARY KEY (Id),
    CONSTRAINT UQ_Equipo_NumeroSerie UNIQUE (NumeroSerie),
    CONSTRAINT FK_Equipo_Cliente FOREIGN KEY (ClienteId)
        REFERENCES dbo.Cliente (Id)
);
GO

CREATE TABLE dbo.OrdenReparacion
(
    Id                  INT            IDENTITY(1,1) NOT NULL,
    FechaIngreso        DATETIME2(0)   NOT NULL CONSTRAINT DF_OrdenReparacion_FechaIngreso DEFAULT (SYSDATETIME()),
    DescripcionProblema NVARCHAR(500)  NOT NULL,
    CostoEstimado       DECIMAL(10, 2) NOT NULL,
    Estado              NVARCHAR(20)   NOT NULL CONSTRAINT DF_OrdenReparacion_Estado DEFAULT ('Pendiente'),
    EquipoId            INT            NOT NULL,
    TipoReparacionId    INT            NOT NULL,
    CONSTRAINT PK_OrdenReparacion PRIMARY KEY (Id),
    CONSTRAINT FK_OrdenReparacion_Equipo FOREIGN KEY (EquipoId)
        REFERENCES dbo.Equipo (Id),
    CONSTRAINT FK_OrdenReparacion_TipoReparacion FOREIGN KEY (TipoReparacionId)
        REFERENCES dbo.TipoReparacion (Id),
    CONSTRAINT CK_OrdenReparacion_CostoEstimado CHECK (CostoEstimado >= 0),
    CONSTRAINT CK_OrdenReparacion_Estado CHECK (Estado IN ('Pendiente', 'EnProceso', 'Reparado', 'Entregado', 'Cancelado'))
);
GO

CREATE INDEX IX_Equipo_ClienteId          ON dbo.Equipo (ClienteId);
CREATE INDEX IX_OrdenReparacion_EquipoId  ON dbo.OrdenReparacion (EquipoId);
CREATE INDEX IX_OrdenReparacion_TipoRepId ON dbo.OrdenReparacion (TipoReparacionId);
GO

INSERT INTO dbo.TipoReparacion (Nombre, PrecioBase) VALUES
    (N'Cambio de pantalla',      250.00),
    (N'Cambio de batería',       120.00),
    (N'Limpieza de placa',        80.00),
    (N'Reemplazo de disco SSD',  300.00),
    (N'Diagnóstico general',      50.00);
GO

INSERT INTO dbo.Cliente (Paterno, Materno, Nombres, Correo, Telefono) VALUES
    (N'Valencia', N'Rojas',  N'Ricardo Antonio', N'rvalencia@correo.com', N'987654321'),
    (N'Quispe',   N'Mamani', N'Lucía Fernanda',  N'lquispe@correo.com',   N'912345678'),
    (N'Torres',   N'Núñez',  N'Carlos Eduardo',  N'ctorres@correo.com',   N'998877665'),
    (N'Ramírez',  NULL,      N'Ana María',       N'aramirez@correo.com',  N'955443322');
GO

INSERT INTO dbo.Equipo (NumeroSerie, Marca, Modelo, Anio, ClienteId) VALUES
    (N'SN-LP-00001', N'Lenovo',  N'ThinkPad T14',   2022, 1),
    (N'SN-CP-00002', N'Samsung', N'Galaxy S22',     2023, 1),
    (N'SN-LP-00003', N'HP',      N'Pavilion 15',    2021, 2),
    (N'SN-TV-00004', N'LG',      N'OLED C2 55',     2023, 3),
    (N'SN-CP-00005', N'Xiaomi',  N'Redmi Note 12',  2024, 4);
GO

INSERT INTO dbo.OrdenReparacion (FechaIngreso, DescripcionProblema, CostoEstimado, Estado, EquipoId, TipoReparacionId) VALUES
    ('2026-09-01T09:30:00', N'La pantalla no enciende, se ve una línea vertical.', 280.00, N'Pendiente', 1, 1),
    ('2026-09-03T11:00:00', N'La batería dura menos de 30 minutos.',               140.00, N'EnProceso', 2, 2),
    ('2026-09-10T15:45:00', N'Se apaga solo por sobrecalentamiento.',               95.00, N'Reparado',  3, 3),
    ('2026-09-15T08:20:00', N'No da imagen, solo se escucha el audio.',            320.00, N'Pendiente', 4, 5),
    ('2026-09-20T17:10:00', N'Almacenamiento lento, se solicita upgrade a SSD.',   350.00, N'Entregado', 5, 4);
GO

SELECT 'TipoReparacion' AS Tabla, COUNT(*) AS Filas FROM dbo.TipoReparacion
UNION ALL SELECT 'Cliente',         COUNT(*) FROM dbo.Cliente
UNION ALL SELECT 'Equipo',          COUNT(*) FROM dbo.Equipo
UNION ALL SELECT 'OrdenReparacion', COUNT(*) FROM dbo.OrdenReparacion;
GO
