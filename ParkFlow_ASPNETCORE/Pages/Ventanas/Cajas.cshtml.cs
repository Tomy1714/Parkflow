using LibParkFlow.Entidades;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class CajasModel : PageModel
    {
        private readonly ICajasNegocioPresentacion _cajasPresentacion;

        [BindProperty]
        public Caja? CajaActiva { get; set; }

        [BindProperty]
        public List<Caja>? HistorialCajas { get; set; }

        [BindProperty]
        public decimal MontoInicial { get; set; } = 100000;

        [BindProperty]
        public decimal MontoFinalCierre { get; set; } = 150000;

        public CajasModel()
        {
            _cajasPresentacion = new CajasNegocioPresentacion();
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
                CajaActiva = _cajasPresentacion.ObtenerCajaActiva();
                HistorialCajas = _cajasPresentacion.Consultar();
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = $"Error al consultar caja: {ex.Message}";
            }
        }

        public IActionResult OnPostBtRefrescar()
        {
            CargarDatos();
            return Page();
        }

        public IActionResult OnPostBtAbrirCaja()
        {
            try
            {
                // Abre caja para cajero default (Id 3 Mariana Cajera)
                var nueva = _cajasPresentacion.AbrirCaja(3, MontoInicial);
                ViewData["MensajeExito"] = $"Caja #{nueva.IdCaja} abierta con éxito. Monto inicial: ${nueva.MontoInicial:N0}.";
                CargarDatos();
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
                CargarDatos();
            }

            return Page();
        }

        public IActionResult OnPostBtCerrarCaja(int data)
        {
            try
            {
                var cerrada = _cajasPresentacion.CerrarCaja(data);
                ViewData["MensajeExito"] = $"Caja #{cerrada.IdCaja} cerrada correctamente. Total recaudado: ${cerrada.TotalRecaudado:N0}.";
                CargarDatos();
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
                CargarDatos();
            }

            return Page();
        }
    }
}
