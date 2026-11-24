using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibParkFlow.Entidades
{
    [Table("AsignacionesRol")]
    public class AsignacionRol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAsignacion { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [Required]
        public int IdRol { get; set; }

        [ForeignKey("IdRol")]
        public virtual RolEntidad? Rol { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; } // e.g. 06:00:00

        [Required]
        public TimeSpan HoraFin { get; set; }    // e.g. 14:00:00

        [Required]
        [MaxLength(50)]
        public string DiasSemana { get; set; } = "TODOS";

        public bool Activo { get; set; } = true;
    }
}
