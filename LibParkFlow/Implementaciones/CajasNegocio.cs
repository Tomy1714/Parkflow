using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public class CajasNegocio : ICajasNegocio
    {
        private readonly IConexion _conexion;

        public CajasNegocio(IConexion? conexion = null)
        {
            _conexion = conexion ?? new Conexion();
        }

        public List<Caja> Consultar()
        {
            return _conexion.Cajas!
                .Include(c => c.Usuario)
                .Include(c => c.Comprobantes)
                .OrderByDescending(c => c.IdCaja)
                .ToList();
        }

        public Caja? ObtenerCajaActiva()
        {
            return _conexion.Cajas!
                .Include(c => c.Usuario)
                .Include(c => c.Comprobantes)
                .OrderByDescending(c => c.IdCaja)
                .FirstOrDefault(c => c.EstadoCaja == EstadoCaja.ABIERTA);
        }

        public Caja AbrirCaja(int idUsuario, decimal montoInicial, DateTime? ahora = null)
        {
            var cajaAbierta = ObtenerCajaActiva();
            if (cajaAbierta != null)
                return cajaAbierta;

            var nuevaCaja = new Caja
            {
                IdUsuario = idUsuario,
                FechaApertura = ahora ?? DateTime.Now,
                MontoInicial = montoInicial,
                TotalRecaudado = 0m,
                EstadoCaja = EstadoCaja.ABIERTA
            };

            _conexion.Cajas!.Add(nuevaCaja);
            _conexion.SaveChanges();

            return nuevaCaja;
        }

        public Caja CerrarCaja(int idCaja)
        {
            var caja = _conexion.Cajas!.FirstOrDefault(c => c.IdCaja == idCaja);
            if (caja == null)
                throw new KeyNotFoundException($"No se encontró la caja #{idCaja}.");

            caja.CerrarCaja();
            _conexion.SaveChanges();

            return caja;
        }
    }
}
