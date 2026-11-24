using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LavadoController : ControllerBase
    {
        private readonly IServiciosLavadoNegocio _serviciosLavadoNegocio;

        public LavadoController(IServiciosLavadoNegocio serviciosLavadoNegocio)
        {
            _serviciosLavadoNegocio = serviciosLavadoNegocio;
        }

        public class SolicitarLavadoDto
        {
            public string Placa { get; set; } = string.Empty;
            public string TipoLavado { get; set; } = "Lavado General";
            public decimal Precio { get; set; } = 25000m;
            public DateTime? Ahora { get; set; }
        }

        public class AvanzarLavadoDto
        {
            public DateTime? Ahora { get; set; }
        }

        [HttpGet]
        public IActionResult Consultar()
        {
            return Ok(_serviciosLavadoNegocio.Consultar());
        }

        [HttpGet("placa/{placa}")]
        public IActionResult ConsultarPorPlaca(string placa)
        {
            return Ok(_serviciosLavadoNegocio.ConsultarPorPlaca(placa));
        }

        [HttpPost("solicitar")]
        public IActionResult Solicitar([FromBody] SolicitarLavadoDto dto)
        {
            try
            {
                var servicio = _serviciosLavadoNegocio.SolicitarLavado(dto.Placa, dto.TipoLavado, dto.Precio, dto.Ahora);
                return Ok(new { ok = true, servicio });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("{id}/avanzar")]
        public IActionResult Avanzar(int id, [FromBody] AvanzarLavadoDto? dto)
        {
            try
            {
                var servicio = _serviciosLavadoNegocio.AvanzarEstado(id, dto?.Ahora);
                return Ok(new { ok = true, servicio });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
    }
}
