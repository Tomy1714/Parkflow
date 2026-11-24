using LibParkFlow.Entidades;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class ReservasNegocioPresentacion : IReservasNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public List<Reserva> Consultar()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/reservas",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Reserva>();

            return JsonConvert.DeserializeObject<List<Reserva>>(respuesta["Valor"].ToString()!) ?? new List<Reserva>();
        }

        public List<Reserva> ConsultarActivas()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/reservas/activas",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Reserva>();

            return JsonConvert.DeserializeObject<List<Reserva>>(respuesta["Valor"].ToString()!) ?? new List<Reserva>();
        }

        public List<Reserva> ConsultarPorUsuario(int idUsuario)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/reservas/cliente/{idUsuario}",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Reserva>();

            return JsonConvert.DeserializeObject<List<Reserva>>(respuesta["Valor"].ToString()!) ?? new List<Reserva>();
        }

        public List<AuditoriaReserva> ConsultarHistorialAuditoria()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/reservas/auditoria",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<AuditoriaReserva>();

            return JsonConvert.DeserializeObject<List<AuditoriaReserva>>(respuesta["Valor"].ToString()!) ?? new List<AuditoriaReserva>();
        }

        public Reserva CrearReserva(string placa, string numeroCelda, DateTime horaReserva, int? idUsuario = null)
        {
            var entidad = new
            {
                Placa = placa,
                NumeroCelda = numeroCelda,
                HoraReserva = horaReserva,
                IdUsuario = idUsuario
            };

            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/reservas",
                ["Metodo"] = "POST",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                throw new Exception("Error al crear reserva");

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            if (dict != null && dict.ContainsKey("reserva"))
            {
                return JsonConvert.DeserializeObject<Reserva>(dict["reserva"].ToString()!) ?? new Reserva();
            }

            throw new Exception("No se pudo obtener la confirmación de la reserva");
        }

        public void CancelarReserva(int idReserva, DateTime? ahora = null)
        {
            var entidad = new { Ahora = ahora };
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/reservas/{idReserva}/cancelar",
                ["Metodo"] = "POST",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
        }

        public int VerificarVencimientosPorTolerancia(DateTime ahora)
        {
            var entidad = new { Ahora = ahora };
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/reservas/verificar-vencimientos",
                ["Metodo"] = "POST",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return 0;

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            if (dict != null && dict.ContainsKey("canceladas"))
            {
                return Convert.ToInt32(dict["canceladas"]);
            }
            return 0;
        }
    }
}
