# PARKFLOW CENTRAL — BACKEND C#, ENTITY FRAMEWORK CORE & SQL SERVER

Sistema integral de gestión operativa y administrativa para parqueadero y lavadero de vehículos 24/7.
Desarrollado bajo la plataforma **.NET 10 / C#**, **Entity Framework Core**, **SQL Server** y **ASP.NET Core (Web API y Razor Pages)**, tomando como base la arquitectura multicapa de referencia del proyecto `CRC`.

---

## 1. Aviso de Adaptación Tecnológica y Lenguajes

En los documentos iniciales de diseño y diagramas C4 (Entregable 2, Nivel 2 y 3, así como en las plantillas de principios SOLID y patrones GoF), el prototipo original del frontend estaba concebido como una maqueta en **HTML5, CSS3 y JavaScript puro (ES2020)** sin backend, cuyos datos residían en un objeto global volátil en memoria.

En cumplimiento de los requerimientos especificados:
- Se reemplazó la lógica simulada en JavaScript por una solución empresarial completa en **C# / .NET 10**.
- La persistencia de datos se trasladó a **Entity Framework Core con SQL Server** (`Microsoft.EntityFrameworkCore.SqlServer`), incluyendo scripts DDL estructurados (`Script.sql`) y soporte para inicialización en memoria para entornos de prueba.
- Se implementó tanto una **API REST (ASP.NET Core Web API)** como una aplicación web con **Razor Pages** (`ParkFlow_ASPNETCORE`), estructurada idénticamente a la solución `CRC`.

---

## 2. Estructura de la Solución (Inspirada en `CRC`)

La solución `ProyectoParkFlow.slnx` contiene 6 proyectos organizados por responsabilidades:

```
ProyectoParkFlow/
├── LibParkFlow/                      # Biblioteca central de dominio, persistencia y reglas
│   ├── Entidades/                    # Modelos POCO: Usuario, Rol, Vehiculo, Celda, Registro, Reserva, etc.
│   ├── Interfaces/                   # Contratos de servicios y patrones GoF
│   ├── Implementaciones/             # Lógica de negocio (Negocio.cs) y DbContext (Conexion.cs)
│   │   └── Patrones/                 # Implementación de los 5 patrones GoF
│   ├── Nucleo/                       # Configuraciones y cadenas de conexión
│   └── Script.sql                    # Script DDL SQL Server completo con datos semilla
├── ParkFlow.Api/                     # API REST ASP.NET Core con controladores OpenAPI/Swagger
│   ├── Controllers/                  # Autenticacion, Ingreso, Reservas, Celdas, Lavado, Liquidacion, Cajas, Vehiculos
│   └── Program.cs                    # Inyección de dependencias, CORS y DbContext
├── ParkFlow_ASPNETCORE/              # Aplicación Web ASP.NET Core Razor Pages
│   ├── Pages/                        # Páginas Razor (Index, Shared, Layout)
│   └── wwwroot/                      # Estilos (CSS) y scripts del frontend integrado
├── LibPresentaciones/                # Librería de comunicación HTTP (HttpClient)
│   ├── InterfacesPresentaciones/     # IComunicaciones
│   └── ImplementacionesPresentaciones/# Comunicaciones.cs
├── ConsolaParkFlow/                  # Aplicación de consola interactiva para demostración y verificación
└── PruebasUnitarias/                 # Suite de 15 pruebas unitarias automatizadas (MSTest)
```

---

## 3. Implementación de los 5 Patrones de Diseño GoF

| Nº | Patrón | Categoría | Módulo | Propósito y Aplicación |
|---|---|---|---|---|
| 1 | **Factory Method** | Creacional | `Autenticación y Roles` | Instancia el rol concreto (`RolAdministrador`, `RolOperador`, `RolCajero`, `RolLavador`, `RolCliente`) según el turno (`AsignacionRol`) sin condicionales dispersos. |
| 2 | **Observer** | Comportamiento | `Mapa de Celdas` | `Celda` notifica a observadores (`MapaCeldasObservador`, `ContadoresOcupacionObservador`) cuando cambia su estado (`Ocupar`, `Liberar`, `Reservar`, `Mantenimiento`). |
| 3 | **State** | Comportamiento | `Control de Lavado` | Modela los estados del lavado (`EstadoPendiente`, `EstadoEnLavado`, `EstadoListo`) asegurando transiciones secuenciales y habilitando el cobro únicamente en `Listo`. |
| 4 | **Strategy** | Comportamiento | `Liquidación y Cobro` | Procesa pagos intercambiables (`PagoEfectivo` calcula cambio, `PagoTransferencia` valida referencia, `PagoTarjeta` autoriza pasarela). |
| 5 | **Facade** | Estructural | `Liquidación, Cobro y Salida` | `LiquidacionFacade` orquesta transaccionalmente el cálculo de estadía (RN-01), suma de lavados listos, cobro con estrategia, factura, movimiento en caja y liberación de celda al salir (RN-04). |

---

## 4. Reglas de Negocio Implementadas

- **RN-01 (Cálculo por Fracción/Hora):** Tarifa base de \$3.000 por hora. Fracciones de 15 minutos (\$750). Si transcurren 5 minutos o más de una fracción, se liquida como fracción completa.
- **RN-02 (Asignación Única):** Ninguna celda en estado `OCUPADA` o `RESERVADA` puede ser asignada a otro vehículo simultáneamente.
- **RN-03 (Tolerancia de Reserva):** Si el vehículo no ingresa dentro de los 15 minutos posteriores a la hora fijada, el sistema cancela la reserva por inasistencia (`CANCELADA_INASISTENCIA`), libera la celda y genera registro de auditoría.
- **RN-04 (Requisito de Cobro para Salida):** No se autoriza la salida ni se libera la celda hasta que la cuenta esté totalmente pagada. La celda permanece ocupada tras pagar y se libera al momento de la salida física.
- **RN-05 (Servicio de Lavado Asociado):** No se puede registrar un servicio de lavado para un vehículo que no cuente con un ingreso de parqueadero activo en el establecimiento.

---

## 5. Instrucciones de Ejecución

### Ejecutar las Pruebas Unitarias:
```bash
dotnet test ProyectoParkFlow/PruebasUnitarias/PruebasUnitarias.csproj
```

### Ejecutar la Demostración en Consola:
```bash
dotnet run --project ProyectoParkFlow/ConsolaParkFlow/ConsolaParkFlow.csproj
```

### Ejecutar la API REST:
```bash
dotnet run --project ProyectoParkFlow/ParkFlow.Api/ParkFlow.Api.csproj
```

### Ejecutar la Aplicación Web (Razor Pages):
```bash
dotnet run --project ProyectoParkFlow/ParkFlow_ASPNETCORE/ParkFlow_ASPNETCORE.csproj
```
