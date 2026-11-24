using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ParkFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacionController : ControllerBase
    {
        private readonly IUsuariosNegocio _usuariosNegocio;

        public AutenticacionController(IUsuariosNegocio usuariosNegocio)
        {
            _usuariosNegocio = usuariosNegocio;
        }

        public class LoginDto
        {
            public string Codigo { get; set; } = string.Empty;
            public string Clave { get; set; } = string.Empty;
            public DateTime? FechaSimulada { get; set; }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var res = _usuariosNegocio.IniciarSesion(dto.Codigo, dto.Clave, dto.FechaSimulada);
            if (!res.Autenticado)
            {
                return BadRequest(new { ok = false, mensaje = res.Mensaje });
            }

            return Ok(new
            {
                ok = true,
                mensaje = res.Mensaje,
                usuario = new
                {
                    res.Usuario!.IdUsuario,
                    res.Usuario.Documento,
                    res.Usuario.Nombre,
                    res.Usuario.CodigoOperativo
                },
                rol = res.RolActivo?.Nombre,
                tipoRol = res.RolActivo?.Tipo.ToString(),
                operaciones = res.OperacionesPermitidas
            });
        }
    }
}
