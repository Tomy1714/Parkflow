using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibParkFlow.Entidades
{
    [Table("Roles")]
    public class RolEntidad
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRol { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreRol { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string TipoRol { get; set; } = string.Empty; // Enum RolUsuario as string

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }

    /// <summary>
    /// Abstracción base para la jerarquía de roles (GoF Factory Method & Modelo C4 Nivel 4)
    /// </summary>
    public abstract class Rol
    {
        public abstract RolUsuario Tipo { get; }
        public abstract string Nombre { get; }
        public abstract List<string> OperacionesPermitidas();
    }

    public class RolAdministrador : Rol
    {
        public override RolUsuario Tipo => RolUsuario.ADMINISTRADOR;
        public override string Nombre => "Administrador";

        public override List<string> OperacionesPermitidas() => new()
        {
            "dashboard", "ingreso", "reservas", "lavado", "caja",
            "consultarIngresosDiarios", "realizarArqueoCaja", "generarReportesFinancieros",
            "gestionarCeldas", "consultarAuditoria"
        };
    }

    public class RolOperador : Rol
    {
        public override RolUsuario Tipo => RolUsuario.OPERADOR;
        public override string Nombre => "Operador";

        public override List<string> OperacionesPermitidas() => new()
        {
            "dashboard", "ingreso", "reservas", "lavado",
            "registrarIngresoVehiculo", "asignarCelda", "gestionarReservas"
        };
    }

    public class RolCajero : Rol
    {
        public override RolUsuario Tipo => RolUsuario.CAJERO;
        public override string Nombre => "Cajero";

        public override List<string> OperacionesPermitidas() => new()
        {
            "dashboard", "caja",
            "procesarCobro", "emitirComprobante", "autorizarSalida"
        };
    }

    public class RolLavador : Rol
    {
        public override RolUsuario Tipo => RolUsuario.LAVADOR;
        public override string Nombre => "Lavador";

        public override List<string> OperacionesPermitidas() => new()
        {
            "lavado",
            "actualizarEstadoLavado"
        };
    }

    public class RolCliente : Rol
    {
        public override RolUsuario Tipo => RolUsuario.CLIENTE;
        public override string Nombre => "Cliente";

        public override List<string> OperacionesPermitidas() => new()
        {
            "c-panel", "c-reservas", "c-lavado", "c-cuenta",
            "crearReserva", "cancelarReserva", "solicitarLavado",
            "consultarCuenta", "pagarEnLinea"
        };
    }
}
