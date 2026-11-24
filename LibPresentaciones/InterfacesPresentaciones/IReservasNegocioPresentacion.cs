using LibParkFlow.Entidades;

namespace LibPresentaciones.InterfacesPresentaciones
{
    public interface IReservasNegocioPresentacion
    {
        List<Reserva> Consultar();
        List<Reserva> ConsultarActivas();
        List<Reserva> ConsultarPorUsuario(int idUsuario);
        List<AuditoriaReserva> ConsultarHistorialAuditoria();
        Reserva CrearReserva(string placa, string numeroCelda, DateTime horaReserva, int? idUsuario = null);
        void CancelarReserva(int idReserva, DateTime? ahora = null);
        int VerificarVencimientosPorTolerancia(DateTime ahora);
    }
}
