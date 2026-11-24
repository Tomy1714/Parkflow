using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibParkFlow.Entidades
{
    [Table("Reservas")]
    public class Reserva
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdReserva { get; set; }

        [Required]
        [MaxLength(10)]
        public string Placa { get; set; } = string.Empty;

        [ForeignKey("Placa")]
        public virtual Vehiculo? Vehiculo { get; set; }

        [Required]
        [MaxLength(10)]
        public string NumeroCelda { get; set; } = string.Empty;

        [ForeignKey("NumeroCelda")]
        public virtual Celda? Celda { get; set; }

        [Required]
        public DateTime FechaHoraReserva { get; set; }

        public int TiempoToleranciaMin { get; set; } = 15; // RN-03

        [Required]
        public EstadoReserva EstadoReserva { get; set; } = EstadoReserva.ACTIVA;

        public DateTime? FechaHoraCancelacion { get; set; }

        public int? IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        public bool ConfirmarIngreso()
        {
            if (EstadoReserva == EstadoReserva.ACTIVA)
            {
                EstadoReserva = EstadoReserva.ATENDIDA;
                return true;
            }
            return false;
        }

        public bool EstaVencida(DateTime ahora)
        {
            if (EstadoReserva != EstadoReserva.ACTIVA) return false;
            var limite = FechaHoraReserva.AddMinutes(TiempoToleranciaMin);
            return ahora > limite;
        }

        public void CancelarPorVencimiento(DateTime ahora)
        {
            EstadoReserva = EstadoReserva.CANCELADA_INASISTENCIA;
            FechaHoraCancelacion = ahora;
        }

        public void CancelarManual(DateTime ahora)
        {
            EstadoReserva = EstadoReserva.CANCELADA_MANUAL;
            FechaHoraCancelacion = ahora;
        }
    }
}
