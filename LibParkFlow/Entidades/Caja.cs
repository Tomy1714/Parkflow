using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibParkFlow.Entidades
{
    [Table("Cajas")]
    public class Caja
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCaja { get; set; }

        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        [Required]
        public DateTime FechaApertura { get; set; }

        public DateTime? FechaCierre { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoInicial { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MontoFinal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRecaudado { get; set; } = 0m;

        [Required]
        public EstadoCaja EstadoCaja { get; set; } = EstadoCaja.ABIERTA;

        public virtual List<ComprobantePago> Comprobantes { get; set; } = new();

        public void RegistrarMovimiento(decimal valor)
        {
            TotalRecaudado += valor;
        }

        public void CerrarCaja()
        {
            EstadoCaja = EstadoCaja.CERRADA;
            FechaCierre = DateTime.Now;
            MontoFinal = MontoInicial + TotalRecaudado;
        }
    }
}
