using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones;
using LibParkFlow.Implementaciones.Patrones.FacadeLiquidacion;
using LibParkFlow.Implementaciones.Patrones.FactoryMethodRoles;
using LibParkFlow.Implementaciones.Patrones.ObserverCeldas;
using LibParkFlow.Implementaciones.Patrones.StrategyPago;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

//clase Program

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("===============================================================");
Console.WriteLine("       PARKFLOW CENTRAL - BACKEND C# .NET / EF CORE / SQL      ");
Console.WriteLine("       Demostración de Arquitectura, Reglas y Patrones GoF     ");
Console.WriteLine("===============================================================\n");
Console.ResetColor();

Console.WriteLine($"[CADENA CONFIGURADA]: {LibParkFlow.Nucleo.Configuraciones.CadenaConexionPorDefecto}");
try
{
    using var testConn = new Microsoft.Data.SqlClient.SqlConnection(LibParkFlow.Nucleo.Configuraciones.CadenaConexionPorDefecto);
    testConn.Open();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"[CONEXION SQL]: Conexión exitosa a {testConn.DataSource} (BD: {testConn.Database})");
    Console.ResetColor();

    using var cmd = testConn.CreateCommand();
    cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
    var count = cmd.ExecuteScalar();
    Console.WriteLine($"[TABLAS ENCONTRADAS]: {count}");

    // Si AsignacionesRol está vacía en DEV1, sembramos las asignaciones de turno
    cmd.CommandText = "SELECT COUNT(*) FROM AsignacionesRol";
    int asignacionesCount = Convert.ToInt32(cmd.ExecuteScalar());
    if (asignacionesCount == 0)
    {
        cmd.CommandText = @"
            INSERT INTO AsignacionesRol (IdUsuario, IdRol, HoraInicio, HoraFin, DiasSemana, Activo)
            SELECT u.IdUsuario, r.IdRol, '00:00:00', '23:59:59', 'TODOS', 1
            FROM Usuarios u
            JOIN Roles r ON (
                (u.CodigoOperativo = 'AD-1001' AND r.TipoRol = 'ADMINISTRADOR') OR
                (u.CodigoOperativo = 'OP-2094' AND r.TipoRol = 'OPERADOR') OR
                (u.CodigoOperativo = 'CJ-3310' AND r.TipoRol = 'CAJERO') OR
                (u.CodigoOperativo = 'LV-4125' AND r.TipoRol = 'LAVADOR') OR
                (u.CodigoOperativo = 'CL-5001' AND r.TipoRol = 'CLIENTE')
            );";
        int insertados = cmd.ExecuteNonQuery();
        Console.WriteLine($"[ASIGNACIONES ROL DEV1]: Se sembraron {insertados} asignaciones de turno de usuario en SQL Server.");
    }

    Console.WriteLine("\n[CONTEO DE REGISTROS POR TABLA EN SQL SERVER DEV1]:");
    string[] tablas = new[] { "Usuarios", "Roles", "AsignacionesRol", "Vehiculos", "Celdas", "RegistrosParqueadero", "Reservas", "AuditoriasReserva", "ServiciosLavado", "Cajas", "ComprobantesPago" };
    foreach (var tabla in tablas)
    {
        cmd.CommandText = $"SELECT COUNT(*) FROM [{tabla}]";
        var cnt = cmd.ExecuteScalar();
        Console.WriteLine($"   * {tabla.PadRight(22)}: {cnt} registros");
    }

    cmd.CommandText = "SELECT Placa, TipoVehiculo, Marca, Modelo FROM Vehiculos";
    using var reader = cmd.ExecuteReader();
    Console.WriteLine("[VEHICULOS ACTUALES EN LA BD SQL SERVER DEV1]:");
    int totalVehiculos = 0;
    while (reader.Read())
    {
        totalVehiculos++;
        Console.WriteLine($"   * Placa: {reader["Placa"]} | Tipo: {reader["TipoVehiculo"]} | Marca: {reader["Marca"]} | Modelo: {reader["Modelo"]}");
    }
    if (totalVehiculos == 0) Console.WriteLine("   (La tabla Vehiculos está vacía actualmente en DEV1)");
    reader.Close();

    cmd.CommandText = "SELECT IdRegistro, Placa, NumeroCelda, FechaHoraEntrada, EstadoRegistro FROM RegistrosParqueadero ORDER BY IdRegistro DESC";
    using var rReg = cmd.ExecuteReader();
    Console.WriteLine("\n[REGISTROS DE PARQUEADERO EN SQL SERVER DEV1]:");
    int totalReg = 0;
    while (rReg.Read())
    {
        totalReg++;
        Console.WriteLine($"   * Ticket #{rReg["IdRegistro"]} | Placa: {rReg["Placa"]} | Celda: {rReg["NumeroCelda"]} | Entrada: {rReg["FechaHoraEntrada"]} | Estado: {rReg["EstadoRegistro"]}");
    }
    if (totalReg == 0) Console.WriteLine("   (No hay registros de parqueadero aún)");
    rReg.Close();

    cmd.CommandText = "SELECT NumeroCelda, Zona, Estado, RegistroActivoId FROM Celdas WHERE Estado = 'OCUPADA'";
    using var rCel = cmd.ExecuteReader();
    Console.WriteLine("\n[CELDAS OCUPADAS EN SQL SERVER DEV1]:");
    int totalCeldasOcupadas = 0;
    while (rCel.Read())
    {
        totalCeldasOcupadas++;
        Console.WriteLine($"   * Celda {rCel["NumeroCelda"]} (Zona {rCel["Zona"]}) -> Estado: {rCel["Estado"]} (RegistroActivo: {rCel["RegistroActivoId"]})");
    }
    if (totalCeldasOcupadas == 0) Console.WriteLine("   (No hay celdas ocupadas actualmente)");
    rCel.Close();
    testConn.Close();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[ERROR CONEXION SQL]: {ex.Message}");
    Console.ResetColor();
}
Console.WriteLine();

