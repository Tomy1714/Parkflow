using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones.Patrones.FacadeLiquidacion
{
    /// <summary>
    /// Fachada (GoF Facade) para liquidación, cobro unificado y autorización de salida
    /// Coordina RegistroParqueadero, ServicioLavado, ComprobantePago, Caja y Celda dentro de una transacción.
    /// </summary>
    public class LiquidacionFacade : ILiquidacionFacade
    {
        private readonly IConexion _conexion;

        public LiquidacionFacade(IConexion conexion)
        {
            _conexion = conexion;
        }

        public (RegistroParqueadero registro, decimal parqueadero, List<ServicioLavado> facturables, List<ServicioLavado> enCurso, decimal total) ConsultarCuenta(string placa, DateTime? ahora = null)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            var reg = _conexion.RegistrosParqueadero!
                .Include(r => r.ServiciosLavado)
                .Include(r => r.Celda)
                .OrderByDescending(r => r.IdRegistro)
                .FirstOrDefault(r => r.Placa == pNorm && r.EstadoRegistro != EstadoRegistro.FINALIZADO);

            if (reg == null)
            {
                throw new InvalidOperationException($"No existe una cuenta abierta para la placa {pNorm}.");
            }

            var fin = ahora ?? DateTime.Now;
            var (_, _, _, tarifaParq) = reg.CalcularTarifaParqueadero(fin);

            var lavados = _conexion.ServiciosLavado!
                .Where(s => s.IdRegistro == reg.IdRegistro)
                .ToList();

            var facturables = lavados.Where(l => l.EstadoLavado == EstadoLavado.LISTO && !l.Facturado).ToList();
            var enCurso = lavados.Where(l => l.EstadoLavado != EstadoLavado.LISTO).ToList();
            decimal total = tarifaParq + facturables.Sum(l => l.PrecioBase);

            return (reg, tarifaParq, facturables, enCurso, total);
        }

        public ComprobantePago Liquidar(string placa, IMetodoPagoStrategy metodo, DatosPago? datos, CanalPago canal, int? idCaja = null, DateTime? ahora = null)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            var (reg, parqueadero, facturables, enCurso, total) = ConsultarCuenta(pNorm, ahora);

            if (reg.EstadoRegistro == EstadoRegistro.PAGADO)
            {
                throw new InvalidOperationException("Esta cuenta ya fue pagada.");
            }

            if (enCurso.Any())
            {
                throw new InvalidOperationException("Hay un lavado en curso: debe estar en estado Listo antes de cobrar.");
            }

            var resultadoPago = metodo.Procesar(total, datos);
            if (!resultadoPago.Aprobado)
            {
                throw new InvalidOperationException(resultadoPago.Mensaje);
            }

            var fechaEmision = ahora ?? DateTime.Now;
            int totalFacturas = (_conexion.ComprobantesPago?.Count() ?? 0) + 1;
            string numFactura = $"FAC-{totalFacturas:D4}";

            var comprobante = new ComprobantePago
            {
                NumeroFactura = numFactura,
                IdRegistro = reg.IdRegistro,
                FechaEmision = fechaEmision,
                ValorParqueadero = parqueadero,
                ValorLavado = facturables.Sum(l => l.PrecioBase),
                TotalPagar = total,
                MetodoPago = metodo.NombreMetodo,
                Canal = canal,
                Referencia = resultadoPago.Referencia,
                ValorRecibido = datos?.ValorRecibido,
                Cambio = resultadoPago.Cambio
            };

            // Buscar caja activa
            Caja? caja = null;
            if (idCaja.HasValue)
            {
                caja = _conexion.Cajas?.FirstOrDefault(c => c.IdCaja == idCaja.Value && c.EstadoCaja == EstadoCaja.ABIERTA);
            }
            if (caja == null)
            {
                caja = _conexion.Cajas?.OrderByDescending(c => c.IdCaja).FirstOrDefault(c => c.EstadoCaja == EstadoCaja.ABIERTA);
            }

            if (caja != null)
            {
                comprobante.IdCaja = caja.IdCaja;
                caja.RegistrarMovimiento(total);
            }

            reg.CostoParqueadero = parqueadero;
            reg.MarcarPagado(fechaEmision);

            foreach (var l in facturables)
            {
                l.Facturado = true;
                _conexion.DetallesServicio?.Add(new DetalleServicio
                {
                    IdRegistro = reg.IdRegistro,
                    IdServicio = l.IdServicio,
                    Cantidad = 1,
                    Subtotal = l.PrecioBase
                });
            }

            _conexion.ComprobantesPago?.Add(comprobante);
            _conexion.SaveChanges();

            return comprobante;
        }

        public void AutorizarSalida(string placa, DateTime? ahora = null)
        {
            var pNorm = (placa ?? "").Trim().ToUpper();
            var reg = _conexion.RegistrosParqueadero!
                .Include(r => r.Celda)
                .OrderByDescending(r => r.IdRegistro)
                .FirstOrDefault(r => r.Placa == pNorm && r.EstadoRegistro != EstadoRegistro.FINALIZADO);

            if (reg == null)
            {
                throw new InvalidOperationException($"No se encontró un registro activo para la placa {pNorm}.");
            }

            if (!reg.EstaPagado())
            {
                throw new InvalidOperationException("Imposible registrar salida: Cuenta con saldo pendiente de pago");
            }

            var fechaSalida = ahora ?? DateTime.Now;
            reg.Finalizar(fechaSalida);

            var celda = _conexion.Celdas?.FirstOrDefault(c => c.NumeroCelda == reg.NumeroCelda);
            if (celda != null)
            {
                celda.Liberar();
            }

            _conexion.SaveChanges();
        }
    }
}
