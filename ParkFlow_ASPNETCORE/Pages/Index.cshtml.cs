using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones.Patrones.FacadeLiquidacion;
using LibParkFlow.Implementaciones.Patrones.StrategyPago;
using LibParkFlow.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkFlow_ASPNETCORE.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICeldasNegocio _celdasNegocio;
        private readonly IRegistrosNegocio _registrosNegocio;
        private readonly IServiciosLavadoNegocio _serviciosLavadoNegocio;
        private readonly ILiquidacionFacade _liquidacionFacade;
        private readonly ICajasNegocio _cajasNegocio;
        private readonly IReservasNegocio _reservasNegocio;
        private readonly IUsuariosNegocio _usuariosNegocio;
        private readonly IVehiculosNegocio _vehiculosNegocio;
        private readonly IComprobantesNegocio _comprobantesNegocio;

        public IndexModel(
            ICeldasNegocio celdasNegocio,
            IRegistrosNegocio registrosNegocio,
            IServiciosLavadoNegocio serviciosLavadoNegocio,
            ILiquidacionFacade liquidacionFacade,
            ICajasNegocio cajasNegocio,
            IReservasNegocio reservasNegocio,
            IUsuariosNegocio usuariosNegocio,
            IVehiculosNegocio vehiculosNegocio,
            IComprobantesNegocio comprobantesNegocio)
        {
            _celdasNegocio = celdasNegocio;
            _registrosNegocio = registrosNegocio;
            _serviciosLavadoNegocio = serviciosLavadoNegocio;
            _liquidacionFacade = liquidacionFacade;
            _cajasNegocio = cajasNegocio;
            _reservasNegocio = reservasNegocio;
            _usuariosNegocio = usuariosNegocio;
            _vehiculosNegocio = vehiculosNegocio;
            _comprobantesNegocio = comprobantesNegocio;
        }

        // --- Estado de Autenticación y Navegación ---
        public string? UsuarioRol => HttpContext.Session.GetString("UsuarioRol");
        public string? UsuarioCodigo => HttpContext.Session.GetString("UsuarioCodigo");
        public string TabActiva { get; set; } = "dashboard";

        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        // --- Login Inputs ---
        [BindProperty]
        public string LoginRol { get; set; } = "Operador";

        [BindProperty]
        public string LoginCodigo { get; set; } = "OP-2094";

        [BindProperty]
        public string LoginClave { get; set; } = "parkflow2026";

        // --- Dashboard & Celdas ---
        public ContadoresCeldasDto? Contadores { get; set; }
        public List<Celda> Celdas { get; set; } = new();

        // --- Ingreso Inputs y Datos ---
        [BindProperty]
        public string IngresoPlaca { get; set; } = string.Empty;

        [BindProperty]
        public string IngresoTipo { get; set; } = "Automóvil";

        [BindProperty]
        public string? IngresoMarca { get; set; }

        [BindProperty]
        public string? IngresoModelo { get; set; }

        [BindProperty]
        public string IngresoCelda { get; set; } = string.Empty;

        [BindProperty]
        public bool IngresoSolicitarLavado { get; set; } = false;

        public List<Celda> CeldasDisponibles { get; set; } = new();
        public List<RegistroParqueadero> UltimosIngresos { get; set; } = new();
        public TicketIngresoDto? TicketGenerado { get; set; }

        // --- Lavado Inputs y Datos ---
        [BindProperty]
        public string LavadoPlaca { get; set; } = string.Empty;

        [BindProperty]
        public string LavadoTipo { get; set; } = "Lavado General";

        [BindProperty]
        public decimal LavadoPrecio { get; set; } = 25000m;

        public List<ServicioLavado> ListaServicios { get; set; } = new();

        // --- Caja & Liquidación Inputs y Datos ---
        [BindProperty]
        public string CajaPlaca { get; set; } = string.Empty;

        [BindProperty]
        public string MetodoPago { get; set; } = "Efectivo";

        public (RegistroParqueadero reg, decimal parq, List<ServicioLavado> facturables, List<ServicioLavado> enCurso, decimal total)? CuentaConsultada { get; set; }
        public List<RegistroParqueadero> CuentasAbiertas { get; set; } = new();
        public Caja? CajaActiva { get; set; }
        public ComprobantePago? ComprobanteGenerado { get; set; }

        // --- Reservas Inputs y Datos ---
        [BindProperty]
        public string ReservaPlaca { get; set; } = string.Empty;

        [BindProperty]
        public string ReservaCelda { get; set; } = string.Empty;

        [BindProperty]
        public string ReservaHora { get; set; } = "11:00";

        public List<Reserva> ReservasActivas { get; set; } = new();
        public List<AuditoriaReserva> AuditoriaReservas { get; set; } = new();

        // --- Datos para Portal de Cliente ---
        public Usuario? ClienteActual { get; set; }
        public List<Vehiculo> VehiculosCliente { get; set; } = new();
        public List<RegistroParqueadero> RegistrosCliente { get; set; } = new();
        public List<ServicioLavado> LavadosCliente { get; set; } = new();
        public List<Reserva> ReservasCliente { get; set; } = new();
        public List<ComprobantePago> ComprobantesCliente { get; set; } = new();

        [BindProperty]
        public string? NuevoVehiculoPlaca { get; set; }

        [BindProperty]
        public string? NuevoVehiculoTipo { get; set; } = "Automóvil";

        [BindProperty]
        public string? NuevoVehiculoMarca { get; set; }

        [BindProperty]
        public string? NuevoVehiculoModelo { get; set; }

        // --- Métodos de ciclo de vida ---
        public void OnGet(string? tab, string? placa)
        {
            TabActiva = string.IsNullOrWhiteSpace(tab) ? (UsuarioRol == "Cliente" ? "c-panel" : "dashboard") : tab.ToLower();
            if (!string.IsNullOrWhiteSpace(placa))
            {
                CajaPlaca = placa.Trim().ToUpper();
                ConsultarCuentaInterno(CajaPlaca);
            }
            CargarDatos();
        }

        public void CargarDatos()
        {
            try
            {
                Contadores = _celdasNegocio.ObtenerContadores();
                Celdas = _celdasNegocio.Consultar();
                CeldasDisponibles = _celdasNegocio.ConsultarDisponibles();
                UltimosIngresos = _registrosNegocio.ConsultarActivos();
                ListaServicios = _serviciosLavadoNegocio.Consultar();
                CuentasAbiertas = _registrosNegocio.ConsultarActivos();
                CajaActiva = _cajasNegocio.ObtenerCajaActiva();
                ReservasActivas = _reservasNegocio.ConsultarActivas();
                AuditoriaReservas = _reservasNegocio.ConsultarHistorialAuditoria();

                // Datos específicos para el rol Cliente
                if (UsuarioRol == "Cliente" && !string.IsNullOrWhiteSpace(UsuarioCodigo))
                {
                    ClienteActual = _usuariosNegocio.BuscarPorCodigo(UsuarioCodigo);
                    if (ClienteActual != null)
                    {
                        VehiculosCliente = _vehiculosNegocio.ConsultarPorUsuario(ClienteActual.IdUsuario);
                        var placas = VehiculosCliente.Select(v => v.Placa.ToUpper()).ToHashSet();
                        RegistrosCliente = UltimosIngresos.Where(r => placas.Contains(r.Placa.ToUpper())).ToList();
                        LavadosCliente = ListaServicios.Where(s => placas.Contains(s.Placa.ToUpper())).ToList();
                        ReservasCliente = ReservasActivas.Where(r => placas.Contains(r.Placa.ToUpper())).ToList();
                        ComprobantesCliente = _comprobantesNegocio.Consultar()
                            .Where(c => c.Registro != null && placas.Contains(c.Registro.Placa.ToUpper()))
                            .OrderByDescending(c => c.IdFactura)
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar datos: {ex.Message}";
            }
        }

        // --- Login & Logout ---
        public IActionResult OnPostLogin()
        {
            if (string.IsNullOrWhiteSpace(LoginCodigo) || string.IsNullOrWhiteSpace(LoginClave))
            {
                MensajeError = "Por favor ingrese su código y contraseña.";
                return Page();
            }

            var respuesta = _usuariosNegocio.IniciarSesion(LoginCodigo.Trim(), LoginClave.Trim());
            if (!respuesta.Autenticado)
            {
                MensajeError = respuesta.Mensaje ?? "Credenciales inválidas. Verifique su código y contraseña.";
                return Page();
            }

            string rolFinal = respuesta.RolActivo?.Nombre ?? LoginRol;
            HttpContext.Session.SetString("UsuarioRol", rolFinal);
            HttpContext.Session.SetString("UsuarioCodigo", LoginCodigo.Trim().ToUpper());

            string destinoTab = rolFinal switch
            {
                "Lavador" => "lavado",
                "Cajero" => "caja",
                "Cliente" => "c-panel",
                _ => "dashboard"
            };

            return RedirectToPage("/Index", new { tab = destinoTab });
        }

        public IActionResult OnPostSeleccionarRol(string rol)
        {
            LoginRol = rol;
            LoginCodigo = rol switch
            {
                "Administrador" => "AD-1001",
                "Operador" => "OP-2094",
                "Cajero" => "CJ-3310",
                "Lavador" => "LV-4125",
                "Cliente" => "CL-5001",
                _ => "OP-2094"
            };
            LoginClave = "parkflow2026";
            return Page();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Index");
        }

        // --- Registro de Ingreso (HU-01) ---
        public IActionResult OnPostRegistrarIngreso()
        {
            TabActiva = "ingreso";
            try
            {
                if (string.IsNullOrWhiteSpace(IngresoPlaca) || IngresoPlaca.Trim().Length < 6)
                {
                    MensajeError = "Ingrese una placa válida de al menos 6 caracteres.";
                    CargarDatos();
                    return Page();
                }

                TicketGenerado = _registrosNegocio.RegistrarIngreso(
                    IngresoPlaca.Trim().ToUpper(),
                    IngresoTipo,
                    IngresoCelda,
                    null,
                    IngresoMarca,
                    IngresoModelo);

                MensajeExito = $"Vehículo {TicketGenerado.Placa} ingresado exitosamente en celda {TicketGenerado.NumeroCelda}. Ticket #{TicketGenerado.IdRegistro} generado.";

                if (IngresoSolicitarLavado)
                {
                    try
                    {
                        var s = _serviciosLavadoNegocio.SolicitarLavado(TicketGenerado.Placa, "Lavado General", 25000m);
                        MensajeExito += $" Además se programó orden de lavado #{s.IdServicio} en estado PENDIENTE.";
                    }
                    catch (Exception exLav)
                    {
                        MensajeError = $"Vehículo ingresado, pero no se pudo programar el lavado: {exLav.Message}";
                    }
                }

                IngresoPlaca = string.Empty;
                IngresoMarca = string.Empty;
                IngresoModelo = string.Empty;
                IngresoSolicitarLavado = false;
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        // --- Control de Lavado (HU-03) ---
        public IActionResult OnPostSolicitarLavado()
        {
            TabActiva = "lavado";
            try
            {
                if (string.IsNullOrWhiteSpace(LavadoPlaca))
                {
                    MensajeError = "Ingrese la placa del vehículo con ingreso activo.";
                    CargarDatos();
                    return Page();
                }

                var s = _serviciosLavadoNegocio.SolicitarLavado(LavadoPlaca.Trim().ToUpper(), LavadoTipo, LavadoPrecio);
                MensajeExito = $"Servicio de lavado #{s.IdServicio} solicitado con éxito para {s.Placa}. Estado inicial: PENDIENTE.";
                LavadoPlaca = string.Empty;
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostAvanzarLavado(int id)
        {
            TabActiva = "lavado";
            try
            {
                var s = _serviciosLavadoNegocio.AvanzarEstado(id);
                MensajeExito = $"Servicio #{s.IdServicio} ({s.Placa}) avanzado exitosamente a: {s.EstadoLavado}.";
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        // --- Caja & Liquidación (HU-04) ---
        private void ConsultarCuentaInterno(string placa)
        {
            try
            {
                CuentaConsultada = _liquidacionFacade.ConsultarCuenta(placa);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
                CuentaConsultada = null;
            }
        }

        public IActionResult OnPostConsultarCuenta(string? placa)
        {
            TabActiva = "caja";
            var target = string.IsNullOrWhiteSpace(placa) ? CajaPlaca : placa;
            if (string.IsNullOrWhiteSpace(target))
            {
                MensajeError = "Ingrese una placa para consultar.";
                CargarDatos();
                return Page();
            }

            CajaPlaca = target.Trim().ToUpper();
            ConsultarCuentaInterno(CajaPlaca);
            CargarDatos();
            return Page();
        }

        public IActionResult OnPostConfirmarPago(string placa, string metodo, decimal? valorRecibido)
        {
            TabActiva = "caja";
            CajaPlaca = placa.Trim().ToUpper();
            try
            {
                var cuenta = _liquidacionFacade.ConsultarCuenta(CajaPlaca);
                decimal montoTotal = cuenta.total;
                decimal recibido = (valorRecibido.HasValue && valorRecibido.Value > 0) ? valorRecibido.Value : montoTotal;

                var estrategia = FabricaMetodoPago.ObtenerEstrategia(metodo);
                var datos = new DatosPago { ValorRecibido = recibido };
                ComprobanteGenerado = _liquidacionFacade.Liquidar(CajaPlaca, estrategia, datos, CanalPago.CAJA);

                string detalleCambio = ComprobanteGenerado.Cambio > 0 ? $" · Cambio: ${ComprobanteGenerado.Cambio:N0}" : "";
                MensajeExito = $"Pago confirmado exitosamente. Factura #{ComprobanteGenerado.NumeroFactura} por ${ComprobanteGenerado.TotalPagar:N0} ({ComprobanteGenerado.MetodoPago}){detalleCambio}. Ya puede autorizar la salida del vehículo.";

                ConsultarCuentaInterno(CajaPlaca);
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
                ConsultarCuentaInterno(CajaPlaca);
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostAutorizarSalida(string placa)
        {
            TabActiva = "caja";
            try
            {
                _liquidacionFacade.AutorizarSalida(placa.Trim().ToUpper());
                MensajeExito = $"Salida autorizada para el vehículo {placa.ToUpper()}. Celda liberada y registro finalizado.";
                CajaPlaca = string.Empty;
                CuentaConsultada = null;
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
                ConsultarCuentaInterno(placa.Trim().ToUpper());
            }

            CargarDatos();
            return Page();
        }

        // --- Reservas (HU-02) ---
        public IActionResult OnPostCrearReserva()
        {
            TabActiva = "reservas";
            try
            {
                if (string.IsNullOrWhiteSpace(ReservaPlaca) || ReservaPlaca.Trim().Length < 6)
                {
                    MensajeError = "Ingrese una placa válida de al menos 6 caracteres.";
                    CargarDatos();
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(ReservaCelda))
                {
                    MensajeError = "Seleccione una celda para reservar.";
                    CargarDatos();
                    return Page();
                }

                var horaParts = (ReservaHora ?? "11:00").Split(':');
                int h = int.Parse(horaParts[0]);
                int m = int.Parse(horaParts[1]);
                var fechaHora = DateTime.Today.AddHours(h).AddMinutes(m);

                var r = _reservasNegocio.CrearReserva(ReservaPlaca.Trim().ToUpper(), ReservaCelda, fechaHora);
                MensajeExito = $"Reserva #{r.IdReserva} creada para {r.Placa} en celda {r.NumeroCelda}. Tolerancia: 15 minutos (Regla RN-03).";
                ReservaPlaca = string.Empty;
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostVerificarVencimientos()
        {
            TabActiva = "reservas";
            try
            {
                int canceladas = _reservasNegocio.VerificarVencimientosPorTolerancia(DateTime.Now);
                MensajeExito = $"Verificación completada: {canceladas} reservas vencidas por inasistencia canceladas automáticamente según la Regla RN-03.";
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostCancelarReserva(int id)
        {
            TabActiva = "reservas";
            try
            {
                _reservasNegocio.CancelarReserva(id, DateTime.Now);
                MensajeExito = $"Reserva #{id} cancelada y celda liberada correctamente.";
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostEliminarReserva(int id)
        {
            TabActiva = "reservas";
            try
            {
                bool eliminada = _reservasNegocio.EliminarReserva(id);
                if (eliminada)
                {
                    MensajeExito = $"Reserva #{id} eliminada correctamente de la base de datos y celda liberada.";
                }
                else
                {
                    MensajeError = $"No se encontró la reserva #{id}.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        // --- Acciones de Cliente ---
        public IActionResult OnPostSolicitarLavadoCliente(string placa)
        {
            TabActiva = "c-lavado";
            try
            {
                if (string.IsNullOrWhiteSpace(placa))
                {
                    MensajeError = "Seleccione un vehículo para el servicio de lavado.";
                    CargarDatos();
                    return Page();
                }

                var s = _serviciosLavadoNegocio.SolicitarLavado(placa.Trim().ToUpper(), "Lavado General", 25000m);
                MensajeExito = $"Servicio de lavado #{s.IdServicio} solicitado con éxito para tu vehículo {s.Placa}. Estado inicial: PENDIENTE.";
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostCrearReservaCliente(string placa, string celda, string hora)
        {
            TabActiva = "c-reservas";
            try
            {
                if (string.IsNullOrWhiteSpace(placa) || string.IsNullOrWhiteSpace(celda))
                {
                    MensajeError = "Debe indicar el vehículo y la celda a reservar.";
                    CargarDatos();
                    return Page();
                }

                var horaParts = (hora ?? "14:00").Split(':');
                int h = int.Parse(horaParts[0]);
                int m = int.Parse(horaParts[1]);
                var fechaHora = DateTime.Today.AddHours(h).AddMinutes(m);

                int? idUser = ClienteActual?.IdUsuario;
                var r = _reservasNegocio.CrearReserva(placa.Trim().ToUpper(), celda, fechaHora, idUser);
                MensajeExito = $"Reserva #{r.IdReserva} confirmada para {r.Placa} en celda {r.NumeroCelda}. Tolerancia de llegada: 15 minutos.";
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostEliminarReservaCliente(int id)
        {
            TabActiva = "c-reservas";
            try
            {
                bool ok = _reservasNegocio.EliminarReserva(id);
                if (ok)
                {
                    MensajeExito = $"Tu reserva #{id} ha sido cancelada y eliminada.";
                }
                else
                {
                    MensajeError = $"No se encontró la reserva #{id}.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostRegistrarVehiculoCliente()
        {
            TabActiva = "c-panel";
            try
            {
                if (string.IsNullOrWhiteSpace(NuevoVehiculoPlaca) || NuevoVehiculoPlaca.Trim().Length < 6)
                {
                    MensajeError = "Por favor ingrese un número de placa válido de al menos 6 caracteres.";
                    CargarDatos();
                    return Page();
                }

                string codigo = HttpContext.Session.GetString("UsuarioCodigo") ?? "CL-5001";
                var cliente = _usuariosNegocio.BuscarPorCodigo(codigo);
                if (cliente == null)
                {
                    MensajeError = "No se pudo identificar la cuenta del cliente actual.";
                    CargarDatos();
                    return Page();
                }

                var vehiculo = new Vehiculo
                {
                    Placa = NuevoVehiculoPlaca.Trim().ToUpper(),
                    TipoVehiculo = string.IsNullOrWhiteSpace(NuevoVehiculoTipo) ? "Automóvil" : NuevoVehiculoTipo,
                    Marca = string.IsNullOrWhiteSpace(NuevoVehiculoMarca) ? null : NuevoVehiculoMarca.Trim(),
                    Modelo = string.IsNullOrWhiteSpace(NuevoVehiculoModelo) ? null : NuevoVehiculoModelo.Trim(),
                    IdUsuarioPropietario = cliente.IdUsuario
                };

                _vehiculosNegocio.Guardar(vehiculo);
                MensajeExito = $"Vehículo {vehiculo.Placa} ({vehiculo.Marca} {vehiculo.Modelo}) vinculado exitosamente a tu cuenta.";
                NuevoVehiculoPlaca = string.Empty;
                NuevoVehiculoMarca = string.Empty;
                NuevoVehiculoModelo = string.Empty;
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }

        public IActionResult OnPostPagarEnLineaCliente(string placa, string metodo)
        {
            TabActiva = "c-cuenta";
            try
            {
                var cuenta = _liquidacionFacade.ConsultarCuenta(placa.Trim().ToUpper());
                var estrategia = FabricaMetodoPago.ObtenerEstrategia(metodo);
                var datos = new DatosPago { ValorRecibido = cuenta.total };
                var comp = _liquidacionFacade.Liquidar(placa.Trim().ToUpper(), estrategia, datos, CanalPago.EN_LINEA);
                MensajeExito = $"Pago en línea procesado exitosamente. Factura #{comp.NumeroFactura} por ${comp.TotalPagar:N0} ({comp.MetodoPago}). Ya puedes retirar tu vehículo; el operador autorizará tu salida en portería.";
            }
            catch (Exception ex)
            {
                MensajeError = ex.Message;
            }

            CargarDatos();
            return Page();
        }
    }
}
