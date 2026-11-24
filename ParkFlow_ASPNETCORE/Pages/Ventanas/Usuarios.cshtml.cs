using LibParkFlow.Entidades;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class UsuariosModel : PageModel
    {
        private readonly IUsuariosNegocioPresentacion _usuariosPresentacion;

        [BindProperty]
        public List<Usuario>? ListaUsuarios { get; set; }

        public UsuariosModel()
        {
            _usuariosPresentacion = new UsuariosNegocioPresentacion();
        }

        public IActionResult OnGet()
        {
            CargarDatos();
            return Page();
        }

        public void CargarDatos()
        {
            try
            {
                ListaUsuarios = _usuariosPresentacion.Consultar();
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = $"Error al consultar usuarios: {ex.Message}";
            }
        }

        public IActionResult OnPostBtRefrescar()
        {
            CargarDatos();
            return Page();
        }
    }
}
