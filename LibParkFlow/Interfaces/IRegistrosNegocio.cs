using LibParkFlow.Entidades;

namespace LibParkFlow.Interfaces
{
    public class TicketIngresoDto
    {
        public int IdRegistro { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string NumeroCelda { get; set; } = string.Empty;
        public string TipoVehiculo { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? Modelo { get; set; }
        public DateTime FechaHoraEntrada { get; set; }
        public string EstadoCelda { get; set; } = "Ocupada";
        public string? MensajeReserva { get; set; }
    }

    public interface IRegistrosNegocio
    {
        List<RegistroParqueadero> Consultar();
        List<RegistroParqueadero> ConsultarActivos();
        RegistroParqueadero? BuscarPorPlaca(string placa);
        TicketIngresoDto RegistrarIngreso(string placa, string tipoVehiculo, string? numeroCelda = null, DateTime? ahora = null, string? marca = null, string? modelo = null);
    }
}
