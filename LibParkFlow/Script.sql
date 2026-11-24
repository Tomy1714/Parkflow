-- =====================================================================
-- SCRIPT DE CREACION Y CARGA INICIAL DE BASE DE DATOS SQL SERVER
-- SISTEMA PARKFLOW - GESTION DE PARQUEADERO Y LAVADERO 24/7
-- =====================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ParkFlow')
BEGIN
    CREATE DATABASE ParkFlow;
END
GO

USE ParkFlow;
GO

-- 1. TABLA ROLES
IF OBJECT_ID('Roles', 'U') IS NULL
BEGIN
    CREATE TABLE Roles (
        IdRol INT PRIMARY KEY IDENTITY(1,1),
        NombreRol NVARCHAR(50) NOT NULL,
        TipoRol NVARCHAR(30) NOT NULL,
        Descripcion NVARCHAR(200) NULL,
        Activo BIT NOT NULL DEFAULT 1
    );
END
GO

-- 2. TABLA USUARIOS
IF OBJECT_ID('Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE Usuarios (
        IdUsuario INT PRIMARY KEY IDENTITY(1,1),
        Documento NVARCHAR(20) NOT NULL UNIQUE,
        Nombre NVARCHAR(100) NOT NULL,
        Telefono NVARCHAR(20) NULL,
        Correo NVARCHAR(100) NULL,
        CodigoOperativo NVARCHAR(20) NOT NULL UNIQUE,
        Contrasena NVARCHAR(100) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1
    );
END
GO

-- 3. TABLA ASIGNACIONES DE ROL (TURNOS)
IF OBJECT_ID('AsignacionesRol', 'U') IS NULL
BEGIN
    CREATE TABLE AsignacionesRol (
        IdAsignacion INT PRIMARY KEY IDENTITY(1,1),
        IdUsuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(IdUsuario),
        IdRol INT NOT NULL FOREIGN KEY REFERENCES Roles(IdRol),
        HoraInicio TIME NOT NULL,
        HoraFin TIME NOT NULL,
        DiasSemana NVARCHAR(50) NOT NULL DEFAULT 'TODOS',
        Activo BIT NOT NULL DEFAULT 1
    );
END
GO

-- 4. TABLA VEHICULOS
IF OBJECT_ID('Vehiculos', 'U') IS NULL
BEGIN
    CREATE TABLE Vehiculos (
        Placa NVARCHAR(10) PRIMARY KEY,
        TipoVehiculo NVARCHAR(20) NOT NULL DEFAULT 'Automóvil',
        Marca NVARCHAR(50) NULL,
        Modelo NVARCHAR(50) NULL,
        IdUsuarioPropietario INT NULL FOREIGN KEY REFERENCES Usuarios(IdUsuario)
    );
END
GO

-- 5. TABLA CELDAS
IF OBJECT_ID('Celdas', 'U') IS NULL
BEGIN
    CREATE TABLE Celdas (
        NumeroCelda NVARCHAR(10) PRIMARY KEY,
        Zona NVARCHAR(5) NOT NULL,
        Estado NVARCHAR(20) NOT NULL DEFAULT 'DISPONIBLE', -- DISPONIBLE, OCUPADA, RESERVADA, MANTENIMIENTO
        TipoPermitido NVARCHAR(20) NOT NULL DEFAULT 'Mixto',
        RegistroActivoId INT NULL,
        ReservaActivaId INT NULL
    );
END
GO

-- 6. TABLA CAJAS
IF OBJECT_ID('Cajas', 'U') IS NULL
BEGIN
    CREATE TABLE Cajas (
        IdCaja INT PRIMARY KEY IDENTITY(1,1),
        IdUsuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(IdUsuario),
        FechaApertura DATETIME NOT NULL,
        FechaCierre DATETIME NULL,
        MontoInicial DECIMAL(18,2) NOT NULL DEFAULT 0,
        MontoFinal DECIMAL(18,2) NULL,
        TotalRecaudado DECIMAL(18,2) NOT NULL DEFAULT 0,
        EstadoCaja NVARCHAR(20) NOT NULL DEFAULT 'ABIERTA' -- ABIERTA, CERRADA
    );
END
GO

-- 7. TABLA REGISTROS DE PARQUEADERO
IF OBJECT_ID('RegistrosParqueadero', 'U') IS NULL
BEGIN
    CREATE TABLE RegistrosParqueadero (
        IdRegistro INT PRIMARY KEY IDENTITY(1,1),
        Placa NVARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Vehiculos(Placa),
        NumeroCelda NVARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Celdas(NumeroCelda),
        FechaHoraEntrada DATETIME NOT NULL,
        FechaHoraSalida DATETIME NULL,
        FechaHoraPagado DATETIME NULL,
        MinutosEstadia INT NULL,
        CostoParqueadero DECIMAL(18,2) NOT NULL DEFAULT 0,
        EstadoRegistro NVARCHAR(20) NOT NULL DEFAULT 'ACTIVO', -- ACTIVO, PAGADO, FINALIZADO
        IdReserva INT NULL
    );
