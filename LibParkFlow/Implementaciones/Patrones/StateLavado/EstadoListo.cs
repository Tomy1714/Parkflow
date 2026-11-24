using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.StateLavado
{
    public class EstadoListo : IEstadoServicioLavado
    {
        public EstadoLavado EstadoEnum => EstadoLavado.LISTO;

        public void Avanzar(ServicioLavado servicio, DateTime ahora)
        {
            throw new InvalidOperationException("El servicio de lavado ya se encuentra en estado Listo. No admite avances posteriores.");
        }

        public bool PuedeCobrarse() => true;
    }
}
