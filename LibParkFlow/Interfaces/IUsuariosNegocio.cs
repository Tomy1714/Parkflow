using LibParkFlow.Entidades;

namespace LibParkFlow.Interfaces
{
    public class RespuestaAutenticacion
    {
        public bool Autenticado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public Usuario? Usuario { get; set; }
        public Rol? RolActivo { get; set; }
        public List<string> OperacionesPermitidas { get; set; } = new();
    }

    public interface IUsuariosNegocio
    {
        List<Usuario> Consultar();
        Usuario? BuscarPorId(int id);
        Usuario? BuscarPorCodigo(string codigo);
        RespuestaAutenticacion IniciarSesion(string codigo, string contrasena, DateTime? ahora = null);
        Usuario Guardar(Usuario entidad);
    }
}
