using LibParkFlow.Entidades;

namespace LibPresentaciones.InterfacesPresentaciones
{
    public interface IComprobantesNegocioPresentacion
    {
        List<ComprobantePago> Consultar();
        List<ComprobantePago> ConsultarPorPlaca(string placa);
        ComprobantePago? BuscarPorId(int idFactura);
        ComprobantePago? BuscarPorNumero(string numeroFactura);
    }
}
