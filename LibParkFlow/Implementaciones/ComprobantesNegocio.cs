using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public class ComprobantesNegocio : IComprobantesNegocio
    {
        private readonly IConexion _conexion;

        public ComprobantesNegocio(IConexion? conexion = null)
        {
            _conexion = conexion ?? new Conexion();
        }

        public List<ComprobantePago> Consultar()
        {
            return _conexion.ComprobantesPago!
                .Include(c => c.Registro)
                .Include(c => c.Caja)
                .OrderByDescending(c => c.IdFactura)
                .ToList();
        }

        public List<ComprobantePago> ConsultarPorPlaca(string placa)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            return _conexion.ComprobantesPago!
                .Include(c => c.Registro)
                .Include(c => c.Caja)
                .Where(c => c.Registro != null && c.Registro.Placa == pNorm)
                .OrderByDescending(c => c.IdFactura)
                .ToList();
        }

        public ComprobantePago? BuscarPorId(int idFactura)
        {
            return _conexion.ComprobantesPago!
                .Include(c => c.Registro)
                .Include(c => c.Caja)
                .FirstOrDefault(c => c.IdFactura == idFactura);
        }

        public ComprobantePago? BuscarPorNumero(string numeroFactura)
        {
            var num = (numeroFactura ?? "").Trim().ToUpper();
            return _conexion.ComprobantesPago!
                .Include(c => c.Registro)
                .Include(c => c.Caja)
                .FirstOrDefault(c => c.NumeroFactura == num);
        }
    }
}