END
GO

-- 8. TABLA RESERVAS
IF OBJECT_ID('Reservas', 'U') IS NULL
BEGIN
    CREATE TABLE Reservas (
        IdReserva INT PRIMARY KEY IDENTITY(1,1),
        Placa NVARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Vehiculos(Placa),
        NumeroCelda NVARCHAR(10) NOT NULL FOREIGN KEY REFERENCES Celdas(NumeroCelda),
        FechaHoraReserva DATETIME NOT NULL,
        TiempoToleranciaMin INT NOT NULL DEFAULT 15,
        EstadoReserva NVARCHAR(30) NOT NULL DEFAULT 'ACTIVA', -- ACTIVA, ATENDIDA, CANCELADA_INASISTENCIA, CANCELADA_MANUAL
        FechaHoraCancelacion DATETIME NULL,
        IdUsuario INT NULL FOREIGN KEY REFERENCES Usuarios(IdUsuario)
    );
END
GO

-- 9. TABLA SERVICIOS DE LAVADO
IF OBJECT_ID('ServiciosLavado', 'U') IS NULL
BEGIN
    CREATE TABLE ServiciosLavado (
        IdServicio INT PRIMARY KEY IDENTITY(1,1),
        IdRegistro INT NOT NULL FOREIGN KEY REFERENCES RegistrosParqueadero(IdRegistro),
        Placa NVARCHAR(10) NOT NULL,
        TipoLavado NVARCHAR(50) NOT NULL DEFAULT 'Lavado General',
        PrecioBase DECIMAL(18,2) NOT NULL DEFAULT 25000,
        EstadoLavado NVARCHAR(20) NOT NULL DEFAULT 'PENDIENTE', -- PENDIENTE, EN_LAVADO, LISTO
        FechaHoraSolicitud DATETIME NOT NULL,
        FechaHoraInicio DATETIME NULL,
        FechaHoraFin DATETIME NULL,
        Facturado BIT NOT NULL DEFAULT 0
    );
END
GO

-- 10. TABLA COMPROBANTES DE PAGO
IF OBJECT_ID('ComprobantesPago', 'U') IS NULL
BEGIN
    CREATE TABLE ComprobantesPago (
        IdFactura INT PRIMARY KEY IDENTITY(1,1),
        NumeroFactura NVARCHAR(20) NOT NULL UNIQUE,
        IdRegistro INT NOT NULL FOREIGN KEY REFERENCES RegistrosParqueadero(IdRegistro),
        IdCaja INT NULL FOREIGN KEY REFERENCES Cajas(IdCaja),
        FechaEmision DATETIME NOT NULL,
        ValorParqueadero DECIMAL(18,2) NOT NULL,
        ValorLavado DECIMAL(18,2) NOT NULL,
        TotalPagar DECIMAL(18,2) NOT NULL,
        MetodoPago NVARCHAR(20) NOT NULL, -- Efectivo, Transferencia, Tarjeta
        Canal NVARCHAR(20) NOT NULL, -- CAJA, EN_LINEA
        Referencia NVARCHAR(100) NULL,
        ValorRecibido DECIMAL(18,2) NULL,
        Cambio DECIMAL(18,2) NULL
    );
END
GO

-- 11. TABLA DETALLES DE SERVICIO
IF OBJECT_ID('DetallesServicio', 'U') IS NULL
BEGIN
    CREATE TABLE DetallesServicio (
        IdDetalle INT PRIMARY KEY IDENTITY(1,1),
        IdRegistro INT NOT NULL FOREIGN KEY REFERENCES RegistrosParqueadero(IdRegistro) ON DELETE NO ACTION,
        IdServicio INT NOT NULL FOREIGN KEY REFERENCES ServiciosLavado(IdServicio) ON DELETE NO ACTION,
        Cantidad INT NOT NULL DEFAULT 1,
        Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0
    );
END
GO

-- 12. TABLA AUDITORIAS DE RESERVA
IF OBJECT_ID('AuditoriasReserva', 'U') IS NULL
BEGIN
    CREATE TABLE AuditoriasReserva (
        IdAuditoria INT PRIMARY KEY IDENTITY(1,1),
        FechaHora DATETIME NOT NULL,
        Placa NVARCHAR(10) NOT NULL,
        NumeroCelda NVARCHAR(10) NOT NULL,
        Evento NVARCHAR(100) NOT NULL,
        Detalles NVARCHAR(255) NULL
    );
END
GO

-- =====================================================================
-- DATOS INICIALES (SEMILLAS)
-- =====================================================================

