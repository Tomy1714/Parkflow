using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngresoController : ControllerBase
    {
        private readonly IRegistrosNegocio _registrosNegocio;

        public IngresoController(IRegistrosNegocio registrosNegocio)
        {
            _registrosNegocio = registrosNegocio;
        }

        public class RegistrarIngresoDto
        {
            public string Placa { get; set; } = string.Empty;
            public string TipoVehiculo { get; set; } = "Automóvil";
            public string? Marca { get; set; }
            public string? Modelo { get; set; }
            public string? NumeroCelda { get; set; }
            public DateTime? Ahora { get; set; }
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistrarIngresoDto dto)
        {
            try
            {
                var ticket = _registrosNegocio.RegistrarIngreso(dto.Placa, dto.TipoVehiculo, dto.NumeroCelda, dto.Ahora, dto.Marca, dto.Modelo);
                return Ok(new { ok = true, ticket });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ok = false, error = ex.Message });
            }
        }

        [HttpGet("activos")]
        public IActionResult ConsultarActivos()
        {
            var lista = _registrosNegocio.ConsultarActivos();
            return Ok(lista);
        }

        [HttpGet("ultimos")]
        public IActionResult ConsultarUltimos()
        {
            var lista = _registrosNegocio.Consultar().Take(10);
            return Ok(lista);
        }
    }
}
