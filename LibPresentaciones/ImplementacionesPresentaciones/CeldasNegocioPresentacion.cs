using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class CeldasNegocioPresentacion : ICeldasNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public List<Celda> Consultar()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/celdas",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Celda>();

            return JsonConvert.DeserializeObject<List<Celda>>(respuesta["Valor"].ToString()!) ?? new List<Celda>();
        }

        public List<Celda> ConsultarDisponibles()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/celdas/disponibles",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Celda>();

            return JsonConvert.DeserializeObject<List<Celda>>(respuesta["Valor"].ToString()!) ?? new List<Celda>();
        }

        public Celda? BuscarPorNumero(string numeroCelda)
        {
            var celdas = Consultar();
            return celdas.FirstOrDefault(c => string.Equals(c.NumeroCelda, numeroCelda, StringComparison.OrdinalIgnoreCase));
        }

        public ContadoresCeldasDto ObtenerContadores()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/celdas/contadores",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new ContadoresCeldasDto();

            return JsonConvert.DeserializeObject<ContadoresCeldasDto>(respuesta["Valor"].ToString()!) ?? new ContadoresCeldasDto();
        }

        public Celda CambiarEstado(string numeroCelda, EstadoCelda nuevoEstado)
        {
            var entidad = new { Estado = nuevoEstado };
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/celdas/{numeroCelda}/estado",
                ["Metodo"] = "PUT",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new Celda { NumeroCelda = numeroCelda, Estado = nuevoEstado };

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            if (dict != null && dict.ContainsKey("celda"))
            {
                return JsonConvert.DeserializeObject<Celda>(dict["celda"].ToString()!) ?? new Celda { NumeroCelda = numeroCelda };
            }

            return new Celda { NumeroCelda = numeroCelda, Estado = nuevoEstado };
        }
    }
}
