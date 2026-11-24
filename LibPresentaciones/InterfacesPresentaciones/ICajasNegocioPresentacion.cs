using LibParkFlow.Entidades;

namespace LibPresentaciones.InterfacesPresentaciones
{
    public interface ICajasNegocioPresentacion
    {
        List<Caja> Consultar();
        Caja? ObtenerCajaActiva();
        Caja AbrirCaja(int idUsuario, decimal montoInicial, DateTime? ahora = null);
        Caja CerrarCaja(int idCaja);
    }
}
