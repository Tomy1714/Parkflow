using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibParkFlow.Implementaciones.Patrones.StateLavado;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Entidades
{
    [Table("ServiciosLavado")]
    public class ServicioLavado
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdServicio { get; set; }

        [Required]
        public int IdRegistro { get; set; }

        [ForeignKey("IdRegistro")]
        public virtual RegistroParqueadero? Registro { get; set; }

        [Required]
        [MaxLength(10)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TipoLavado { get; set; } = "Lavado General";

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioBase { get; set; } = 25000m;

        [Required]
        public EstadoLavado EstadoLavado { get; set; } = EstadoLavado.PENDIENTE;

        [Required]
        public DateTime FechaHoraSolicitud { get; set; }

        public DateTime? FechaHoraInicio { get; set; }

        public DateTime? FechaHoraFin { get; set; }

        public bool Facturado { get; set; } = false;

        [NotMapped]
        private IEstadoServicioLavado? _estadoActual;

        [NotMapped]
        public IEstadoServicioLavado EstadoActual
        {
            get => _estadoActual ??= FabricaEstadoLavado.ObtenerEstado(EstadoLavado);
            private set
            {
                _estadoActual = value;
                EstadoLavado = value.EstadoEnum;
            }
        }

        public void CambiarEstado(IEstadoServicioLavado nuevoEstado)
        {
            EstadoActual = nuevoEstado;
        }

        public void Avanzar(DateTime? ahora = null)
        {
            EstadoActual.Avanzar(this, ahora ?? DateTime.Now);
        }

        public bool PuedeCobrarse()
        {
            return EstadoActual.PuedeCobrarse();
        }

        public void NotificarListoParaCobro()
        {
            // Disparador a caja / indicador
        }
    }
}
