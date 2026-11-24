using LibParkFlow.Entidades;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class CajasNegocioPresentacion : ICajasNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public List<Caja> Consultar()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/cajas",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Caja>();

            return JsonConvert.DeserializeObject<List<Caja>>(respuesta["Valor"].ToString()!) ?? new List<Caja>();
        }

        public Caja? ObtenerCajaActiva()
        {
            try
            {
                var datos = new Dictionary<string, object>
                {
                    ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/cajas/activa",
                    ["Metodo"] = "GET"
                };

                _comunicaciones = new Comunicaciones();
                var task = _comunicaciones.Ejecutar(datos);
                task.Wait();
                var respuesta = task.Result;

                if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                    return null;

                return JsonConvert.DeserializeObject<Caja>(respuesta["Valor"].ToString()!);
            }
            catch
            {
                return null;
            }
        }

        public Caja AbrirCaja(int idUsuario, decimal montoInicial, DateTime? ahora = null)
        {
            var payload = new
            {
                IdUsuario = idUsuario,
                MontoInicial = montoInicial,
                Ahora = ahora
            };

            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/cajas/abrir",
                ["Metodo"] = "POST",
                ["Entidad"] = payload
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            var json = JObject.Parse(respuesta["Valor"].ToString()!);
            var cajaToken = json["caja"];
            if (cajaToken == null) throw new Exception("No se recibió la caja creada desde el servidor.");
            return cajaToken.ToObject<Caja>()!;
        }

        public Caja CerrarCaja(int idCaja)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/cajas/{idCaja}/cerrar",
                ["Metodo"] = "POST"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            var json = JObject.Parse(respuesta["Valor"].ToString()!);
            var cajaToken = json["caja"];
            if (cajaToken == null) throw new Exception("No se pudo cerrar la caja.");
            return cajaToken.ToObject<Caja>()!;
        }
    }
}