// Inicialización de contexto DbContext en memoria para la consola de demostración
var options = new DbContextOptionsBuilder<Conexion>()
    .UseInMemoryDatabase("ParkFlow_Consola_Db")
    .Options;

using var context = new Conexion(options);
context.Database.EnsureCreated();
InicializadorDatos.Inicializar(context);

// Instanciación de servicios de negocio
IUsuariosNegocio usuariosNegocio = new UsuariosNegocio(context);
ICeldasNegocio celdasNegocio = new CeldasNegocio(context);
IRegistrosNegocio registrosNegocio = new RegistrosNegocio(context);
IReservasNegocio reservasNegocio = new ReservasNegocio(context);
IServiciosLavadoNegocio lavadoNegocio = new ServiciosLavadoNegocio(context);
ICajasNegocio cajasNegocio = new CajasNegocio(context);
ILiquidacionFacade liquidacionFacade = new LiquidacionFacade(context);

DateTime relojSimulado = DateTime.Today.AddHours(9).AddMinutes(42);

Console.WriteLine($"[RELOJ SIMULADO INICIAL]: {relojSimulado:HH:mm:ss}\n");

// -------------------------------------------------------------
// 1. PATRÓN FACTORY METHOD - Autenticación y roles según turno
// -------------------------------------------------------------
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("1. PRUEBA PATRÓN FACTORY METHOD (RNF-04 - Autenticación por roles)");
Console.ResetColor();

var loginOp = usuariosNegocio.IniciarSesion("OP-2094", "parkflow2026", relojSimulado);
Console.WriteLine($"-> Login OP-2094: {loginOp.Mensaje}. Rol creado: {loginOp.RolActivo?.Nombre} ({loginOp.RolActivo?.Tipo})");
Console.WriteLine($"   Operaciones permitidas: {string.Join(", ", loginOp.OperacionesPermitidas.Take(4))}...");

