using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibParkFlow.Entidades
{
    [Table("DetallesServicio")]
    public class DetalleServicio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDetalle { get; set; }

        public int IdRegistro { get; set; }

        [ForeignKey("IdRegistro")]
        public virtual RegistroParqueadero? Registro { get; set; }

        public int IdServicio { get; set; }

        [ForeignKey("IdServicio")]
        public virtual ServicioLavado? Servicio { get; set; }

        public int Cantidad { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}
