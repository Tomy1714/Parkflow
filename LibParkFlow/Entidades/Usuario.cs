using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibParkFlow.Entidades
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(20)]
        public string Documento { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Correo { get; set; }

        [Required]
        [MaxLength(20)]
        public string CodigoOperativo { get; set; } = string.Empty; // AD-1001, OP-2094, CJ-3310, LV-4125, CL-5001

        [Required]
        [MaxLength(100)]
        public string Contrasena { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual List<AsignacionRol> Asignaciones { get; set; } = new();

        public virtual List<Vehiculo> Vehiculos { get; set; } = new();

        public bool Autenticar(string clave)
        {
            return Activo && Contrasena == clave;
        }

        public AsignacionRol? AsignacionVigenteEn(DateTime ahora)
        {
            var horaActual = ahora.TimeOfDay;
            return Asignaciones.FirstOrDefault(a => a.Activo &&
                (a.HoraInicio <= a.HoraFin
                    ? (horaActual >= a.HoraInicio && horaActual <= a.HoraFin)
                    : (horaActual >= a.HoraInicio || horaActual <= a.HoraFin)));
        }
    }
}
