using LibParkFlow.Entidades;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class ComprobantesNegocioPresentacion : IComprobantesNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public List<ComprobantePago> Consultar()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/comprobantes",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<ComprobantePago>();

            return JsonConvert.DeserializeObject<List<ComprobantePago>>(respuesta["Valor"].ToString()!) ?? new List<ComprobantePago>();
        }

        public List<ComprobantePago> ConsultarPorPlaca(string placa)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/comprobantes/placa/{placa}",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<ComprobantePago>();

            return JsonConvert.DeserializeObject<List<ComprobantePago>>(respuesta["Valor"].ToString()!) ?? new List<ComprobantePago>();
        }

        public ComprobantePago? BuscarPorId(int idFactura)
        {
            try
            {
                var datos = new Dictionary<string, object>
                {
                    ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/comprobantes/{idFactura}",
                    ["Metodo"] = "GET"
                };

                _comunicaciones = new Comunicaciones();
                var task = _comunicaciones.Ejecutar(datos);
                task.Wait();
                var respuesta = task.Result;

                if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                    return null;

                return JsonConvert.DeserializeObject<ComprobantePago>(respuesta["Valor"].ToString()!);
            }
            catch
            {
                return null;
            }
        }

        public ComprobantePago? BuscarPorNumero(string numeroFactura)
        {
            try
            {
                var datos = new Dictionary<string, object>
                {
                    ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/comprobantes/numero/{numeroFactura}",
                    ["Metodo"] = "GET"
                };

                _comunicaciones = new Comunicaciones();
                var task = _comunicaciones.Ejecutar(datos);
                task.Wait();
                var respuesta = task.Result;

                if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                    return null;

                return JsonConvert.DeserializeObject<ComprobantePago>(respuesta["Valor"].ToString()!);
            }
            catch
            {
                return null;
            }
        }
    }
}
