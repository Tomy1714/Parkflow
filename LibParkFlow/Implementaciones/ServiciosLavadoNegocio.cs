using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public class ServiciosLavadoNegocio : IServiciosLavadoNegocio
    {
        private readonly IConexion _conexion;

        public ServiciosLavadoNegocio(IConexion? conexion = null)
        {
            _conexion = conexion ?? new Conexion();
        }

        public List<ServicioLavado> Consultar()
        {
            return _conexion.ServiciosLavado!
                .Include(s => s.Registro)
                .OrderByDescending(s => s.IdServicio)
                .ToList();
        }

        public List<ServicioLavado> ConsultarPorPlaca(string placa)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            return _conexion.ServiciosLavado!
                .Include(s => s.Registro)
                .Where(s => s.Placa == pNorm)
                .OrderByDescending(s => s.IdServicio)
                .ToList();
        }

        public ServicioLavado? BuscarPorId(int idServicio)
        {
            return _conexion.ServiciosLavado!
                .Include(s => s.Registro)
                .FirstOrDefault(s => s.IdServicio == idServicio);
        }

        public ServicioLavado SolicitarLavado(string placa, string tipoLavado = "Lavado General", decimal precio = 25000m, DateTime? ahora = null)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();

            // Regla RN-05 & Criterio 2 (HU-03): Obligatorio registro de parqueadero activo
            var registroActivo = _conexion.RegistrosParqueadero!
                .Where(r => r.Placa == pNorm && (r.EstadoRegistro == EstadoRegistro.ACTIVO || r.EstadoRegistro == EstadoRegistro.PAGADO))
                .OrderByDescending(r => r.IdRegistro)
                .FirstOrDefault();

            if (registroActivo == null)
            {
                throw new InvalidOperationException("No se puede asignar un lavado a un vehículo que no cuenta con un registro de parqueadero activo");
            }

            var fecha = ahora ?? DateTime.Now;
            var servicio = new ServicioLavado
            {
                IdRegistro = registroActivo.IdRegistro,
                Placa = pNorm,
                TipoLavado = string.IsNullOrWhiteSpace(tipoLavado) ? "Lavado General" : tipoLavado,
                PrecioBase = precio > 0 ? precio : 25000m,
                EstadoLavado = EstadoLavado.PENDIENTE,
                FechaHoraSolicitud = fecha,
                Facturado = false
            };

            _conexion.ServiciosLavado!.Add(servicio);
            _conexion.SaveChanges();

            return servicio;
        }

        public ServicioLavado AvanzarEstado(int idServicio, DateTime? ahora = null)
        {
            var servicio = BuscarPorId(idServicio);
            if (servicio == null)
                throw new KeyNotFoundException($"No se encontró el servicio de lavado #{idServicio}.");

            var fecha = ahora ?? DateTime.Now;

            // Delegación al patrón GoF State
            servicio.Avanzar(fecha);

            _conexion.SaveChanges();
            return servicio;
        }
    }
}
