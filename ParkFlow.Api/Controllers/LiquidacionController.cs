using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones.Patrones.StrategyPago;
using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LiquidacionController : ControllerBase
    {
        private readonly ILiquidacionFacade _liquidacionFacade;

        public LiquidacionController(ILiquidacionFacade liquidacionFacade)
        {
            _liquidacionFacade = liquidacionFacade;
        }

        [HttpGet("cuenta/{placa}")]
        public IActionResult ConsultarCuenta(string placa, [FromQuery] DateTime? ahora = null)
        {
            try
            {
                var (reg, parq, facturables, enCurso, total) = _liquidacionFacade.ConsultarCuenta(placa, ahora);
                return Ok(new
                {
                    ok = true,
                    placa = reg.Placa,
                    celda = reg.NumeroCelda,
                    fechaEntrada = reg.FechaHoraEntrada,
                    minutosEstadia = reg.CalcularTiempoEstadia(ahora),
                    costoParqueadero = parq,
                    facturables = facturables.Select(l => new { l.IdServicio, l.TipoLavado, l.PrecioBase, l.EstadoLavado }),
                    enCurso = enCurso.Select(l => new { l.IdServicio, l.TipoLavado, l.EstadoLavado }),
                    total,
                    estadoRegistro = reg.EstadoRegistro.ToString()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        public class LiquidarDto
        {
            public string Placa { get; set; } = string.Empty;
            public string MetodoPago { get; set; } = "Efectivo"; // Efectivo, Transferencia, Tarjeta
            public CanalPago Canal { get; set; } = CanalPago.CAJA;
            public decimal ValorRecibido { get; set; }
            public string? Referencia { get; set; }
            public int? IdCaja { get; set; }
            public DateTime? Ahora { get; set; }
        }

        [HttpPost("liquidar")]
        public IActionResult Liquidar([FromBody] LiquidarDto dto)
        {
            try
            {
                var estrategia = FabricaMetodoPago.ObtenerEstrategia(dto.MetodoPago);
                var datosPago = new DatosPago
                {
                    ValorRecibido = dto.ValorRecibido,
                    Referencia = dto.Referencia
                };

                var comprobante = _liquidacionFacade.Liquidar(dto.Placa, estrategia, datosPago, dto.Canal, dto.IdCaja, dto.Ahora);

                return Ok(new
                {
                    ok = true,
                    mensaje = "Cobro procesado exitosamente.",
                    comprobante = new
                    {
                        comprobante.IdFactura,
                        comprobante.NumeroFactura,
                        comprobante.FechaEmision,
                        comprobante.ValorParqueadero,
                        comprobante.ValorLavado,
                        comprobante.TotalPagar,
                        comprobante.MetodoPago,
                        comprobante.Referencia,
                        comprobante.Cambio
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        public class AutorizarSalidaDto
        {
            public string Placa { get; set; } = string.Empty;
            public DateTime? Ahora { get; set; }
        }

        [HttpPost("autorizar-salida")]
        public IActionResult AutorizarSalida([FromBody] AutorizarSalidaDto dto)
        {
            try
            {
                _liquidacionFacade.AutorizarSalida(dto.Placa, dto.Ahora);
                return Ok(new
                {
                    ok = true,
                    mensaje = $"Salida autorizada para el vehículo {dto.Placa}. Celda liberada."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
    }
}
