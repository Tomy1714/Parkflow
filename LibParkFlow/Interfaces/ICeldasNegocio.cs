using LibParkFlow.Entidades;

namespace LibParkFlow.Interfaces
{
    public class ContadoresCeldasDto
    {
        public int Total { get; set; }
        public int Disponibles { get; set; }
        public int Ocupadas { get; set; }
        public int Reservadas { get; set; }
        public int Mantenimiento { get; set; }
    }

    public interface ICeldasNegocio
    {
        List<Celda> Consultar();
        List<Celda> ConsultarDisponibles();
        Celda? BuscarPorNumero(string numeroCelda);
        ContadoresCeldasDto ObtenerContadores();
        Celda CambiarEstado(string numeroCelda, EstadoCelda nuevoEstado);
    }
}
