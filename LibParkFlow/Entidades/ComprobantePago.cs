using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Entidades
{
    [Table("ComprobantesPago")]
    public class ComprobantePago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdFactura { get; set; }

        [Required]
        [MaxLength(20)]
        public string NumeroFactura { get; set; } = string.Empty; // e.g. "FAC-0001"

        [Required]
        public int IdRegistro { get; set; }

        [ForeignKey("IdRegistro")]
        public virtual RegistroParqueadero? Registro { get; set; }

        public int? IdCaja { get; set; }

        [ForeignKey("IdCaja")]
        public virtual Caja? Caja { get; set; }

        [Required]
        public DateTime FechaEmision { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorParqueadero { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorLavado { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPagar { get; set; }

        [Required]
        [MaxLength(20)]
        public string MetodoPago { get; set; } = "Efectivo";

        [Required]
        public CanalPago Canal { get; set; } = CanalPago.CAJA;

        [MaxLength(100)]
        public string? Referencia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorRecibido { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cambio { get; set; }

        public bool ProcesarPago(IMetodoPagoStrategy estrategia, DatosPago? datos)
        {
            var resultado = estrategia.Procesar(TotalPagar, datos);
            if (resultado.Aprobado)
            {
                MetodoPago = estrategia.NombreMetodo;
                Referencia = resultado.Referencia;
                Cambio = resultado.Cambio;
                if (datos != null && datos.ValorRecibido > 0)
                {
                    ValorRecibido = datos.ValorRecibido;
                }
                return true;
            }
            return false;
        }
    }
}
