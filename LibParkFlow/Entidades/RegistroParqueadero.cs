using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibParkFlow.Entidades
{
    [Table("RegistrosParqueadero")]
    public class RegistroParqueadero
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdRegistro { get; set; }

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
        public DateTime FechaHoraEntrada { get; set; }

        public DateTime? FechaHoraSalida { get; set; }

        public DateTime? FechaHoraPagado { get; set; }

        public int? MinutosEstadia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoParqueadero { get; set; }

        [Required]
        public EstadoRegistro EstadoRegistro { get; set; } = EstadoRegistro.ACTIVO;

        public int? IdReserva { get; set; }

        // Relaciones
        public virtual List<ServicioLavado> ServiciosLavado { get; set; } = new();
        public virtual ComprobantePago? Comprobante { get; set; }

        public const decimal TarifaHora = 3000m;
        public const decimal TarifaFraccion = TarifaHora / 4m; // 750
        public const int MinutosFraccion = 15;
        public const int UmbralFraccion = 5;

        public int CalcularTiempoEstadia(DateTime? hasta = null)
        {
            var fin = hasta ?? FechaHoraSalida ?? DateTime.Now;
            var diferencia = fin - FechaHoraEntrada;
            return Math.Max(0, (int)diferencia.TotalMinutes);
        }

        /// <summary>
        /// Aplica la regla RN-01: cobro por hora o fracción de 15 min;
        /// desde 5 min transcurridos se cobra la fracción completa.
        /// </summary>
        public (int minutos, int horas, int fracciones, decimal tarifa) CalcularTarifaParqueadero(DateTime? hasta = null)
        {
            int minutos = CalcularTiempoEstadia(hasta);
            int horas = minutos / 60;
            int resto = minutos % 60;
            int fracciones = 0;

            if (resto >= UmbralFraccion)
            {
                fracciones = (int)Math.Ceiling((double)resto / MinutosFraccion);
            }

            decimal total = (horas * TarifaHora) + (fracciones * TarifaFraccion);
            return (minutos, horas, fracciones, total);
        }

        public void MarcarPagado(DateTime? fechaPago = null)
        {
            EstadoRegistro = EstadoRegistro.PAGADO;
            FechaHoraPagado = fechaPago ?? DateTime.Now;
        }

        public void Finalizar(DateTime? fechaSalida = null)
        {
            EstadoRegistro = EstadoRegistro.FINALIZADO;
            FechaHoraSalida = fechaSalida ?? DateTime.Now;
            MinutosEstadia = CalcularTiempoEstadia(FechaHoraSalida);
        }

        public bool EstaPagado() => EstadoRegistro == EstadoRegistro.PAGADO || EstadoRegistro == EstadoRegistro.FINALIZADO;
    }
}
