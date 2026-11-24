using LibParkFlow.Entidades;

namespace LibParkFlow.Interfaces
{
    public interface ILiquidacionFacade
    {
        (RegistroParqueadero registro, decimal parqueadero, List<ServicioLavado> facturables, List<ServicioLavado> enCurso, decimal total) ConsultarCuenta(string placa, DateTime? ahora = null);

        ComprobantePago Liquidar(string placa, IMetodoPagoStrategy metodo, DatosPago? datos, CanalPago canal, int? idCaja = null, DateTime? ahora = null);

        void AutorizarSalida(string placa, DateTime? ahora = null);
    }
}
