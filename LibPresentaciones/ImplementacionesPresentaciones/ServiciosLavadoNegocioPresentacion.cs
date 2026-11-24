using LibParkFlow.Entidades;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class ServiciosLavadoNegocioPresentacion : IServiciosLavadoNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public List<ServicioLavado> Consultar()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/lavado",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<ServicioLavado>();

            return JsonConvert.DeserializeObject<List<ServicioLavado>>(respuesta["Valor"].ToString()!) ?? new List<ServicioLavado>();
        }

        public List<ServicioLavado> ConsultarPorPlaca(string placa)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/lavado/placa/{placa}",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<ServicioLavado>();

            return JsonConvert.DeserializeObject<List<ServicioLavado>>(respuesta["Valor"].ToString()!) ?? new List<ServicioLavado>();
        }

        public ServicioLavado? BuscarPorId(int idServicio)
        {
            var lavados = Consultar();
            return lavados.FirstOrDefault(s => s.IdServicio == idServicio);
        }

        public ServicioLavado SolicitarLavado(string placa, string tipoLavado = "Lavado General", decimal precio = 25000m, DateTime? ahora = null)
        {
            var entidad = new
            {
                Placa = placa,
                TipoLavado = tipoLavado,
                Precio = precio,
                Ahora = ahora
            };

            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/lavado/solicitar",
                ["Metodo"] = "POST",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                throw new Exception("Error al solicitar lavado");

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            if (dict != null && dict.ContainsKey("servicio"))
            {
                return JsonConvert.DeserializeObject<ServicioLavado>(dict["servicio"].ToString()!) ?? new ServicioLavado();
            }

            throw new Exception("No se pudo confirmar la solicitud de lavado");
        }

        public ServicioLavado AvanzarEstado(int idServicio, DateTime? ahora = null)
        {
            var entidad = new { Ahora = ahora };
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/lavado/{idServicio}/avanzar",
                ["Metodo"] = "POST",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                throw new Exception("Error al avanzar el estado del lavado");

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            if (dict != null && dict.ContainsKey("servicio"))
            {
                return JsonConvert.DeserializeObject<ServicioLavado>(dict["servicio"].ToString()!) ?? new ServicioLavado();
            }

            throw new Exception("No se pudo avanzar el estado");
        }
    }
}
