using LibParkFlow.Entidades;
using LibPresentaciones.ImplementacionesPresentaciones;
using LibPresentaciones.InterfacesPresentaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages.Ventanas
{
    public class LiquidacionModel : PageModel
    {
        private readonly ILiquidacionNegocioPresentacion _liquidacionPresentacion;
        private readonly IRegistrosNegocioPresentacion _registrosPresentacion;

        [BindProperty]
        public string PlacaConsulta { get; set; } = string.Empty;

        [BindProperty]
        public string MetodoPago { get; set; } = "Efectivo";

        [BindProperty]
        public decimal MontoRecibido { get; set; } = 0;

        public ResumenCuentaPresentacionDto? CuentaActiva { get; set; }
        public ComprobantePago? ComprobanteGenerado { get; set; }
        public List<RegistroParqueadero> CuentasAbiertas { get; set; } = new();

        public LiquidacionModel()
        {
            _liquidacionPresentacion = new LiquidacionNegocioPresentacion();
            _registrosPresentacion = new RegistrosNegocioPresentacion();
        }

        public IActionResult OnGet()
        {
            CargarCuentas();
            return Page();
        }

        private void CargarCuentas()
        {
            try
            {
                CuentasAbiertas = _registrosPresentacion.ConsultarActivos();
            }
            catch
            {
                CuentasAbiertas = new();
            }
        }

        public IActionResult OnPostBtConsultar(string? placa = null)
        {
            CargarCuentas();
            try
            {
                var target = string.IsNullOrWhiteSpace(placa) ? PlacaConsulta : placa;
                if (string.IsNullOrWhiteSpace(target))
                {
                    ViewData["MensajeError"] = "Por favor ingrese o seleccione una placa válida.";
                    return Page();
                }

                PlacaConsulta = target.Trim().ToUpper();
                CuentaActiva = _liquidacionPresentacion.ConsultarCuenta(PlacaConsulta);
                MontoRecibido = CuentaActiva.Total;
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
            }

            return Page();
        }

        public IActionResult OnPostBtPagar()
        {
            CargarCuentas();
            try
            {
                if (string.IsNullOrWhiteSpace(PlacaConsulta))
                {
                    ViewData["MensajeError"] = "Por favor ingrese la placa a liquidar.";
                    return Page();
                }

                var dto = new LiquidarPresentacionDto
                {
                    Placa = PlacaConsulta.Trim().ToUpper(),
                    MetodoPago = MetodoPago,
                    Canal = CanalPago.CAJA,
                    ValorRecibido = MontoRecibido,
                    Referencia = MetodoPago == "Tarjeta" ? "AUTH-" + DateTime.Now.Ticks.ToString().Substring(10) : (MetodoPago == "Transferencia" ? "TRANSF-QR" : null)
                };

                ComprobanteGenerado = _liquidacionPresentacion.Liquidar(dto);
                ViewData["MensajeExito"] = $"¡Pago procesado exitosamente! Factura #{ComprobanteGenerado.NumeroFactura} por ${ComprobanteGenerado.TotalPagar:N0} ({ComprobanteGenerado.MetodoPago}).";

                CuentaActiva = _liquidacionPresentacion.ConsultarCuenta(PlacaConsulta.Trim().ToUpper());
                CargarCuentas();
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
            }

            return Page();
        }

        public IActionResult OnPostBtAutorizarSalida()
        {
            CargarCuentas();
            try
            {
                if (string.IsNullOrWhiteSpace(PlacaConsulta))
                {
                    ViewData["MensajeError"] = "Placa no válida.";
                    return Page();
                }

                _liquidacionPresentacion.AutorizarSalida(PlacaConsulta.Trim().ToUpper());
                ViewData["MensajeExito"] = $"¡Salida autorizada con éxito para el vehículo {PlacaConsulta.ToUpper()}! Celda liberada en SQL Server.";
                CuentaActiva = null;
                PlacaConsulta = string.Empty;
                CargarCuentas();
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = ex.Message;
            }

            return Page();
        }
    }
}
