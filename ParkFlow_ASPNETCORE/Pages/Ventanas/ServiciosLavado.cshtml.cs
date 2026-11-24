using LibParkFlow.Entidades;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class ServiciosLavadoModel : PageModel
    {
        private readonly IServiciosLavadoNegocioPresentacion _lavadoPresentacion;
        private readonly IRegistrosNegocioPresentacion _registrosPresentacion;

        [BindProperty]
        public List<ServicioLavado>? ListaServicios { get; set; }

        [BindProperty]
        public List<RegistroParqueadero>? RegistrosActivos { get; set; }

        [BindProperty]
        public string Placa { get; set; } = string.Empty;

        [BindProperty]
        public string TipoLavado { get; set; } = "Lavado General";

        [BindProperty]
        public decimal PrecioBase { get; set; } = 25000;

        public ServiciosLavadoModel()
        {
            _lavadoPresentacion = new ServiciosLavadoNegocioPresentacion();
            _registrosPresentacion = new RegistrosNegocioPresentacion();
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
                ListaServicios = _lavadoPresentacion.Consultar();
                RegistrosActivos = _registrosPresentacion.ConsultarActivos();
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = $"Error al consultar servicios de lavado: {ex.Message}";
            }
        }

        public IActionResult OnPostBtRefrescar()
        {
            CargarDatos();
            return Page();
        }

        public IActionResult OnPostBtSolicitarLavado()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Placa))
                {
                    ViewData["MensajeError"] = "Por favor ingrese la placa del vehículo.";
                    CargarDatos();
                    return Page();
                }

                var nuevo = _lavadoPresentacion.SolicitarLavado(Placa.Trim().ToUpper(), TipoLavado, PrecioBase);
                ViewData["MensajeExito"] = $"Servicio de lavado #{nuevo.IdServicio} solicitado con éxito para el vehículo {nuevo.Placa}. Estado: PENDIENTE.";

                Placa = string.Empty;
                CargarDatos();
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
                CargarDatos();
            }

            return Page();
        }

        public IActionResult OnPostBtAvanzarEstado(int data)
        {
            try
            {
                var actualizado = _lavadoPresentacion.AvanzarEstado(data);
                ViewData["MensajeExito"] = $"Servicio #{actualizado.IdServicio} avanzado correctamente a estado: {actualizado.EstadoLavado}.";
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
