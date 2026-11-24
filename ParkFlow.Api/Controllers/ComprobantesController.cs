using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComprobantesController : ControllerBase
    {
        private readonly IComprobantesNegocio _comprobantesNegocio;

        public ComprobantesController(IComprobantesNegocio comprobantesNegocio)
        {
            _comprobantesNegocio = comprobantesNegocio;
        }

        [HttpGet]
        public IActionResult Consultar()
        {
            return Ok(_comprobantesNegocio.Consultar());
        }

        [HttpGet("{id:int}")]
        public IActionResult BuscarPorId(int id)
        {
            var comp = _comprobantesNegocio.BuscarPorId(id);
            if (comp == null) return NotFound(new { ok = false, mensaje = "Comprobante no encontrado." });
            return Ok(comp);
        }

        [HttpGet("numero/{numero}")]
        public IActionResult BuscarPorNumero(string numero)
        {
            var comp = _comprobantesNegocio.BuscarPorNumero(numero);
            if (comp == null) return NotFound(new { ok = false, mensaje = "Comprobante no encontrado." });
            return Ok(comp);
        }

        [HttpGet("placa/{placa}")]
        public IActionResult ConsultarPorPlaca(string placa)
        {
            return Ok(_comprobantesNegocio.ConsultarPorPlaca(placa));
        }
    }
}
