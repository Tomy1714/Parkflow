using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibParkFlow.Entidades
{
    [Table("Vehiculos")]
    public class Vehiculo
    {
        [Key]
        [MaxLength(10)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string TipoVehiculo { get; set; } = "Automóvil"; // Automóvil, Moto

        [MaxLength(50)]
        public string? Marca { get; set; }

        [MaxLength(50)]
        public string? Modelo { get; set; }

        public int? IdUsuarioPropietario { get; set; }

        [ForeignKey("IdUsuarioPropietario")]
        public virtual Usuario? Propietario { get; set; }

        public virtual List<RegistroParqueadero> HistorialRegistros { get; set; } = new();
        public virtual List<Reserva> HistorialReservas { get; set; } = new();
    }
}
