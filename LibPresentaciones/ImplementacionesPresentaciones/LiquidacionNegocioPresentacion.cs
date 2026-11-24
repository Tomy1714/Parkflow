using LibParkFlow.Entidades;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class LiquidacionNegocioPresentacion : ILiquidacionNegocioPresentacion
    {
        private IComunicaciones? _comunicaciones;

        public ResumenCuentaPresentacionDto ConsultarCuenta(string placa, DateTime? ahora = null)
        {
            var url = $"{ConfiguracionPresentacion.UrlBase}/liquidacion/cuenta/{placa}";
            if (ahora.HasValue)
            {
                url += $"?ahora={Uri.EscapeDataString(ahora.Value.ToString("o"))}";
            }

            var datos = new Dictionary<string, object>
            {
                ["Url"] = url,
                ["Metodo"] = "GET"
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            var val = respuesta["Valor"]?.ToString();
            if (string.IsNullOrWhiteSpace(val))
                throw new Exception("No se recibió respuesta al consultar la cuenta.");

            return JsonConvert.DeserializeObject<ResumenCuentaPresentacionDto>(val)!;
        }

        public ComprobantePago Liquidar(LiquidarPresentacionDto datosCobro)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/liquidacion/liquidar",
                ["Metodo"] = "POST",
                ["Entidad"] = datosCobro
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            var val = respuesta["Valor"]?.ToString();
            if (string.IsNullOrWhiteSpace(val))
                throw new Exception("No se recibió respuesta al procesar la liquidación.");

            var json = JObject.Parse(val);
            var compToken = json["comprobante"];
            if (compToken == null)
                throw new Exception("El servidor no devolvió el comprobante de pago generado.");

            return compToken.ToObject<ComprobantePago>()!;
        }

        public bool AutorizarSalida(string placa, DateTime? ahora = null)
        {
            var datos = new Dictionary<string, object>
            {
                ["Url"] = $"{ConfiguracionPresentacion.UrlBase}/liquidacion/autorizar-salida",
                ["Metodo"] = "POST",
                ["Entidad"] = new { Placa = placa, Ahora = ahora }
            };

            _comunicaciones = new Comunicaciones();
            var task = _comunicaciones.Ejecutar(datos);
            task.Wait();
            var respuesta = task.Result;

            var val = respuesta["Valor"]?.ToString();
            if (string.IsNullOrWhiteSpace(val))
                throw new Exception("No se recibió respuesta al autorizar la salida.");

            return true;
        }
    }
}
