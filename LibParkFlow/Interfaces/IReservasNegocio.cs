using LibParkFlow.Entidades;

namespace LibParkFlow.Interfaces
{
    public interface IReservasNegocio
    {
        List<Reserva> Consultar();
        List<Reserva> ConsultarActivas();
        List<Reserva> ConsultarPorUsuario(int idUsuario);
        List<AuditoriaReserva> ConsultarHistorialAuditoria();
        Reserva CrearReserva(string placa, string numeroCelda, DateTime horaReserva, int? idUsuario = null);
        void CancelarReserva(int idReserva, DateTime? ahora = null);
        bool EliminarReserva(int idReserva);
        int VerificarVencimientosPorTolerancia(DateTime ahora);
    }
}
