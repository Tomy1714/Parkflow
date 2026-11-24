using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public class RegistrosNegocio : IRegistrosNegocio
    {
        private readonly IConexion _conexion;

        public RegistrosNegocio(IConexion? conexion = null)
        {
            _conexion = conexion ?? new Conexion();
        }

        public List<RegistroParqueadero> Consultar()
        {
            return _conexion.RegistrosParqueadero!
                .Include(r => r.Vehiculo)
                .Include(r => r.Celda)
                .Include(r => r.ServiciosLavado)
                .OrderByDescending(r => r.IdRegistro)
                .ToList();
        }

        public List<RegistroParqueadero> ConsultarActivos()
        {
            return _conexion.RegistrosParqueadero!
                .Include(r => r.Vehiculo)
                .Include(r => r.Celda)
                .Include(r => r.ServiciosLavado)
                .Where(r => r.EstadoRegistro == EstadoRegistro.ACTIVO || r.EstadoRegistro == EstadoRegistro.PAGADO)
                .OrderByDescending(r => r.IdRegistro)
                .ToList();
        }

        public RegistroParqueadero? BuscarPorPlaca(string placa)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            return _conexion.RegistrosParqueadero!
                .Include(r => r.Vehiculo)
                .Include(r => r.Celda)
                .Include(r => r.ServiciosLavado)
                .OrderByDescending(r => r.IdRegistro)
                .FirstOrDefault(r => r.Placa == pNorm && r.EstadoRegistro != EstadoRegistro.FINALIZADO);
        }

        public TicketIngresoDto RegistrarIngreso(string placa, string tipoVehiculo, string? numeroCelda = null, DateTime? ahora = null, string? marca = null, string? modelo = null)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();

            // Criterio de Aceptación 3 (HU-01): Validación de formato de placa
            if (string.IsNullOrWhiteSpace(pNorm) || pNorm.Length < 6)
            {
                throw new ArgumentException("Por favor ingrese un número de placa válido antes de continuar");
            }

            // Validar si el vehículo ya tiene un ingreso activo
            var registroExistente = BuscarPorPlaca(pNorm);
            if (registroExistente != null)
            {
                throw new InvalidOperationException($"El vehículo con placa {pNorm} ya cuenta con un ingreso activo en la celda {registroExistente.NumeroCelda}.");
            }

            DateTime fechaEntrada = ahora ?? DateTime.Now;

            // Asegurar que el vehículo esté registrado y actualizar Marca / Modelo si vienen proporcionados
            var vehiculo = _conexion.Vehiculos!.FirstOrDefault(v => v.Placa == pNorm);
            if (vehiculo == null)
            {
                vehiculo = new Vehiculo
                {
                    Placa = pNorm,
                    TipoVehiculo = string.IsNullOrWhiteSpace(tipoVehiculo) ? "Automóvil" : tipoVehiculo,
                    Marca = string.IsNullOrWhiteSpace(marca) ? null : marca.Trim(),
                    Modelo = string.IsNullOrWhiteSpace(modelo) ? null : modelo.Trim()
                };
                _conexion.Vehiculos!.Add(vehiculo);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(tipoVehiculo))
                {
                    vehiculo.TipoVehiculo = tipoVehiculo;
                }
                if (!string.IsNullOrWhiteSpace(marca))
                {
                    vehiculo.Marca = marca.Trim();
                }
                if (!string.IsNullOrWhiteSpace(modelo))
                {
                    vehiculo.Modelo = modelo.Trim();
                }
            }

            string? mensajeReserva = null;
            string celdaFinal = numeroCelda ?? string.Empty;
            int? idReservaVinculada = null;

            // Criterio de Aceptación 4 (HU-01): Ingreso asociado a una reserva previa existente
            var reserva = _conexion.Reservas!
                .Include(r => r.Celda)
                .Where(r => r.Placa == pNorm && r.EstadoReserva == EstadoReserva.ACTIVA)
                .OrderByDescending(r => r.FechaHoraReserva)
                .FirstOrDefault();

            if (reserva != null && !reserva.EstaVencida(fechaEntrada))
            {
                celdaFinal = reserva.NumeroCelda;
                idReservaVinculada = reserva.IdReserva;
                reserva.ConfirmarIngreso();
                mensajeReserva = $"Ingreso vinculado a reserva activa en celda {celdaFinal}.";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(celdaFinal))
                {
                    // Asignación automática inteligente si no se especifica celda manual
                    var celdaLibre = _conexion.Celdas!
                        .Where(c => c.Estado == EstadoCelda.DISPONIBLE)
                        .OrderBy(c => c.NumeroCelda)
                        .FirstOrDefault();

                    if (celdaLibre == null)
                    {
                        throw new InvalidOperationException("No hay celdas disponibles en este momento en el parqueadero.");
                    }
                    celdaFinal = celdaLibre.NumeroCelda;
                }
            }

            var celdaObj = _conexion.Celdas!.FirstOrDefault(c => c.NumeroCelda == celdaFinal);
            if (celdaObj == null)
            {
                throw new KeyNotFoundException($"La celda {celdaFinal} no existe.");
            }

            // Criterio de Aceptación 2 (HU-01): Intento de asignación en celda no disponible
            if (celdaObj.Estado != EstadoCelda.DISPONIBLE && (idReservaVinculada == null || celdaObj.Estado != EstadoCelda.RESERVADA))
            {
                throw new InvalidOperationException("La celda seleccionada no está disponible");
            }

            var nuevoRegistro = new RegistroParqueadero
            {
                Placa = pNorm,
                NumeroCelda = celdaFinal,
                FechaHoraEntrada = fechaEntrada,
                EstadoRegistro = EstadoRegistro.ACTIVO,
                IdReserva = idReservaVinculada
            };

            _conexion.RegistrosParqueadero!.Add(nuevoRegistro);
            _conexion.SaveChanges(); // Para obtener IdRegistro

            celdaObj.Ocupar(nuevoRegistro.IdRegistro);
            _conexion.SaveChanges();

            return new TicketIngresoDto
            {
                IdRegistro = nuevoRegistro.IdRegistro,
                Placa = pNorm,
                NumeroCelda = celdaFinal,
                TipoVehiculo = vehiculo.TipoVehiculo,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo,
                FechaHoraEntrada = fechaEntrada,
                EstadoCelda = "Ocupada",
                MensajeReserva = mensajeReserva
            };
        }
    }
}
