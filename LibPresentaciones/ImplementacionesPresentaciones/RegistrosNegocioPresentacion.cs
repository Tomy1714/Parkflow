using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class RegistrosNegocioPresentacion : IRegistrosNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public List<RegistroParqueadero> Consultar()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/ingreso/ultimos",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<RegistroParqueadero>();

            return JsonConvert.DeserializeObject<List<RegistroParqueadero>>(respuesta["Valor"].ToString()!) ?? new List<RegistroParqueadero>();
        }

        public List<RegistroParqueadero> ConsultarActivos()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/ingreso/activos",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<RegistroParqueadero>();

            return JsonConvert.DeserializeObject<List<RegistroParqueadero>>(respuesta["Valor"].ToString()!) ?? new List<RegistroParqueadero>();
        }

        public RegistroParqueadero? BuscarPorPlaca(string placa)
        {
            var activos = ConsultarActivos();
            return activos.FirstOrDefault(r => string.Equals(r.Placa, placa, StringComparison.OrdinalIgnoreCase));
        }

        public TicketIngresoDto RegistrarIngreso(string placa, string tipoVehiculo, string? numeroCelda = null, DateTime? ahora = null, string? marca = null, string? modelo = null)
        {
            var entidad = new
            {
                Placa = placa,
                TipoVehiculo = tipoVehiculo,
                Marca = marca,
                Modelo = modelo,
                NumeroCelda = numeroCelda,
                Ahora = ahora
            };

            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/ingreso/registrar",
                ["Metodo"] = "POST",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                throw new Exception("Error al procesar el ingreso en el servidor");

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            if (dict != null && dict.ContainsKey("ticket"))
            {
                return JsonConvert.DeserializeObject<TicketIngresoDto>(dict["ticket"].ToString()!) ?? new TicketIngresoDto();
            }

            throw new Exception("No se pudo obtener el ticket de ingreso");
        }
    }
}
