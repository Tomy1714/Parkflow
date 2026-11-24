using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CeldasController : ControllerBase
    {
        private readonly ICeldasNegocio _celdasNegocio;

        public CeldasController(ICeldasNegocio celdasNegocio)
        {
            _celdasNegocio = celdasNegocio;
        }

        [HttpGet]
        public IActionResult Consultar()
        {
            return Ok(_celdasNegocio.Consultar());
        }

        [HttpGet("disponibles")]
        public IActionResult ConsultarDisponibles()
        {
            return Ok(_celdasNegocio.ConsultarDisponibles());
        }

        [HttpGet("contadores")]
        public IActionResult ObtenerContadores()
        {
            return Ok(_celdasNegocio.ObtenerContadores());
        }

        public class CambiarEstadoCeldaDto
        {
            public EstadoCelda Estado { get; set; }
        }

        [HttpPut("{numero}/estado")]
        public IActionResult CambiarEstado(string numero, [FromBody] CambiarEstadoCeldaDto dto)
        {
            try
            {
                var celda = _celdasNegocio.CambiarEstado(numero, dto.Estado);
                return Ok(new { ok = true, celda });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
    }
}
