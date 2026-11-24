using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservasController : ControllerBase
    {
        private readonly IReservasNegocio _reservasNegocio;

        public ReservasController(IReservasNegocio reservasNegocio)
        {
            _reservasNegocio = reservasNegocio;
        }

        public class CrearReservaDto
        {
            public string Placa { get; set; } = string.Empty;
            public string NumeroCelda { get; set; } = string.Empty;
            public DateTime HoraReserva { get; set; }
            public int? IdUsuario { get; set; }
        }

        public class VerificarVencimientosDto
        {
            public DateTime Ahora { get; set; }
        }

        [HttpGet]
        public IActionResult Consultar()
        {
            return Ok(_reservasNegocio.Consultar());
        }

        [HttpGet("activas")]
        public IActionResult ConsultarActivas()
        {
            return Ok(_reservasNegocio.ConsultarActivas());
        }

        [HttpGet("cliente/{idUsuario}")]
        public IActionResult ConsultarPorUsuario(int idUsuario)
        {
            return Ok(_reservasNegocio.ConsultarPorUsuario(idUsuario));
        }

        [HttpGet("auditoria")]
        public IActionResult ConsultarAuditoria()
        {
            return Ok(_reservasNegocio.ConsultarHistorialAuditoria());
        }

        [HttpPost]
        public IActionResult Crear([FromBody] CrearReservaDto dto)
        {
            try
            {
                var reserva = _reservasNegocio.CrearReserva(dto.Placa, dto.NumeroCelda, dto.HoraReserva, dto.IdUsuario);
                return Ok(new { ok = true, reserva });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("{id}/cancelar")]
        public IActionResult Cancelar(int id, [FromBody] VerificarVencimientosDto? dto)
        {
            try
            {
                _reservasNegocio.CancelarReserva(id, dto?.Ahora);
                return Ok(new { ok = true, mensaje = $"Reserva #{id} cancelada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("verificar-vencimientos")]
        public IActionResult VerificarVencimientos([FromBody] VerificarVencimientosDto dto)
        {
            int canceladas = _reservasNegocio.VerificarVencimientosPorTolerancia(dto.Ahora);
            return Ok(new { ok = true, canceladas });
        }
    }
}
