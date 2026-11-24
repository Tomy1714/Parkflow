using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculosController : ControllerBase
    {
        private readonly IVehiculosNegocio _vehiculosNegocio;

        public VehiculosController(IVehiculosNegocio vehiculosNegocio)
        {
            _vehiculosNegocio = vehiculosNegocio;
        }

        [HttpGet]
        public IActionResult Consultar()
        {
            return Ok(_vehiculosNegocio.Consultar());
        }

        [HttpGet("cliente/{idUsuario}")]
        public IActionResult ConsultarPorUsuario(int idUsuario)
        {
            return Ok(_vehiculosNegocio.ConsultarPorUsuario(idUsuario));
        }

        [HttpGet("{placa}")]
        public IActionResult BuscarPorPlaca(string placa)
        {
            var vehiculo = _vehiculosNegocio.BuscarPorPlaca(placa);
            if (vehiculo == null) return NotFound(new { ok = false, mensaje = "Vehículo no encontrado" });
            return Ok(vehiculo);
        }

        [HttpPost]
        public IActionResult Guardar([FromBody] Vehiculo vehiculo)
        {
            try
            {
                var guardado = _vehiculosNegocio.Guardar(vehiculo);
                return Ok(new { ok = true, vehiculo = guardado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }

        public class RegistrarVehiculoClienteDto
        {
            public string Placa { get; set; } = string.Empty;
            public int IdUsuario { get; set; }
            public string TipoVehiculo { get; set; } = "Automóvil";
        }

        [HttpPost("cliente")]
        public IActionResult RegistrarCliente([FromBody] RegistrarVehiculoClienteDto dto)
        {
            try
            {
                var v = _vehiculosNegocio.RegistrarVehiculoCliente(dto.Placa, dto.IdUsuario, dto.TipoVehiculo);
                return Ok(new { ok = true, vehiculo = v });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
        [HttpDelete("{placa}")]
        public IActionResult Borrar(string placa)
        {
            try
            {
                var eliminado = _vehiculosNegocio.Borrar(placa);
                if (!eliminado) return NotFound(new { ok = false, mensaje = "Vehículo no encontrado" });
                return Ok(new { ok = true, mensaje = "Vehículo eliminado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ok = false, error = ex.Message });
            }
        }
    }
}