var loginCli = usuariosNegocio.IniciarSesion("CL-5001", "parkflow2026", relojSimulado);
Console.WriteLine($"-> Login CL-5001: {loginCli.Mensaje}. Rol creado: {loginCli.RolActivo?.Nombre} ({loginCli.RolActivo?.Tipo})\n");

// -------------------------------------------------------------
// 2. HU-01: Registro de Ingreso de Vehículo
// -------------------------------------------------------------
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("2. PRUEBA HU-01: Registro de Ingreso de Vehículo (Camino Feliz & Reservas)");
Console.ResetColor();

try
{
    // Ingreso de ABC123 en A-12
    var ticket = registrosNegocio.RegistrarIngreso("ABC123", "Automóvil", "A-12", relojSimulado);
    Console.WriteLine($"-> [EXITOSO] Ticket #{ticket.IdRegistro}: Placa {ticket.Placa} asignada a celda {ticket.NumeroCelda} a las {ticket.FechaHoraEntrada:HH:mm}. Estado celda: {ticket.EstadoCelda}");
}
catch (Exception ex)
{
    Console.WriteLine($"-> [ERROR]: {ex.Message}");
}

// Validación de placa corta
try
{
    Console.Write("-> Probando placa inválida 'AB': ");
    registrosNegocio.RegistrarIngreso("AB", "Automóvil", "A-01", relojSimulado);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Correctamente rechazada: \"{ex.Message}\"");
    Console.ResetColor();
}

// Validación de celda no disponible
try
{
    Console.Write("-> Probando asignación de celda 'B-05' (Ocupada): ");
    registrosNegocio.RegistrarIngreso("NEW123", "Automóvil", "B-05", relojSimulado);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Correctamente rechazada: \"{ex.Message}\"");
    Console.ResetColor();
}
Console.WriteLine();

// -------------------------------------------------------------
// 3. HU-02: Control de Vencimiento de Reservas (RN-03 Tolerancia 15 min)
// -------------------------------------------------------------
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("3. PRUEBA HU-02: Control de Vencimiento de Reserva (RN-03)");
Console.ResetColor();

var activasAntes = reservasNegocio.ConsultarActivas();
Console.WriteLine($"-> Reservas activas antes del avance del reloj: {activasAntes.Count}");
foreach (var r in activasAntes)
{
    Console.WriteLine($"   Reserva #{r.IdReserva}: Placa {r.Placa} en Celda {r.NumeroCelda} programada para {r.FechaHoraReserva:HH:mm}");
}

// Simulamos avance del tiempo a las 10:20 (Reserva PQR321 y XYZ789 eran a las 10:00 -> >15 min expiradas)
relojSimulado = relojSimulado.AddMinutes(40); // 10:22 AM
Console.WriteLine($"-> Reloj simulado avanzado a: {relojSimulado:HH:mm:ss}");

int canceladas = reservasNegocio.VerificarVencimientosPorTolerancia(relojSimulado);
Console.WriteLine($"-> Reservas canceladas automáticamente por inasistencia: {canceladas}");

var celdaA05 = celdasNegocio.BuscarPorNumero("A-05");
Console.WriteLine($"-> Celda A-05 liberada automáticamente. Estado actual: {celdaA05?.Estado}\n");

// -------------------------------------------------------------
// 4. HU-03: Control de Estado de Lavado (Patrón GoF State y RN-05)
// -------------------------------------------------------------
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("4. PRUEBA HU-03 & PATRÓN STATE: Control del Lavado (RN-05)");
Console.ResetColor();

// Intento de solicitar lavado para vehículo sin parqueadero activo
try
{
    Console.Write("-> Probando solicitud de lavado para 'ZZZ999' (sin ingreso): ");
    lavadoNegocio.SolicitarLavado("ZZZ999", "Lavado General", 25000, relojSimulado);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Correctamente rechazada: \"{ex.Message}\"");
    Console.ResetColor();
}

// Solicitar lavado para ABC123 que está en parqueadero
var lavadoAbc = lavadoNegocio.SolicitarLavado("ABC123", "Lavado General", 25000, relojSimulado);
Console.WriteLine($"-> Lavado creado para ABC123: Id #{lavadoAbc.IdServicio}, Estado: {lavadoAbc.EstadoLavado}");

