using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;

namespace LibPresentaciones.InterfacesPresentaciones
{
    public interface ICeldasNegocioPresentacion
    {
        List<Celda> Consultar();
        List<Celda> ConsultarDisponibles();
        Celda? BuscarPorNumero(string numeroCelda);
        ContadoresCeldasDto ObtenerContadores();
        Celda CambiarEstado(string numeroCelda, EstadoCelda nuevoEstado);
    }
}
