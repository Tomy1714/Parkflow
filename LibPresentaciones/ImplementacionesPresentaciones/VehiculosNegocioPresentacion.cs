using LibParkFlow.Entidades;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class VehiculosNegocioPresentacion : IVehiculosNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public List<Vehiculo> Consultar()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/vehiculos",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Vehiculo>();

            return JsonConvert.DeserializeObject<List<Vehiculo>>(respuesta["Valor"].ToString()!) ?? new List<Vehiculo>();
        }

        public List<Vehiculo> ConsultarPorUsuario(int idUsuario)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/vehiculos/cliente/{idUsuario}",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Vehiculo>();

            return JsonConvert.DeserializeObject<List<Vehiculo>>(respuesta["Valor"].ToString()!) ?? new List<Vehiculo>();
        }

        public Vehiculo? BuscarPorPlaca(string placa)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/vehiculos/{placa}",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return null;

            return JsonConvert.DeserializeObject<Vehiculo>(respuesta["Valor"].ToString()!);
        }

        public Vehiculo Guardar(Vehiculo vehiculo)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/vehiculos",
                ["Metodo"] = "POST",
                ["Entidad"] = vehiculo
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return vehiculo;

            var resObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            if (resObj != null && resObj.ContainsKey("vehiculo"))
            {
                return JsonConvert.DeserializeObject<Vehiculo>(resObj["vehiculo"].ToString()!) ?? vehiculo;
            }

            return vehiculo;
        }

        public Vehiculo RegistrarVehiculoCliente(string placa, int idUsuario, string tipoVehiculo = "Automóvil")
        {
            var entidad = new
            {
                Placa = placa,
                IdUsuario = idUsuario,
                TipoVehiculo = tipoVehiculo
            };

            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/vehiculos/cliente",
                ["Metodo"] = "POST",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new Vehiculo { Placa = placa, IdUsuarioPropietario = idUsuario, TipoVehiculo = tipoVehiculo };

            var resObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            if (resObj != null && resObj.ContainsKey("vehiculo"))
            {
                return JsonConvert.DeserializeObject<Vehiculo>(resObj["vehiculo"].ToString()!) ?? new Vehiculo { Placa = placa };
            }

            return new Vehiculo { Placa = placa, IdUsuarioPropietario = idUsuario, TipoVehiculo = tipoVehiculo };
        }

        public bool Borrar(string placa)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/vehiculos/{placa}",
                ["Metodo"] = "DELETE"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            return respuesta.ContainsKey("Valor");
        }
    }
}
