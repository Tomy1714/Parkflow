using LibParkFlow.Entidades;
using LibParkFlow.Implementaciones.Patrones.ObserverCeldas;
using LibParkFlow.Interfaces;

namespace LibParkFlow.Implementaciones
{
    public class CeldasNegocio : ICeldasNegocio
    {
        private readonly IConexion _conexion;
        private readonly ContadoresOcupacionObservador _contadoresObs = new();

        public CeldasNegocio(IConexion? conexion = null)
        {
            _conexion = conexion ?? new Conexion();
        }

        public List<Celda> Consultar()
        {
            return _conexion.Celdas!.OrderBy(c => c.NumeroCelda).ToList();
        }

        public List<Celda> ConsultarDisponibles()
        {
            return _conexion.Celdas!
                .Where(c => c.Estado == EstadoCelda.DISPONIBLE)
                .OrderBy(c => c.NumeroCelda)
                .ToList();
        }

        public Celda? BuscarPorNumero(string numeroCelda)
        {
            var num = (numeroCelda ?? "").Trim().ToUpper();
            return _conexion.Celdas!.FirstOrDefault(c => c.NumeroCelda.ToUpper() == num);
        }

        public ContadoresCeldasDto ObtenerContadores()
        {
            var celdas = Consultar();
            _contadoresObs.Recalcular(celdas);

            return new ContadoresCeldasDto
            {
                Total = _contadoresObs.TotalCeldas,
                Disponibles = _contadoresObs.CeldasDisponibles,
                Ocupadas = _contadoresObs.CeldasOcupadas,
                Reservadas = _contadoresObs.CeldasReservadas,
                Mantenimiento = _contadoresObs.CeldasMantenimiento
            };
        }

        public Celda CambiarEstado(string numeroCelda, EstadoCelda nuevoEstado)
        {
            var celda = BuscarPorNumero(numeroCelda);
            if (celda == null)
                throw new KeyNotFoundException($"No se encontró la celda {numeroCelda}");

            // Suscribir observador para validar patrón Observer
            var mapaObs = new MapaCeldasObservador();
            celda.Suscribir(mapaObs);

            switch (nuevoEstado)
            {
                case EstadoCelda.DISPONIBLE:
                    celda.Liberar();
                    break;
                case EstadoCelda.OCUPADA:
                    celda.Ocupar();
                    break;
                case EstadoCelda.RESERVADA:
                    celda.Reservar();
                    break;
                case EstadoCelda.MANTENIMIENTO:
                    celda.Mantenimiento();
                    break;
            }

            _conexion.SaveChanges();
            return celda;
        }
    }
}