-- Roles
IF NOT EXISTS (SELECT 1 FROM Roles)
BEGIN
    INSERT INTO Roles (NombreRol, TipoRol, Descripcion) VALUES
    ('Administrador', 'ADMINISTRADOR', 'Control total y reportes'),
    ('Operador', 'OPERADOR', 'Registro de ingresos y mapa'),
    ('Cajero', 'CAJERO', 'Cobro, liquidación y arqueo'),
    ('Lavador', 'LAVADOR', 'Gestión de turnos de lavado'),
    ('Cliente', 'CLIENTE', 'Autogestión de reservas y pagos');
END
GO

-- Usuarios
IF NOT EXISTS (SELECT 1 FROM Usuarios)
BEGIN
    INSERT INTO Usuarios (Documento, Nombre, Telefono, Correo, CodigoOperativo, Contrasena) VALUES
    ('1001', 'Administrador General', '3001112233', 'admin@parkflow.com', 'AD-1001', 'parkflow2026'),
    ('2094', 'Carlos Operador', '3002223344', 'operador@parkflow.com', 'OP-2094', 'parkflow2026'),
    ('3310', 'Mariana Cajera', '3003334455', 'cajero@parkflow.com', 'CJ-3310', 'parkflow2026'),
    ('4125', 'Pedro Lavador', '3004445566', 'lavador@parkflow.com', 'LV-4125', 'parkflow2026'),
    ('5001', 'Juan Pérez (Cliente)', '3005556677', 'cliente@parkflow.com', 'CL-5001', 'parkflow2026');
END
GO

-- Celdas (Zona A: 12, Zona B: 10, Zona C: 8)
IF NOT EXISTS (SELECT 1 FROM Celdas)
BEGIN
    INSERT INTO Celdas (NumeroCelda, Zona, Estado) VALUES
    ('A-01','A','DISPONIBLE'),('A-02','A','OCUPADA'),('A-03','A','DISPONIBLE'),('A-04','A','DISPONIBLE'),
    ('A-05','A','RESERVADA'),('A-06','A','DISPONIBLE'),('A-07','A','OCUPADA'),('A-08','A','DISPONIBLE'),
    ('A-09','A','OCUPADA'),('A-10','A','MANTENIMIENTO'),('A-11','A','DISPONIBLE'),('A-12','A','DISPONIBLE'),
    ('B-01','B','OCUPADA'),('B-02','B','RESERVADA'),('B-03','B','OCUPADA'),('B-04','B','DISPONIBLE'),
    ('B-05','B','OCUPADA'),('B-06','B','DISPONIBLE'),('B-07','B','DISPONIBLE'),('B-08','B','MANTENIMIENTO'),
    ('B-09','B','DISPONIBLE'),('B-10','B','DISPONIBLE'),
    ('C-01','C','RESERVADA'),('C-02','C','DISPONIBLE'),('C-03','C','DISPONIBLE'),('C-04','C','OCUPADA'),
    ('C-05','C','DISPONIBLE'),('C-06','C','DISPONIBLE'),('C-07','C','DISPONIBLE'),('C-08','C','DISPONIBLE');
END
GO

-- Asignaciones de Rol (Turnos de Operación)
IF NOT EXISTS (SELECT 1 FROM AsignacionesRol)
BEGIN
    INSERT INTO AsignacionesRol (IdUsuario, IdRol, HoraInicio, HoraFin, DiasSemana, Activo)
    SELECT u.IdUsuario, r.IdRol, '00:00:00', '23:59:59', 'TODOS', 1
    FROM Usuarios u
    JOIN Roles r ON (
        (u.CodigoOperativo = 'AD-1001' AND r.TipoRol = 'ADMINISTRADOR') OR
        (u.CodigoOperativo = 'OP-2094' AND r.TipoRol = 'OPERADOR') OR
        (u.CodigoOperativo = 'CJ-3310' AND r.TipoRol = 'CAJERO') OR
        (u.CodigoOperativo = 'LV-4125' AND r.TipoRol = 'LAVADOR') OR
        (u.CodigoOperativo = 'CL-5001' AND r.TipoRol = 'CLIENTE')
    );
END
GO

-- Caja Activa Inicial
IF NOT EXISTS (SELECT 1 FROM Cajas)
BEGIN
    DECLARE @idCajero INT = (SELECT TOP 1 IdUsuario FROM Usuarios WHERE CodigoOperativo = 'CJ-3310');
    IF @idCajero IS NOT NULL
    BEGIN
        INSERT INTO Cajas (IdUsuario, FechaApertura, MontoInicial, TotalRecaudado, EstadoCaja)
        VALUES (@idCajero, GETDATE(), 500000.00, 0.00, 'ABIERTA');
    END
END
GO

use ParkFlow
SELECT * FROM Vehiculos;
