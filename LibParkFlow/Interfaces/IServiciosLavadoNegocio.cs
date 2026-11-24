using LibParkFlow.Entidades;

namespace LibParkFlow.Interfaces
{
    public interface IServiciosLavadoNegocio
    {
        List<ServicioLavado> Consultar();
        List<ServicioLavado> ConsultarPorPlaca(string placa);
        ServicioLavado? BuscarPorId(int idServicio);
        ServicioLavado SolicitarLavado(string placa, string tipoLavado = "Lavado General", decimal precio = 25000m, DateTime? ahora = null);
        ServicioLavado AvanzarEstado(int idServicio, DateTime? ahora = null);
    }
}
