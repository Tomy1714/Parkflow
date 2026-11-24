using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.StateLavado
{
    public class EstadoEnLavado : IEstadoServicioLavado
    {
        public EstadoLavado EstadoEnum => EstadoLavado.EN_LAVADO;

        public void Avanzar(ServicioLavado servicio, DateTime ahora)
        {
            servicio.FechaHoraFin = ahora;
            servicio.CambiarEstado(new EstadoListo());
            // Notifica a caja que el servicio está listo
            servicio.NotificarListoParaCobro();
        }

        public bool PuedeCobrarse() => false;
    }
}
