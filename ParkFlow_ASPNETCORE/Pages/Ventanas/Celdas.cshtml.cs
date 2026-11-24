using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class CeldasModel : PageModel
    {
        private readonly ICeldasNegocioPresentacion _celdasNegocio;

        [BindProperty]
        public List<Celda>? Lista { get; set; }

        public ContadoresCeldasDto? Contadores { get; set; }

        public CeldasModel()
        {
            _celdasNegocio = new CeldasNegocioPresentacion();
        }

        public IActionResult OnGet()
        {
            OnPostBtRefrescar();
            return Page();
        }

        public void OnPostBtRefrescar()
        {
            try
            {
                Lista = _celdasNegocio.Consultar();
                Contadores = _celdasNegocio.ObtenerContadores();
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
            }
        }

        public IActionResult OnPostBtCambiarEstado(string numeroCelda, string nuevoEstado)
        {
            try
            {
                if (Enum.TryParse<EstadoCelda>(nuevoEstado, out var estado))
                {
                    _celdasNegocio.CambiarEstado(numeroCelda, estado);
                }
                OnPostBtRefrescar();
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = ex.Message;
            }

            return Page();
        }
    }
}
