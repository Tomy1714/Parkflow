using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class IngresoModel : PageModel
    {
        private readonly IRegistrosNegocioPresentacion _registrosNegocioPresentacion;
        private readonly ICeldasNegocioPresentacion _celdasNegocioPresentacion;

        [BindProperty]
        public string Placa { get; set; } = string.Empty;

        [BindProperty]
        public string TipoVehiculo { get; set; } = "Automóvil";

        [BindProperty]
        public string? Marca { get; set; }

        [BindProperty]
        public string? Modelo { get; set; }

        [BindProperty]
        public string NumeroCelda { get; set; } = string.Empty;

        public List<RegistroParqueadero> UltimosIngresos { get; set; } = new();
        public List<Celda> CeldasDisponibles { get; set; } = new();
        public TicketIngresoDto? TicketGenerado { get; set; }

        public IngresoModel()
        {
            _registrosNegocioPresentacion = new RegistrosNegocioPresentacion();
            _celdasNegocioPresentacion = new CeldasNegocioPresentacion();
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
                UltimosIngresos = _registrosNegocioPresentacion.Consultar();
                CeldasDisponibles = _celdasNegocioPresentacion.ConsultarDisponibles();
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = $"Error al consultar API: {ex.Message}";
            }
        }

        public IActionResult OnPostBtRegistrar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Placa) || Placa.Trim().Length < 6)
                {
                    ViewData["MensajeError"] = "Por favor ingrese un número de placa válido antes de continuar.";
                    CargarDatos();
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(NumeroCelda))
                {
                    ViewData["MensajeError"] = "Debe seleccionar una celda para el ingreso.";
                    CargarDatos();
                    return Page();
                }

                TicketGenerado = _registrosNegocioPresentacion.RegistrarIngreso(Placa.Trim().ToUpper(), TipoVehiculo, NumeroCelda, null, Marca, Modelo);
                ViewData["MensajeExito"] = $"Ingreso registrado exitosamente en SQL Server. Ticket #{TicketGenerado.IdRegistro} - Celda {TicketGenerado.NumeroCelda}.";

                Placa = string.Empty;
                Marca = string.Empty;
                Modelo = string.Empty;
                NumeroCelda = string.Empty;
                CargarDatos();
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
                CargarDatos();
            }

            return Page();
        }

        public IActionResult OnPostBtRefrescar()
        {
            CargarDatos();
            return Page();
        }
    }
}
