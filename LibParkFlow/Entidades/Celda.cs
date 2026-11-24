using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Entidades
{
    [Table("Celdas")]
    public class Celda
    {
        [Key]
        [MaxLength(10)]
        public string NumeroCelda { get; set; } = string.Empty; // e.g., "A-01", "B-05", "C-01"

        [Required]
        [MaxLength(5)]
        public string Zona { get; set; } = "A"; // "A", "B", "C"

        [Required]
        public EstadoCelda Estado { get; set; } = EstadoCelda.DISPONIBLE;

        [MaxLength(20)]
        public string TipoPermitido { get; set; } = "Mixto";

        // Id de registro o reserva vinculada activamente
        public int? RegistroActivoId { get; set; }
        public int? ReservaActivaId { get; set; }

        [NotMapped]
        private readonly List<IObservadorCelda> _observadores = new();

        public void Suscribir(IObservadorCelda observador)
        {
            if (!_observadores.Contains(observador))
            {
                _observadores.Add(observador);
            }
        }

        public void Desuscribir(IObservadorCelda observador)
        {
            _observadores.Remove(observador);
        }

        public void Ocupar(int? registroId = null)
        {
            Estado = EstadoCelda.OCUPADA;
            RegistroActivoId = registroId;
            ReservaActivaId = null;
            Notificar();
        }

        public void Liberar()
        {
            Estado = EstadoCelda.DISPONIBLE;
            RegistroActivoId = null;
            ReservaActivaId = null;
            Notificar();
        }

        public void Reservar(int? reservaId = null)
        {
            Estado = EstadoCelda.RESERVADA;
            ReservaActivaId = reservaId;
            Notificar();
        }

        public void Mantenimiento()
        {
            Estado = EstadoCelda.MANTENIMIENTO;
            Notificar();
        }

        private void Notificar()
        {
            foreach (var obs in _observadores.ToList())
            {
                obs.Actualizar(this);
            }
        }
    }
}
