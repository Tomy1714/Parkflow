using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.StateLavado
{
    public static class FabricaEstadoLavado
    {
        public static IEstadoServicioLavado ObtenerEstado(EstadoLavado estado)
        {
            return estado switch
            {
                EstadoLavado.PENDIENTE => new EstadoPendiente(),
                EstadoLavado.EN_LAVADO => new EstadoEnLavado(),
                EstadoLavado.LISTO => new EstadoListo(),
                _ => new EstadoPendiente()
            };
        }
    }
}
