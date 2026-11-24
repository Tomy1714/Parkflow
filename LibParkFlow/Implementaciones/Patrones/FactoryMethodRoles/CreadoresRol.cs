using LibParkFlow.Entidades;

namespace LibParkFlow.Implementaciones.Patrones.FactoryMethodRoles
{
    /// <summary>
    /// Creador abstracto del patrón Factory Method
    /// </summary>
    public abstract class CreadorRol
    {
        public abstract Rol CrearRol();
    }

    public class CreadorRolAdministrador : CreadorRol
    {
        public override Rol CrearRol() => new RolAdministrador();
    }

    public class CreadorRolOperador : CreadorRol
    {
        public override Rol CrearRol() => new RolOperador();
    }

    public class CreadorRolCajero : CreadorRol
    {
        public override Rol CrearRol() => new RolCajero();
    }

    public class CreadorRolLavador : CreadorRol
    {
        public override Rol CrearRol() => new RolLavador();
    }

    public class CreadorRolCliente : CreadorRol
    {
        public override Rol CrearRol() => new RolCliente();
    }

    public static class FabricaRolResolutor
    {
        private static readonly Dictionary<RolUsuario, CreadorRol> _creadores = new()
        {
            { RolUsuario.ADMINISTRADOR, new CreadorRolAdministrador() },
            { RolUsuario.OPERADOR, new CreadorRolOperador() },
            { RolUsuario.CAJERO, new CreadorRolCajero() },
            { RolUsuario.LAVADOR, new CreadorRolLavador() },
            { RolUsuario.CLIENTE, new CreadorRolCliente() }
        };

        public static CreadorRol ObtenerCreador(RolUsuario rol)
        {
            if (_creadores.TryGetValue(rol, out var creador))
            {
                return creador;
            }
            return new CreadorRolOperador();
        }

        public static Rol CrearRol(RolUsuario rol)
        {
            return ObtenerCreador(rol).CrearRol();
        }
    }
}
