using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public class ReservasNegocio : IReservasNegocio
    {
        private readonly IConexion _conexion;

        public ReservasNegocio(IConexion? conexion = null)
        {
            _conexion = conexion ?? new Conexion();
        }

        public List<Reserva> Consultar()
        {
            return _conexion.Reservas!
                .Include(r => r.Vehiculo)
                .Include(r => r.Celda)
                .OrderByDescending(r => r.IdReserva)
                .ToList();
        }

        public List<Reserva> ConsultarActivas()
        {
            return _conexion.Reservas!
                .Include(r => r.Vehiculo)
                .Include(r => r.Celda)
                .Where(r => r.EstadoReserva == EstadoReserva.ACTIVA)
                .OrderBy(r => r.FechaHoraReserva)
                .ToList();
        }

        public List<Reserva> ConsultarPorUsuario(int idUsuario)
        {
            return _conexion.Reservas!
                .Include(r => r.Vehiculo)
                .Include(r => r.Celda)
                .Where(r => r.IdUsuario == idUsuario)
                .OrderByDescending(r => r.IdReserva)
                .ToList();
        }

        public List<AuditoriaReserva> ConsultarHistorialAuditoria()
        {
            return _conexion.AuditoriasReserva!
                .OrderByDescending(a => a.FechaHora)
                .ToList();
        }

        public Reserva CrearReserva(string placa, string numeroCelda, DateTime horaReserva, int? idUsuario = null)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            var cNorm = (numeroCelda ?? "").Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(pNorm) || pNorm.Length < 6)
                throw new ArgumentException("Por favor ingrese un número de placa válido antes de continuar");

            var celda = _conexion.Celdas!.FirstOrDefault(c => c.NumeroCelda == cNorm);
            if (celda == null)
                throw new KeyNotFoundException($"La celda {cNorm} no existe.");

            // Regla RN-02: Asignación única
            if (celda.Estado != EstadoCelda.DISPONIBLE)
                throw new InvalidOperationException($"La celda {cNorm} no está disponible (Estado actual: {celda.Estado}).");

            // Asegurar vehículo
            var vehiculo = _conexion.Vehiculos!.FirstOrDefault(v => v.Placa == pNorm);
            if (vehiculo == null)
            {
                vehiculo = new Vehiculo
                {
                    Placa = pNorm,
                    TipoVehiculo = "Automóvil",
                    IdUsuarioPropietario = idUsuario
                };
                _conexion.Vehiculos!.Add(vehiculo);
            }

            var reserva = new Reserva
            {
                Placa = pNorm,
                NumeroCelda = cNorm,
                FechaHoraReserva = horaReserva,
                TiempoToleranciaMin = 15, // RN-03
                EstadoReserva = EstadoReserva.ACTIVA,
                IdUsuario = idUsuario
            };

            _conexion.Reservas!.Add(reserva);
            _conexion.SaveChanges();

            celda.Reservar(reserva.IdReserva);
            _conexion.SaveChanges();

            return reserva;
        }

        public void CancelarReserva(int idReserva, DateTime? ahora = null)
        {
            var reserva = _conexion.Reservas!
                .Include(r => r.Celda)
                .FirstOrDefault(r => r.IdReserva == idReserva);

            if (reserva == null)
                throw new KeyNotFoundException($"No se encontró la reserva #{idReserva}.");

            if (reserva.EstadoReserva != EstadoReserva.ACTIVA)
                throw new InvalidOperationException("Solo se pueden cancelar reservas en estado Activa.");

            var fecha = ahora ?? DateTime.Now;
            reserva.CancelarManual(fecha);

            if (reserva.Celda != null)
            {
                reserva.Celda.Liberar();
            }

            _conexion.AuditoriasReserva?.Add(new AuditoriaReserva
            {
                FechaHora = fecha,
                Placa = reserva.Placa,
                NumeroCelda = reserva.NumeroCelda,
                Evento = "Cancelada manualmente por Usuario",
                Detalles = $"Reserva #{reserva.IdReserva} cancelada."
            });

            _conexion.SaveChanges();
        }

        public bool EliminarReserva(int idReserva)
        {
            var reserva = _conexion.Reservas!
                .Include(r => r.Celda)
                .FirstOrDefault(r => r.IdReserva == idReserva);

            if (reserva == null)
                return false;

            // Si la celda asociada está reservada por esta reserva, liberarla
            if (reserva.Celda != null && reserva.Celda.Estado == EstadoCelda.RESERVADA)
            {
                reserva.Celda.Liberar();
            }

            _conexion.AuditoriasReserva?.Add(new AuditoriaReserva
            {
                FechaHora = DateTime.Now,
                Placa = reserva.Placa,
                NumeroCelda = reserva.NumeroCelda,
                Evento = "Eliminación física de Reserva",
                Detalles = $"Reserva #{reserva.IdReserva} eliminada de la base de datos."
            });

            _conexion.Reservas!.Remove(reserva);
            _conexion.SaveChanges();
            return true;
        }

        /// <summary>
        /// Aplica la regla RN-03 y HU-02: Cancela reservas cuya tolerancia de 15 minutos haya expirado sin ingreso
        /// </summary>
        public int VerificarVencimientosPorTolerancia(DateTime ahora)
        {
            var reservasActivas = _conexion.Reservas!
                .Include(r => r.Celda)
                .Where(r => r.EstadoReserva == EstadoReserva.ACTIVA)
                .ToList();

            int canceladas = 0;

            foreach (var r in reservasActivas)
            {
                if (r.EstaVencida(ahora))
                {
                    r.CancelarPorVencimiento(ahora);

                    if (r.Celda != null)
                    {
                        r.Celda.Liberar();
                    }

                    _conexion.AuditoriasReserva?.Add(new AuditoriaReserva
                    {
                        FechaHora = ahora,
                        Placa = r.Placa,
                        NumeroCelda = r.NumeroCelda,
                        Evento = "Cancelada automáticamente por Sistema",
                        Detalles = $"Reserva #{r.IdReserva} cancelada por inasistencia (tolerancia de 15 min superada)."
                    });

                    canceladas++;
                }
            }

            if (canceladas > 0)
            {
                _conexion.SaveChanges();
            }

            return canceladas;
        }
    }
}
