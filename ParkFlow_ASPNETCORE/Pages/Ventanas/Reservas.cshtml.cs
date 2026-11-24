using LibParkFlow.Entidades;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class ReservasModel : PageModel
    {
        private readonly IReservasNegocioPresentacion _reservasPresentacion;
        private readonly ICeldasNegocioPresentacion _celdasPresentacion;

        [BindProperty]
        public List<Reserva>? ReservasActivas { get; set; }

        [BindProperty]
        public List<AuditoriaReserva>? HistorialAuditoria { get; set; }

        [BindProperty]
        public List<Celda>? CeldasDisponibles { get; set; }

        [BindProperty]
        public string Placa { get; set; } = string.Empty;

        [BindProperty]
        public string NumeroCelda { get; set; } = string.Empty;

        [BindProperty]
        public string HoraReserva { get; set; } = "10:00";

        public ReservasModel()
        {
            _reservasPresentacion = new ReservasNegocioPresentacion();
            _celdasPresentacion = new CeldasNegocioPresentacion();
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
                ReservasActivas = _reservasPresentacion.ConsultarActivas();
                HistorialAuditoria = _reservasPresentacion.ConsultarHistorialAuditoria();
                CeldasDisponibles = _celdasPresentacion.ConsultarDisponibles();
            }
            catch (Exception ex)
            {
                ViewData["Mensaje"] = $"Error al consultar datos: {ex.Message}";
            }
        }

        public IActionResult OnPostBtRefrescar()
        {
            CargarDatos();
            return Page();
        }

        public IActionResult OnPostBtCrearReserva()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Placa) || Placa.Trim().Length < 6)
                {
                    ViewData["MensajeError"] = "Por favor ingrese un número de placa válido (mínimo 6 caracteres).";
                    CargarDatos();
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(NumeroCelda))
                {
                    ViewData["MensajeError"] = "Debe seleccionar una celda para la reserva.";
                    CargarDatos();
                    return Page();
                }

                var partesHora = (HoraReserva ?? "10:00").Split(':');
                int horas = int.Parse(partesHora[0]);
                int minutos = int.Parse(partesHora[1]);
                DateTime fechaHora = DateTime.Today.AddHours(horas).AddMinutes(minutos);

                var nueva = _reservasPresentacion.CrearReserva(Placa.Trim().ToUpper(), NumeroCelda, fechaHora);
                ViewData["MensajeExito"] = $"Reserva creada exitosamente: Placa {nueva.Placa} en Celda {nueva.NumeroCelda} para las {fechaHora:HH:mm}.";

                Placa = string.Empty;
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

        public IActionResult OnPostBtCancelarReserva(int data)
        {
            try
            {
                _reservasPresentacion.CancelarReserva(data, DateTime.Now);
                ViewData["MensajeExito"] = $"Reserva #{data} cancelada y celda liberada correctamente.";
                CargarDatos();
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
                CargarDatos();
            }

            return Page();
        }

        public IActionResult OnPostBtVerificarVencimientos()
        {
            try
            {
                int canceladas = _reservasPresentacion.VerificarVencimientosPorTolerancia(DateTime.Now);
                ViewData["MensajeExito"] = $"Verificación completada: {canceladas} reservas vencidas por inasistencia (Regla RN-03) canceladas automáticamente.";
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
