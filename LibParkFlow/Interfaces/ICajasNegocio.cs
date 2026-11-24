using LibParkFlow.Entidades;

namespace LibParkFlow.Interfaces
{
    public interface ICajasNegocio
    {
        List<Caja> Consultar();
        Caja? ObtenerCajaActiva();
        Caja AbrirCaja(int idUsuario, decimal montoInicial, DateTime? ahora = null);
        Caja CerrarCaja(int idCaja);
    }
}
