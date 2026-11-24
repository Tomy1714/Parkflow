using Newtonsoft.Json;

namespace LibParkFlow.Nucleo
{
    public class Configuraciones
    {
        public static string CadenaConexionPorDefecto = Environment.GetEnvironmentVariable("PARKFLOW_CONNECTION_STRING")
            ?? "server=TOMAS-PC\\DEV;database=ParkFlow;Integrated Security=True;TrustServerCertificate=true;";

        public static string Obtener(string clave)
        {
            if (clave == "string_conexion")
            {
                return CadenaConexionPorDefecto;
            }
            return CadenaConexionPorDefecto;
        }

        public static string ConvertirAString(object data, bool ignore = false)
        {
            if (!ignore)
                return JsonConvert.SerializeObject(data);

            return JsonConvert.SerializeObject(data, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }
            );
        }
    }
}
