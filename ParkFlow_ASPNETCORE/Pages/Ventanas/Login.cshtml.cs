using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class LoginModel : PageModel
    {
        private readonly IUsuariosNegocioPresentacion _usuariosPresentacion;

        [BindProperty]
        public string CodigoOperativo { get; set; } = "OP-2094";

        [BindProperty]
        public string Contrasena { get; set; } = "parkflow2026";

        public LoginModel()
        {
            _usuariosPresentacion = new UsuariosNegocioPresentacion();
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPostBtIngresar()
        {
            try
            {
                var resultado = _usuariosPresentacion.IniciarSesion(CodigoOperativo.Trim(), Contrasena);

                if (resultado.Autenticado)
                {
                    ViewData["MensajeExito"] = $"{resultado.Mensaje}. Rol activo: {resultado.RolActivo?.Nombre} ({resultado.RolActivo?.Tipo}).";
                }
                else
                {
                    ViewData["MensajeError"] = resultado.Mensaje;
                }
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
            }

            return Page();
        }
    }
}
