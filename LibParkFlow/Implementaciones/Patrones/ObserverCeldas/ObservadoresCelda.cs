using LibParkFlow.Entidades;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones.Patrones.ObserverCeldas
{
    public class MapaCeldasObservador : IObservadorCelda
    {
        public string UltimoCambio { get; private set; } = string.Empty;

        public void Actualizar(Celda celda)
        {
            UltimoCambio = $"Celda {celda.NumeroCelda} cambió su estado a {celda.Estado}";
        }
    }

    public class ContadoresOcupacionObservador : IObservadorCelda
    {
        public int TotalCeldas { get; set; }
        public int CeldasDisponibles { get; set; }
        public int CeldasOcupadas { get; set; }
        public int CeldasReservadas { get; set; }
        public int CeldasMantenimiento { get; set; }

        public void Actualizar(Celda celda)
        {
            // Se puede invocar al recalcular o actualizar en tiempo real
        }

        public void Recalcular(IEnumerable<Celda> celdas)
        {
            var list = celdas.ToList();
            TotalCeldas = list.Count;
            CeldasDisponibles = list.Count(c => c.Estado == EstadoCelda.DISPONIBLE);
            CeldasOcupadas = list.Count(c => c.Estado == EstadoCelda.OCUPADA);
            CeldasReservadas = list.Count(c => c.Estado == EstadoCelda.RESERVADA);
            CeldasMantenimiento = list.Count(c => c.Estado == EstadoCelda.MANTENIMIENTO);
        }
    }
}
