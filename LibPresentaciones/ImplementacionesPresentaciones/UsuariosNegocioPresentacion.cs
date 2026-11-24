using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class UsuariosNegocioPresentacion : IUsuariosNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public List<Usuario> Consultar()
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/usuarios",
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return new List<Usuario>();

            return JsonConvert.DeserializeObject<List<Usuario>>(respuesta["Valor"].ToString()!) ?? new List<Usuario>();
        }

        public Usuario? BuscarPorId(int id)
        {
            var lista = Consultar();
            return lista.FirstOrDefault(u => u.IdUsuario == id);
        }

        public Usuario? BuscarPorCodigo(string codigo)
        {
            var lista = Consultar();
            return lista.FirstOrDefault(u => string.Equals(u.CodigoOperativo, codigo, StringComparison.OrdinalIgnoreCase));
        }

        public RespuestaAutenticacion IniciarSesion(string codigo, string contrasena, DateTime? ahora = null)
        {
            var payload = new
            {
                Codigo = codigo,
                Clave = contrasena,
                FechaSimulada = ahora
            };

            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/autenticacion/login",
                ["Metodo"] = "POST",
                ["Entidad"] = payload
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
            {
                return new RespuestaAutenticacion { Autenticado = false, Mensaje = "Error de respuesta del servidor" };
            }

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(respuesta["Valor"].ToString()!);
            bool ok = dict != null && dict.ContainsKey("ok") && Convert.ToBoolean(dict["ok"]);

            var res = new RespuestaAutenticacion
            {
                Autenticado = ok,
                Mensaje = dict?.ContainsKey("mensaje") == true ? dict["mensaje"].ToString()! : (ok ? "OK" : "Fallo de autenticación")
            };

            if (ok && dict!.ContainsKey("usuario"))
            {
                res.Usuario = JsonConvert.DeserializeObject<Usuario>(dict["usuario"].ToString()!);
            }

            if (ok && dict!.ContainsKey("operaciones"))
            {
                res.OperacionesPermitidas = JsonConvert.DeserializeObject<List<string>>(dict["operaciones"].ToString()!) ?? new();
            }

            return res;
        }

        public Usuario Guardar(Usuario entidad)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/usuarios",
                ["Metodo"] = "POST",
                ["Entidad"] = entidad
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            if (!respuesta.ContainsKey("Valor") || string.IsNullOrWhiteSpace(respuesta["Valor"]?.ToString()))
                return entidad;

            return JsonConvert.DeserializeObject<Usuario>(respuesta["Valor"].ToString()!) ?? entidad;
        }
    }
}
