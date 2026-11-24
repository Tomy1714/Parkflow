using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CajasController : ControllerBase
    {
        private readonly ICajasNegocio _cajasNegocio;

        public CajasController(ICajasNegocio cajasNegocio)
        {
            _cajasNegocio = cajasNegocio;
        }

        [HttpGet]
        public IActionResult Consultar()
        {
            return Ok(_cajasNegocio.Consultar());
        }

        [HttpGet("activa")]
        public IActionResult ObtenerCajaActiva()
        {
            var caja = _cajasNegocio.ObtenerCajaActiva();
            if (caja == null) return NotFound(new { ok = false, mensaje = "No hay una caja abierta." });
            return Ok(caja);
        }

        public class AbrirCajaDto
        {
            public int IdUsuario { get; set; }
            public decimal MontoInicial { get; set; }
            public DateTime? Ahora { get; set; }
        }

        [HttpPost("abrir")]
        public IActionResult Abrir([FromBody] AbrirCajaDto dto)
        {
            try
            {
                var caja = _cajasNegocio.AbrirCaja(dto.IdUsuario, dto.MontoInicial, dto.Ahora);
                return Ok(new { ok = true, caja });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("{id}/cerrar")]
        public IActionResult Cerrar(int id)
        {
            try
            {
                var caja = _cajasNegocio.CerrarCaja(id);
                return Ok(new { ok = true, caja });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
    }
}
