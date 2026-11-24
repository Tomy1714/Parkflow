using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibParkFlow.Entidades
{
    [Table("AuditoriasReserva")]
    public class AuditoriaReserva
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdAuditoria { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        [MaxLength(10)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string NumeroCelda { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Evento { get; set; } = string.Empty; // "Cancelada automáticamente por Sistema"

        [MaxLength(255)]
        public string? Detalles { get; set; }
    }
}
