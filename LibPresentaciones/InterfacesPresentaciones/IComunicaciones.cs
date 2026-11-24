namespace LibPresentaciones.InterfacesPresentaciones
{
    public interface IComunicaciones
    {
        Task<Dictionary<string, object>> Ejecutar(Dictionary<string, object> datos);
    }
}
