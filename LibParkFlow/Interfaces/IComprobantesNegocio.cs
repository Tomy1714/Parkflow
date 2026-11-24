using LibParkFlow.Entidades;

namespace LibParkFlow.Interfaces
{
    public interface IComprobantesNegocio
    {
        List<ComprobantePago> Consultar();
        List<ComprobantePago> ConsultarPorPlaca(string placa);
        ComprobantePago? BuscarPorId(int idFactura);
        ComprobantePago? BuscarPorNumero(string numeroFactura);
    }
}
