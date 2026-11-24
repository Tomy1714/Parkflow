using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;

namespace LibPresentaciones.InterfacesPresentaciones
{
    public interface IUsuariosNegocioPresentacion
    {
        List<Usuario> Consultar();
        Usuario? BuscarPorId(int id);
        Usuario? BuscarPorCodigo(string codigo);
        RespuestaAutenticacion IniciarSesion(string codigo, string contrasena, DateTime? ahora = null);
        Usuario Guardar(Usuario entidad);
    }
}
