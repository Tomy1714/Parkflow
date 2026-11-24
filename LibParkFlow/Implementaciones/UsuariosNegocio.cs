using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones.Patrones.FactoryMethodRoles;
using LibParkFlow.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibParkFlow.Implementaciones
{
    public class UsuariosNegocio : IUsuariosNegocio
    {
        private readonly IConexion _conexion;

        public UsuariosNegocio(IConexion? conexion = null)
        {
            _conexion = conexion ?? new Conexion();
        }

        public List<Usuario> Consultar()
        {
            return _conexion.Usuarios!
                .Include(u => u.Asignaciones)
                    .ThenInclude(a => a.Rol)
                .Include(u => u.Vehiculos)
                .ToList();
        }

        public Usuario? BuscarPorId(int id)
        {
            return _conexion.Usuarios!
                .Include(u => u.Asignaciones)
                    .ThenInclude(a => a.Rol)
                .Include(u => u.Vehiculos)
                .FirstOrDefault(u => u.IdUsuario == id);
        }

        public Usuario? BuscarPorCodigo(string codigo)
        {
            var cod = (codigo ?? "").Trim().ToUpper();
            return _conexion.Usuarios!
                .Include(u => u.Asignaciones)
                    .ThenInclude(a => a.Rol)
                .Include(u => u.Vehiculos)
                .FirstOrDefault(u => u.CodigoOperativo.ToUpper() == cod);
        }

        /// <summary>
        /// Valida credenciales y utiliza el patrón Factory Method para instanciar el rol activo según turno
        /// </summary>
        public RespuestaAutenticacion IniciarSesion(string codigo, string contrasena, DateTime? ahora = null)
        {
            var cod = (codigo ?? "").Trim();
            var usuario = BuscarPorCodigo(cod);

            if (usuario == null)
            {
                return new RespuestaAutenticacion
                {
                    Autenticado = false,
                    Mensaje = $"Usuario con código '{codigo}' no encontrado."
                };
            }

            if (!usuario.Autenticar(contrasena))
            {
                return new RespuestaAutenticacion
                {
                    Autenticado = false,
                    Mensaje = "Contraseña incorrecta."
                };
            }

            DateTime fechaActual = ahora ?? DateTime.Now;
            var asignacion = usuario.AsignacionVigenteEn(fechaActual);

            // Si no tiene asignación por hora específica, tomar la primera asignación activa o derivar por código
            RolUsuario tipoRol = RolUsuario.OPERADOR;

            if (asignacion?.Rol != null && Enum.TryParse<RolUsuario>(asignacion.Rol.TipoRol, true, out var rEnum))
            {
                tipoRol = rEnum;
            }
            else if (usuario.Asignaciones.Any())
            {
                var primera = usuario.Asignaciones.First(a => a.Activo);
                if (primera.Rol != null && Enum.TryParse<RolUsuario>(primera.Rol.TipoRol, true, out var rEnum2))
                {
                    tipoRol = rEnum2;
                }
            }
            else
            {
                // Fallback por prefijo de código
                if (cod.StartsWith("AD", StringComparison.OrdinalIgnoreCase)) tipoRol = RolUsuario.ADMINISTRADOR;
                else if (cod.StartsWith("OP", StringComparison.OrdinalIgnoreCase)) tipoRol = RolUsuario.OPERADOR;
                else if (cod.StartsWith("CJ", StringComparison.OrdinalIgnoreCase)) tipoRol = RolUsuario.CAJERO;
                else if (cod.StartsWith("LV", StringComparison.OrdinalIgnoreCase)) tipoRol = RolUsuario.LAVADOR;
                else if (cod.StartsWith("CL", StringComparison.OrdinalIgnoreCase)) tipoRol = RolUsuario.CLIENTE;
            }

            // Aplicación del patrón Factory Method
            Rol rolInstancia = FabricaRolResolutor.CrearRol(tipoRol);

            return new RespuestaAutenticacion
            {
                Autenticado = true,
                Mensaje = "Autenticación exitosa",
                Usuario = usuario,
                RolActivo = rolInstancia,
                OperacionesPermitidas = rolInstancia.OperacionesPermitidas()
            };
        }

        public Usuario Guardar(Usuario entidad)
        {
            if (string.IsNullOrWhiteSpace(entidad.Documento))
                throw new ArgumentException("Debe ingresar el documento.");

            if (string.IsNullOrWhiteSpace(entidad.Nombre))
                throw new ArgumentException("Debe ingresar el nombre.");

            if (string.IsNullOrWhiteSpace(entidad.CodigoOperativo))
                throw new ArgumentException("Debe ingresar el código operativo.");

            if (_conexion.Usuarios!.Any(u => u.CodigoOperativo == entidad.CodigoOperativo && u.IdUsuario != entidad.IdUsuario))
                throw new InvalidOperationException("El código operativo ya está en uso.");

            if (entidad.IdUsuario == 0)
            {
                _conexion.Usuarios!.Add(entidad);
            }
            else
            {
                _conexion.Usuarios!.Update(entidad);
            }

            _conexion.SaveChanges();
            return entidad;
        }
    }
}
