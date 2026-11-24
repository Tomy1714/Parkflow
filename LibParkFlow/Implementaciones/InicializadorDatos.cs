using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public static class InicializadorDatos
    {
        public static void Inicializar(IConexion context)
        {
            // 1. Roles
            if (context.Roles != null && !context.Roles.Any())
            {
                var roles = new List<RolEntidad>
                {
                    new() { NombreRol = "Administrador", TipoRol = "ADMINISTRADOR", Descripcion = "Control total y reportes" },
                    new() { NombreRol = "Operador", TipoRol = "OPERADOR", Descripcion = "Registro de ingresos y mapa" },
                    new() { NombreRol = "Cajero", TipoRol = "CAJERO", Descripcion = "Cobro, liquidación y arqueo" },
                    new() { NombreRol = "Lavador", TipoRol = "LAVADOR", Descripcion = "Gestión de turnos de lavado" },
                    new() { NombreRol = "Cliente", TipoRol = "CLIENTE", Descripcion = "Autogestión de reservas y pagos" }
                };
                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            // 2. Usuarios
            if (context.Usuarios != null && !context.Usuarios.Any())
            {
                var usuarios = new List<Usuario>
                {
                    new() { Documento = "1001", Nombre = "Administrador General", CodigoOperativo = "AD-1001", Contrasena = "parkflow2026", Telefono = "3001112233" },
                    new() { Documento = "2094", Nombre = "Carlos Operador", CodigoOperativo = "OP-2094", Contrasena = "parkflow2026", Telefono = "3002223344" },
                    new() { Documento = "3310", Nombre = "Mariana Cajera", CodigoOperativo = "CJ-3310", Contrasena = "parkflow2026", Telefono = "3003334455" },
                    new() { Documento = "4125", Nombre = "Pedro Lavador", CodigoOperativo = "LV-4125", Contrasena = "parkflow2026", Telefono = "3004445566" },
                    new() { Documento = "5001", Nombre = "Juan Pérez (Cliente)", CodigoOperativo = "CL-5001", Contrasena = "parkflow2026", Telefono = "3005556677" }
                };
                context.Usuarios.AddRange(usuarios);
                context.SaveChanges();
            }

            // Asignaciones de Rol
            if (context.AsignacionesRol != null && !context.AsignacionesRol.Any())
            {
                var rolesList = context.Roles!.ToList();
                var allUsers = context.Usuarios!.ToList();
                var uAdmin = allUsers.FirstOrDefault(u => u.CodigoOperativo == "AD-1001" || u.Documento == "1001");
                var uOp = allUsers.FirstOrDefault(u => u.CodigoOperativo == "OP-2094" || u.Documento == "2094");
                var uCaj = allUsers.FirstOrDefault(u => u.CodigoOperativo == "CJ-3310" || u.Documento == "3310");
                var uLav = allUsers.FirstOrDefault(u => u.CodigoOperativo == "LV-4125" || u.Documento == "4125");
                var uCli = allUsers.FirstOrDefault(u => u.CodigoOperativo == "CL-5001" || u.Documento == "5001");

                var rAdmin = rolesList.FirstOrDefault(r => string.Equals(r.TipoRol, "ADMINISTRADOR", StringComparison.OrdinalIgnoreCase) || string.Equals(r.NombreRol, "Administrador", StringComparison.OrdinalIgnoreCase));
                var rOp = rolesList.FirstOrDefault(r => string.Equals(r.TipoRol, "OPERADOR", StringComparison.OrdinalIgnoreCase) || string.Equals(r.NombreRol, "Operador", StringComparison.OrdinalIgnoreCase));
                var rCaj = rolesList.FirstOrDefault(r => string.Equals(r.TipoRol, "CAJERO", StringComparison.OrdinalIgnoreCase) || string.Equals(r.NombreRol, "Cajero", StringComparison.OrdinalIgnoreCase));
                var rLav = rolesList.FirstOrDefault(r => string.Equals(r.TipoRol, "LAVADOR", StringComparison.OrdinalIgnoreCase) || string.Equals(r.NombreRol, "Lavador", StringComparison.OrdinalIgnoreCase));
                var rCli = rolesList.FirstOrDefault(r => string.Equals(r.TipoRol, "CLIENTE", StringComparison.OrdinalIgnoreCase) || string.Equals(r.NombreRol, "Cliente", StringComparison.OrdinalIgnoreCase));

                var asignaciones = new List<AsignacionRol>();
                if (uAdmin != null && rAdmin != null)
                    asignaciones.Add(new AsignacionRol { IdUsuario = uAdmin.IdUsuario, IdRol = rAdmin.IdRol, HoraInicio = TimeSpan.Zero, HoraFin = new TimeSpan(23, 59, 59), DiasSemana = "TODOS" });
                if (uOp != null && rOp != null)
                    asignaciones.Add(new AsignacionRol { IdUsuario = uOp.IdUsuario, IdRol = rOp.IdRol, HoraInicio = TimeSpan.Zero, HoraFin = new TimeSpan(23, 59, 59), DiasSemana = "TODOS" });
                if (uCaj != null && rCaj != null)
                    asignaciones.Add(new AsignacionRol { IdUsuario = uCaj.IdUsuario, IdRol = rCaj.IdRol, HoraInicio = TimeSpan.Zero, HoraFin = new TimeSpan(23, 59, 59), DiasSemana = "TODOS" });
                if (uLav != null && rLav != null)
                    asignaciones.Add(new AsignacionRol { IdUsuario = uLav.IdUsuario, IdRol = rLav.IdRol, HoraInicio = TimeSpan.Zero, HoraFin = new TimeSpan(23, 59, 59), DiasSemana = "TODOS" });
                if (uCli != null && rCli != null)
                    asignaciones.Add(new AsignacionRol { IdUsuario = uCli.IdUsuario, IdRol = rCli.IdRol, HoraInicio = TimeSpan.Zero, HoraFin = new TimeSpan(23, 59, 59), DiasSemana = "TODOS" });

                if (asignaciones.Any())
                {
                    context.AsignacionesRol.AddRange(asignaciones);
                    context.SaveChanges();
                }
            }

            // 3. Celdas (30 celdas: Zona A: 12, Zona B: 10, Zona C: 8)
            if (context.Celdas != null && !context.Celdas.Any())
            {
                var celdas = new List<Celda>();
                for (int i = 1; i <= 12; i++)
                    celdas.Add(new Celda { NumeroCelda = $"A-{i:D2}", Zona = "A", Estado = EstadoCelda.DISPONIBLE, TipoPermitido = "Mixto" });

                for (int i = 1; i <= 10; i++)
                    celdas.Add(new Celda { NumeroCelda = $"B-{i:D2}", Zona = "B", Estado = EstadoCelda.DISPONIBLE, TipoPermitido = "Mixto" });

                for (int i = 1; i <= 8; i++)
                    celdas.Add(new Celda { NumeroCelda = $"C-{i:D2}", Zona = "C", Estado = EstadoCelda.DISPONIBLE, TipoPermitido = "Mixto" });

                // Mantenimiento
                celdas.First(c => c.NumeroCelda == "A-10").Estado = EstadoCelda.MANTENIMIENTO;
                celdas.First(c => c.NumeroCelda == "B-08").Estado = EstadoCelda.MANTENIMIENTO;

                context.Celdas.AddRange(celdas);
                context.SaveChanges();
            }

            // 4. Vehículos iniciales
            var clienteUser = context.Usuarios?.FirstOrDefault(u => u.CodigoOperativo == "CL-5001");
            if (context.Vehiculos != null)
            {
                var vehiculos = new List<Vehiculo>
                {
                    new() { Placa = "ABC123", TipoVehiculo = "Automóvil", Marca = "Mazda", Modelo = "3", IdUsuarioPropietario = clienteUser?.IdUsuario },
                    new() { Placa = "XYZ789", TipoVehiculo = "Automóvil", Marca = "Chevrolet", Modelo = "Onix", IdUsuarioPropietario = clienteUser?.IdUsuario },
                    new() { Placa = "DEF456", TipoVehiculo = "Automóvil", Marca = "Toyota", Modelo = "Corolla" },
                    new() { Placa = "KTM456", TipoVehiculo = "Moto", Marca = "KTM", Modelo = "Duke 200" },
                    new() { Placa = "GHI789", TipoVehiculo = "Automóvil", Marca = "Renault", Modelo = "Sandero" },
                    new() { Placa = "JKL012", TipoVehiculo = "Automóvil", Marca = "Kia", Modelo = "Picanto" },
                    new() { Placa = "MNB321", TipoVehiculo = "Moto", Marca = "Yamaha", Modelo = "FZ" },
                    new() { Placa = "PLM908", TipoVehiculo = "Automóvil", Marca = "Nissan", Modelo = "Versa" },
                    new() { Placa = "RTY567", TipoVehiculo = "Automóvil", Marca = "Hyundai", Modelo = "Accent" },
                    new() { Placa = "PQR321", TipoVehiculo = "Automóvil", Marca = "Ford", Modelo = "Fiesta" },
                    new() { Placa = "MNO456", TipoVehiculo = "Automóvil", Marca = "Volkswagen", Modelo = "Gol" }
                };
                foreach (var v in vehiculos)
                {
                    if (!context.Vehiculos.Any(x => x.Placa == v.Placa))
                    {
                        context.Vehiculos.Add(v);
                    }
                }
                context.SaveChanges();
            }

            // 5. Caja inicial abierta
            var uCajero = context.Usuarios?.FirstOrDefault(u => u.CodigoOperativo == "CJ-3310");
            if (context.Cajas != null && !context.Cajas.Any() && uCajero != null)
            {
                var caja = new Caja
                {
                    IdUsuario = uCajero.IdUsuario,
                    FechaApertura = DateTime.Today.AddHours(8),
                    MontoInicial = 100000m,
                    TotalRecaudado = 0m,
                    EstadoCaja = EstadoCaja.ABIERTA
                };
                context.Cajas.Add(caja);
                context.SaveChanges();
            }

            // 6. Registros activos de parqueadero iniciales
            if (context.RegistrosParqueadero != null && !context.RegistrosParqueadero.Any())
            {
                var hoy = DateTime.Today;
                var ocupaciones = new List<(string placa, string celda, int h, int m)>
                {
                    ("DEF456", "A-02", 9, 10),
                    ("KTM456", "B-01", 8, 30),
                    ("GHI789", "B-03", 8, 50),
                    ("JKL012", "B-05", 9, 20),
                    ("MNB321", "A-07", 9, 30),
                    ("PLM908", "A-09", 8, 5),
                    ("RTY567", "C-04", 9, 0)
                };

                foreach (var (placa, celdaId, h, m) in ocupaciones)
                {
                    if (!context.Vehiculos!.Any(v => v.Placa == placa))
                    {
                        context.Vehiculos!.Add(new Vehiculo { Placa = placa, TipoVehiculo = "Automóvil" });
                        context.SaveChanges();
                    }

                    var r = new RegistroParqueadero
                    {
                        Placa = placa,
                        NumeroCelda = celdaId,
                        FechaHoraEntrada = hoy.AddHours(h).AddMinutes(m),
                        EstadoRegistro = EstadoRegistro.ACTIVO
                    };
                    context.RegistrosParqueadero.Add(r);
                    context.SaveChanges();

                    var celda = context.Celdas!.FirstOrDefault(c => c.NumeroCelda == celdaId);
                    if (celda != null)
                    {
                        celda.Ocupar(r.IdRegistro);
                    }
                }
                context.SaveChanges();
            }

            // 7. Reservas iniciales
            if (context.Reservas != null && !context.Reservas.Any())
            {
                var hoy = DateTime.Today;
                var reservas = new List<(string placa, string celda, int h, int m)>
                {
                    ("PQR321", "A-05", 10, 0),
                    ("XYZ789", "C-01", 10, 0),
                    ("MNO456", "B-02", 14, 0)
                };

                foreach (var (placa, celdaId, h, m) in reservas)
                {
                    if (!context.Vehiculos!.Any(v => v.Placa == placa))
                    {
                        context.Vehiculos!.Add(new Vehiculo { Placa = placa, TipoVehiculo = "Automóvil" });
                        context.SaveChanges();
                    }

                    var res = new Reserva
                    {
                        Placa = placa,
                        NumeroCelda = celdaId,
                        FechaHoraReserva = hoy.AddHours(h).AddMinutes(m),
                        TiempoToleranciaMin = 15,
                        EstadoReserva = EstadoReserva.ACTIVA,
                        IdUsuario = clienteUser?.IdUsuario
                    };
                    context.Reservas.Add(res);
                    context.SaveChanges();

                    var celda = context.Celdas!.FirstOrDefault(c => c.NumeroCelda == celdaId);
                    if (celda != null)
                    {
                        celda.Reservar(res.IdReserva);
                    }
                }
                context.SaveChanges();
            }

            // 8. Servicios de lavado iniciales
            if (context.ServiciosLavado != null && !context.ServiciosLavado.Any())
            {
                var hoy = DateTime.Today;
                var regGhi = context.RegistrosParqueadero?.FirstOrDefault(r => r.Placa == "GHI789");
                var regKtm = context.RegistrosParqueadero?.FirstOrDefault(r => r.Placa == "KTM456");
                var regDef = context.RegistrosParqueadero?.FirstOrDefault(r => r.Placa == "DEF456");

                if (regGhi != null)
                {
                    context.ServiciosLavado!.Add(new ServicioLavado
                    {
                        IdRegistro = regGhi.IdRegistro,
                        Placa = "GHI789",
                        TipoLavado = "Lavado General",
                        PrecioBase = 25000m,
                        EstadoLavado = EstadoLavado.LISTO,
                        FechaHoraSolicitud = hoy.AddHours(8).AddMinutes(55),
                        FechaHoraInicio = hoy.AddHours(9).AddMinutes(5),
                        FechaHoraFin = hoy.AddHours(9).AddMinutes(55),
                        Facturado = false
                    });
                }

                if (regKtm != null)
                {
                    context.ServiciosLavado!.Add(new ServicioLavado
                    {
                        IdRegistro = regKtm.IdRegistro,
                        Placa = "KTM456",
                        TipoLavado = "Lavado General",
                        PrecioBase = 25000m,
                        EstadoLavado = EstadoLavado.EN_LAVADO,
                        FechaHoraSolicitud = hoy.AddHours(8).AddMinutes(35),
                        FechaHoraInicio = hoy.AddHours(8).AddMinutes(45),
                        Facturado = false
                    });
                }

                if (regDef != null)
                {
                    context.ServiciosLavado!.Add(new ServicioLavado
                    {
                        IdRegistro = regDef.IdRegistro,
                        Placa = "DEF456",
                        TipoLavado = "Lavado General",
                        PrecioBase = 25000m,
                        EstadoLavado = EstadoLavado.PENDIENTE,
                        FechaHoraSolicitud = hoy.AddHours(9).AddMinutes(15),
                        Facturado = false
                    });
                }
                context.SaveChanges();
            }
        }
    }
}
