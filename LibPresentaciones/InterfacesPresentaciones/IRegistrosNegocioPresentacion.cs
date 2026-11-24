using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;

namespace LibPresentaciones.InterfacesPresentaciones
{
    public interface IRegistrosNegocioPresentacion
    {
        List<RegistroParqueadero> Consultar();
        List<RegistroParqueadero> ConsultarActivos();
        RegistroParqueadero? BuscarPorPlaca(string placa);
        TicketIngresoDto RegistrarIngreso(string placa, string tipoVehiculo, string? numeroCelda = null, DateTime? ahora = null, string? marca = null, string? modelo = null);
    }
}