// Avanzar con State pattern: Pendiente -> En Lavado
lavadoAbc = lavadoNegocio.AvanzarEstado(lavadoAbc.IdServicio, relojSimulado.AddMinutes(5));
Console.WriteLine($"-> Estado avanzado 1: {lavadoAbc.EstadoLavado} (Inicio: {lavadoAbc.FechaHoraInicio:HH:mm})");

// Avanzar con State pattern: En Lavado -> Listo
lavadoAbc = lavadoNegocio.AvanzarEstado(lavadoAbc.IdServicio, relojSimulado.AddMinutes(35));
Console.WriteLine($"-> Estado avanzado 2: {lavadoAbc.EstadoLavado} (Fin: {lavadoAbc.FechaHoraFin:HH:mm}, PuedeCobrarse: {lavadoAbc.PuedeCobrarse()})\n");

// -------------------------------------------------------------
// 5. HU-04: Liquidación, Cobro y Salida (Facade + Strategy + RN-01 + RN-04)
// -------------------------------------------------------------
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("5. PRUEBA HU-04 & PATRONES FACADE / STRATEGY: Liquidación y Cobro Consolidado");
Console.ResetColor();

// Reloj simulado avanzado 2 horas para ABC123
relojSimulado = relojSimulado.AddHours(2);
Console.WriteLine($"-> Reloj simulado para liquidación: {relojSimulado:HH:mm:ss}");

var (reg, parqueadero, facturables, enCurso, total) = liquidacionFacade.ConsultarCuenta("ABC123", relojSimulado);
Console.WriteLine($"-> Cuenta de ABC123: Estadía: {reg.CalcularTiempoEstadia(relojSimulado)} min.");
Console.WriteLine($"   Valor Parqueadero (RN-01): ${parqueadero:N0}");
Console.WriteLine($"   Lavados en estado Listo: {facturables.Count} (${facturables.Sum(l => l.PrecioBase):N0})");
Console.WriteLine($"   TOTAL CONSOLIDADO: ${total:N0}");

// Intentar autorizar salida sin haber pagado (RN-04)
try
{
    Console.Write("-> Probando autorizar salida sin pagar: ");
    liquidacionFacade.AutorizarSalida("ABC123", relojSimulado);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Correctamente bloqueada (RN-04): \"{ex.Message}\"");
    Console.ResetColor();
}

// Pago usando Strategy (PagoEfectivo)
Console.WriteLine("-> Pagando con Estrategia 'PagoEfectivo' (Monto entregado: $35.000):");
var estrategiaEfectivo = new PagoEfectivo();
var comprobante = liquidacionFacade.Liquidar("ABC123", estrategiaEfectivo, new DatosPago { ValorRecibido = 35000 }, CanalPago.CAJA, ahora: relojSimulado);

Console.WriteLine($"   [FACTURADO]: {comprobante.NumeroFactura} por ${comprobante.TotalPagar:N0}. Método: {comprobante.MetodoPago}. Cambio: ${comprobante.Cambio:N0}");

var celdaAbc = celdasNegocio.BuscarPorNumero("A-12");
Console.WriteLine($"   Nota de diseño: Tras el pago, la celda A-12 sigue: {celdaAbc?.Estado}");

// Autorizar salida ahora que está pagada
Console.WriteLine("-> Autorizando salida del vehículo...");
liquidacionFacade.AutorizarSalida("ABC123", relojSimulado.AddMinutes(5));

celdaAbc = celdasNegocio.BuscarPorNumero("A-12");
Console.WriteLine($"   [SALIDA EXITOSA]: Celda A-12 liberada. Estado actual: {celdaAbc?.Estado}\n");

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("===============================================================");
Console.WriteLine("       DEMOSTRACION FINALIZADA - TODOS LOS COMPONENTES OK      ");
Console.WriteLine("===============================================================");
Console.ResetColor();


// fin de la clase program