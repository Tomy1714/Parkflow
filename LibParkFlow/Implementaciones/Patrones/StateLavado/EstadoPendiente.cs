using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.StateLavado
{
    public class EstadoPendiente : IEstadoServicioLavado
    {
        public EstadoLavado EstadoEnum => EstadoLavado.PENDIENTE;

        public void Avanzar(ServicioLavado servicio, DateTime ahora)
        {
            servicio.FechaHoraInicio = ahora;
            servicio.CambiarEstado(new EstadoEnLavado());
        }

        public bool PuedeCobrarse() => false;
    }
}
