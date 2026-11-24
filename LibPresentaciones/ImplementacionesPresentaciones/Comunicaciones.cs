using System.Text;
using LibPresentaciones.InterfacesPresentaciones;
using Newtonsoft.Json;

namespace LibPresentaciones.ImplementacionesPresentaciones
{
    public class Comunicaciones : IComunicaciones
    {
        public async Task<Dictionary<string, object>> Ejecutar(Dictionary<string, object> datos)
        {
            var url = datos["Url"].ToString()!;
            var metodo = datos["Metodo"].ToString()!.ToUpper();

            datos.Remove("Url");
            datos.Remove("Metodo");

            var stringData = datos.ContainsKey("Entidad") ?
                JsonConvert.SerializeObject(datos["Entidad"]) : "{}";

            var body = new StringContent(stringData, Encoding.UTF8, "application/json");

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(2);

            HttpResponseMessage? message = null;

            if (metodo == "GET")
            {
                message = await httpClient.GetAsync(url);
            }
            else if (metodo == "POST")
            {
                message = await httpClient.PostAsync(url, body);
            }
            else if (metodo == "PUT")
            {
                message = await httpClient.PutAsync(url, body);
            }
            else if (metodo == "DELETE")
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url) { Content = body };
                message = await httpClient.SendAsync(request);
            }
            else
            {
                throw new NotSupportedException($"Método HTTP '{metodo}' no soportado.");
            }

            var resp = await message.Content.ReadAsStringAsync();

            if (!message.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error de comunicación con API ({message.StatusCode}): {resp}");
            }

            return new Dictionary<string, object>
            {
                { "Valor", resp }
            };
        }
    }
}
